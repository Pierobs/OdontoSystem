using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OdontoSystem.BLL.Services
{
    public class DisponibilidadService
    {
        /// <summary>
        /// Lista los bloques de disponibilidad de un odontólogo en un rango de fechas.
        /// </summary>
        public IEnumerable<DisponibilidadOdontologo> Listar(int idOdontologo, DateTime desde, DateTime hasta)
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                return ctx.Disponibilidades
                          .Where(d => d.IdOdontologo == idOdontologo
                                   && d.Estado == "A"
                                   && d.Fecha >= desde.Date
                                   && d.Fecha <= hasta.Date)
                          .OrderBy(d => d.Fecha)
                          .ThenBy(d => d.HoraInicio)
                          .ToList();
            }
        }

        /// <summary>
        /// Crea un nuevo bloque de disponibilidad. Valida solapamiento con bloques existentes.
        /// </summary>
        public int Crear(int idOdontologo, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            if (horaInicio >= horaFin)
                throw new InvalidOperationException("La hora de inicio debe ser anterior a la hora de fin");

            if (fecha.Date < DateTime.Today)
                throw new InvalidOperationException("No se puede crear disponibilidad en fechas pasadas");

            using (var ctx = new OdontoContext())
            {
                // Validar que no haya solapamiento con otro bloque activo del mismo odontólogo
                bool seSolapa = ctx.Disponibilidades.Any(d =>
                    d.IdOdontologo == idOdontologo &&
                    d.Estado == "A" &&
                    d.Fecha == fecha.Date &&
                    d.HoraInicio < horaFin &&
                    d.HoraFin > horaInicio);

                if (seSolapa)
                    throw new InvalidOperationException(
                        "Ya existe un bloque de disponibilidad que se solapa con este horario");

                var disp = new DisponibilidadOdontologo
                {
                    IdOdontologo = idOdontologo,
                    Fecha = fecha.Date,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                    Estado = "A",
                    FechaRegistro = DateTime.Now
                };

                ctx.Disponibilidades.Add(disp);
                ctx.SaveChanges();
                return disp.IdDisponibilidad;
            }
        }

        /// <summary>
        /// Elimina (soft delete) un bloque de disponibilidad.
        /// </summary>
        public void Eliminar(int idDisponibilidad, int idOdontologoRequester, bool esAdmin)
        {
            using (var ctx = new OdontoContext())
            {
                var disp = ctx.Disponibilidades.FirstOrDefault(d => d.IdDisponibilidad == idDisponibilidad);
                if (disp == null)
                    throw new InvalidOperationException("Bloque no encontrado");

                // Solo el dueño o un admin puede eliminar
                if (!esAdmin && disp.IdOdontologo != idOdontologoRequester)
                    throw new InvalidOperationException("No tiene permisos para eliminar este bloque");

                // Validar que no haya citas pendientes en ese bloque
                bool tieneCitas = ctx.Citas.Any(c =>
                    c.IdOdontologo == disp.IdOdontologo &&
                    c.FechaCita == disp.Fecha &&
                    c.HoraCita >= disp.HoraInicio &&
                    c.HoraCita < disp.HoraFin &&
                    c.Estado == "Pendiente");

                if (tieneCitas)
                    throw new InvalidOperationException(
                        "No se puede eliminar el bloque: hay citas pendientes en ese horario");

                disp.Estado = "I";
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Replica los bloques de una semana base en las próximas N semanas.
        /// </summary>
        public int ReplicarSemana(int idOdontologo, DateTime fechaInicioSemana, int semanasAReplicar)
        {
            if (semanasAReplicar < 1 || semanasAReplicar > 12)
                throw new InvalidOperationException("Debe replicar entre 1 y 12 semanas");

            using (var ctx = new OdontoContext())
            {
                var fechaFinSemana = fechaInicioSemana.AddDays(6);
                var bloquesBase = ctx.Disponibilidades
                    .Where(d => d.IdOdontologo == idOdontologo
                             && d.Estado == "A"
                             && d.Fecha >= fechaInicioSemana.Date
                             && d.Fecha <= fechaFinSemana.Date)
                    .ToList();

                if (!bloquesBase.Any())
                    throw new InvalidOperationException("No hay bloques en la semana base para replicar");

                int replicados = 0;
                for (int s = 1; s <= semanasAReplicar; s++)
                {
                    foreach (var b in bloquesBase)
                    {
                        var nuevaFecha = b.Fecha.AddDays(7 * s);

                        // Saltar si ya existe un bloque solapado
                        bool yaExiste = ctx.Disponibilidades.Any(d =>
                            d.IdOdontologo == idOdontologo &&
                            d.Estado == "A" &&
                            d.Fecha == nuevaFecha &&
                            d.HoraInicio < b.HoraFin &&
                            d.HoraFin > b.HoraInicio);

                        if (yaExiste) continue;

                        ctx.Disponibilidades.Add(new DisponibilidadOdontologo
                        {
                            IdOdontologo = idOdontologo,
                            Fecha = nuevaFecha,
                            HoraInicio = b.HoraInicio,
                            HoraFin = b.HoraFin,
                            Estado = "A",
                            FechaRegistro = DateTime.Now
                        });
                        replicados++;
                    }
                }

                ctx.SaveChanges();
                return replicados;
            }
        }

        /// <summary>
        /// Verifica si un odontólogo tiene disponibilidad en un slot específico.
        /// </summary>
        public bool EstaDisponible(int idOdontologo, DateTime fecha, TimeSpan hora)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Disponibilidades.Any(d =>
                    d.IdOdontologo == idOdontologo &&
                    d.Estado == "A" &&
                    d.Fecha == fecha.Date &&
                    d.HoraInicio <= hora &&
                    d.HoraFin > hora);
            }
        }

        /// <summary>
        /// Devuelve los odontólogos disponibles en una fecha+hora específica.
        /// </summary>
        public IEnumerable<Usuario> OdontologosDisponibles(DateTime fecha, TimeSpan hora)
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                var idsDisponibles = ctx.Disponibilidades
                    .Where(d => d.Estado == "A" &&
                                d.Fecha == fecha.Date &&
                                d.HoraInicio <= hora &&
                                d.HoraFin > hora)
                    .Select(d => d.IdOdontologo)
                    .Distinct()
                    .ToList();

                return ctx.Usuarios
                    .Where(u => idsDisponibles.Contains(u.IdUsuario) &&
                                u.Estado == "A" &&
                                u.Rol.Descripcion == "Odontologo")
                    .ToList();
            }
        }

        /// <summary>
        /// Devuelve las horas (slots de 30 min) disponibles de un odontólogo en una fecha.
        /// Considera sus bloques de disponibilidad y las citas ya agendadas.
        /// </summary>
        public IEnumerable<TimeSpan> SlotsDisponiblesOdontologo(int idOdontologo, DateTime fecha)
        {
            using (var ctx = new OdontoContext())
            {
                var bloques = ctx.Disponibilidades
                    .Where(d => d.IdOdontologo == idOdontologo &&
                                d.Estado == "A" &&
                                d.Fecha == fecha.Date)
                    .ToList();

                if (!bloques.Any()) return new List<TimeSpan>();

                var citasOcupadas = ctx.Citas
                    .Where(c => c.IdOdontologo == idOdontologo &&
                                c.FechaCita == fecha.Date &&
                                c.Estado != "Cancelada")
                    .Select(c => c.HoraCita)
                    .ToList();

                var slots = new List<TimeSpan>();
                foreach (var bloque in bloques)
                {
                    var hora = bloque.HoraInicio;
                    while (hora.Add(TimeSpan.FromMinutes(30)) <= bloque.HoraFin)
                    {
                        if (!citasOcupadas.Contains(hora))
                            slots.Add(hora);
                        hora = hora.Add(TimeSpan.FromMinutes(30));
                    }
                }
                return slots.Distinct().OrderBy(s => s).ToList();
            }
        }

        public IEnumerable<Usuario> ListarOdontologos()
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                return ctx.Usuarios
                    .Include(u => u.Rol)
                    .Where(u => u.Estado == "A" && u.Rol.Descripcion == "Odontologo")
                    .OrderBy(u => u.ApellidoPaterno)
                    .ToList();
            }
        }
    }
}