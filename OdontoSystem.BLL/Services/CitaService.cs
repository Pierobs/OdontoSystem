using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using OdontoSystem.DAL.Context;
using OdontoSystem.DAL.Repositories;
using OdontoSystem.Entities;

namespace OdontoSystem.BLL.Services
{
    public class CitaService
    {
        public IEnumerable<Cita> Listar()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Citas
                          .Include(c => c.Paciente)
                          .Include(c => c.Odontologo)
                          .OrderByDescending(c => c.FechaCita)
                          .ThenByDescending(c => c.HoraCita)
                          .ToList();
            }
        }

        public Cita ObtenerPorId(int id)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Citas
                          .Include(c => c.Paciente)
                          .Include(c => c.Odontologo)
                          .FirstOrDefault(c => c.IdCita == id);
            }
        }

        public IEnumerable<HistorialEstadoCita> ObtenerHistorial(int idCita)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.HistorialEstadosCita
                          .Include(h => h.Usuario)
                          .Where(h => h.IdCita == idCita)
                          .OrderByDescending(h => h.FechaCambio)
                          .ToList();
            }
        }

      

        public void Cancelar(int id, string motivo, int? idUsuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("Debe ingresar un motivo para cancelar la cita");

            using (var ctx = new OdontoContext())
            {
                var cita = ctx.Citas.FirstOrDefault(c => c.IdCita == id);
                if (cita == null)
                    throw new InvalidOperationException("Cita no encontrada");

                if (cita.Estado == "Cancelada")
                    throw new InvalidOperationException("La cita ya fue cancelada");

                if (cita.Estado == "Atendida")
                    throw new InvalidOperationException("No se puede cancelar una cita ya atendida");

                string estadoAnterior = cita.Estado;
                cita.Estado = "Cancelada";
                cita.Motivo = motivo.Trim();
                cita.FechaModificacion = DateTime.Now;

                RegistrarHistorial(ctx, id, estadoAnterior, "Cancelada", motivo, idUsuario);
                ctx.SaveChanges();
            }
        }

        public void Agendar(Cita cita)
        {
            using (var ctx = new OdontoContext())
            {
                // Validar fecha/hora completa
                ValidarFechaHoraCita(cita.FechaCita, cita.HoraCita);

                // Validar capacidad global (máx 2 citas por slot en todo el consultorio)
                ValidarCapacidadSlot(ctx, cita.FechaCita, cita.HoraCita, idCitaExcluir: null);

                // Validar que el odontólogo no tenga otra cita en ese slot
                ValidarOdontologoDisponible(ctx, cita.IdOdontologo, cita.FechaCita, cita.HoraCita, idCitaExcluir: null);

                cita.Estado = "Pendiente";
                cita.FechaRegistro = DateTime.Now;
                ctx.Citas.Add(cita);
                ctx.SaveChanges();

                // Registrar en historial
                RegistrarHistorial(ctx, cita.IdCita, null, "Pendiente", "Cita creada", null);
                ctx.SaveChanges();
            }
        }

        public void Reprogramar(int id, DateTime nuevaFecha, TimeSpan nuevaHora, string motivo, int? idUsuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("Debe ingresar un motivo para reprogramar la cita");

            using (var ctx = new OdontoContext())
            {
                var cita = ctx.Citas.FirstOrDefault(c => c.IdCita == id);
                if (cita == null)
                    throw new InvalidOperationException("Cita no encontrada");

                if (cita.Estado != "Pendiente")
                    throw new InvalidOperationException(
                        $"Solo se pueden reprogramar citas pendientes (estado actual: {cita.Estado})");

                // Validar nueva fecha/hora
                ValidarFechaHoraCita(nuevaFecha, nuevaHora);

                // Validar capacidad y odontólogo, excluyendo esta misma cita
                ValidarCapacidadSlot(ctx, nuevaFecha, nuevaHora, idCitaExcluir: id);
                ValidarOdontologoDisponible(ctx, cita.IdOdontologo, nuevaFecha, nuevaHora, idCitaExcluir: id);

                string motivoCompleto = $"Reprogramada de {cita.FechaCita:dd/MM/yyyy} {cita.HoraCita:hh\\:mm} → " +
                                        $"{nuevaFecha:dd/MM/yyyy} {nuevaHora:hh\\:mm}. Motivo: {motivo.Trim()}";

                cita.FechaCita = nuevaFecha.Date;
                cita.HoraCita = nuevaHora;
                cita.Motivo = motivo.Trim();
                cita.FechaModificacion = DateTime.Now;

                RegistrarHistorial(ctx, id, "Pendiente", "Reprogramada", motivoCompleto, idUsuario);
                ctx.SaveChanges();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  VALIDACIONES PRIVADAS
        // ─────────────────────────────────────────────────────────────

        private void ValidarFechaHoraCita(DateTime fecha, TimeSpan hora)
        {
            var fechaHora = fecha.Date + hora;

            if (fechaHora < DateTime.Now)
                throw new InvalidOperationException("No se puede agendar una cita en una fecha u hora pasada");

            if (!HorarioAtencion.EsSlotValido(hora))
                throw new InvalidOperationException(
                    $"La hora seleccionada no es válida. Horario de atención: {HorarioAtencion.DescripcionHorario}");
        }

        private void ValidarCapacidadSlot(OdontoContext ctx, DateTime fecha, TimeSpan hora, int? idCitaExcluir)
        {
            int citasEnSlot = ctx.Citas.Count(c =>
                c.FechaCita == fecha.Date &&
                c.HoraCita == hora &&
                c.Estado != "Cancelada" &&
                (idCitaExcluir == null || c.IdCita != idCitaExcluir));

            if (citasEnSlot >= HorarioAtencion.CapacidadPorSlot)
                throw new InvalidOperationException(
                    $"Horario no disponible — ya hay {HorarioAtencion.CapacidadPorSlot} citas agendadas en ese slot. " +
                    "Seleccione otra hora.");
        }

        private void ValidarOdontologoDisponible(OdontoContext ctx, int idOdontologo, DateTime fecha,
                                                  TimeSpan hora, int? idCitaExcluir)
        {
            bool ocupado = ctx.Citas.Any(c =>
                c.IdOdontologo == idOdontologo &&
                c.FechaCita == fecha.Date &&
                c.HoraCita == hora &&
                c.Estado != "Cancelada" &&
                (idCitaExcluir == null || c.IdCita != idCitaExcluir));

            if (ocupado)
                throw new InvalidOperationException("El odontólogo ya tiene una cita en ese horario");
        }

        // ─────────────────────────────────────────────────────────────
        //  MÉTODOS DE CONSULTA
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Devuelve los slots disponibles para una fecha específica, marcando cuáles están ocupados.
        /// </summary>
        public IEnumerable<object> ObtenerSlotsDeFecha(DateTime fecha)
        {
            using (var ctx = new OdontoContext())
            {
                var slots = HorarioAtencion.ObtenerSlotsDisponibles();
                var citasDelDia = ctx.Citas
                    .Where(c => c.FechaCita == fecha.Date && c.Estado != "Cancelada")
                    .ToList();

                return slots.Select(s => new
                {
                    hora = s.ToString(@"hh\:mm"),
                    ocupados = citasDelDia.Count(c => c.HoraCita == s),
                    disponible = citasDelDia.Count(c => c.HoraCita == s) < HorarioAtencion.CapacidadPorSlot
                });
            }
        }

        private void RegistrarHistorial(OdontoContext ctx, int idCita, string estadoAnterior,
                                         string estadoNuevo, string motivo, int? idUsuario)
        {
            ctx.HistorialEstadosCita.Add(new HistorialEstadoCita
            {
                IdCita = idCita,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                Motivo = motivo,
                FechaCambio = DateTime.Now,
                IdUsuario = idUsuario
            });
        }

        public IEnumerable<Paciente> ListarPacientesActivos()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Pacientes
                          .Where(p => p.Estado == "A")
                          .OrderBy(p => p.ApellidoPaterno)
                          .ToList();
            }
        }

        public IEnumerable<Usuario> ListarOdontologosActivos()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Usuarios
                          .Where(u => u.Estado == "A" && u.Rol.Descripcion == "Odontologo")
                          .ToList();
            }
        }
    }
}