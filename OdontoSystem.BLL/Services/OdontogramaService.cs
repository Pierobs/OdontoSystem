using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OdontoSystem.BLL.Services
{
    public class OdontogramaService
    {
        // Lista de estados clínicos permitidos
        public static readonly string[] EstadosValidos = {
            "Sano", "Caries", "Curacion", "Extraccion",
            "Corona", "Implante", "Ausente", "Endodoncia", "Fractura", "Perno"
        };

        public static readonly string[] SuperficiesValidas = {
            "Completo", "Oclusal", "Vestibular", "Lingual", "Mesial", "Distal"
        };

        // Lista FDI: 32 piezas
        public static readonly int[] PiezasFDI = {
            18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28,
            48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38
        };

        public Odontograma ObtenerPorCita(int idCita)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Odontogramas
                    .Include(o => o.Paciente)
                    .Include(o => o.Cita.Odontologo)
                    .Include(o => o.DientesEstado)
                    .FirstOrDefault(o => o.IdCita == idCita);
            }
        }

        public Odontograma ObtenerPorId(int idOdontograma)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Odontogramas
                    .Include(o => o.Paciente)
                    .Include(o => o.Paciente.TipoDocumento) // HU-14: necesario en la vista PDF (contexto ya disposed al renderizar)
                    .Include(o => o.Cita.Odontologo)
                    .Include(o => o.DientesEstado)
                    .FirstOrDefault(o => o.IdOdontograma == idOdontograma);
            }
        }

        public IEnumerable<Odontograma> ListarPorPaciente(int idPaciente)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Odontogramas
                    .Include(o => o.Cita.Odontologo)
                    .Include(o => o.DientesEstado)
                    .Where(o => o.IdPaciente == idPaciente)
                    .OrderByDescending(o => o.FechaRegistro)
                    .ToList();
            }
        }

        public int AtenderCitaYCrearOdontograma(int idCita)
        {
            using (var ctx = new OdontoContext())
            {
                var cita = ctx.Citas.FirstOrDefault(c => c.IdCita == idCita);
                if (cita == null)
                    throw new InvalidOperationException("Cita no encontrada");

                if (cita.Estado == "Cancelada")
                    throw new InvalidOperationException("No se puede atender una cita cancelada");

                // Si la cita ya fue atendida, devolver el odontograma de esa cita
                if (cita.Estado == "Atendida")
                {
                    var odExistente = ctx.Odontogramas
                        .Where(o => o.IdCita == idCita)
                        .OrderByDescending(o => o.FechaRegistro)
                        .FirstOrDefault();
                    if (odExistente != null) return odExistente.IdOdontograma;
                }

                if (cita.Estado != "Pendiente")
                    throw new InvalidOperationException(
                        $"Estado inválido para atender: {cita.Estado}");

                // Buscar odontograma previo del paciente (de cualquier cita anterior)
                var odontogramaPrevio = ctx.Odontogramas
                    .Where(o => o.IdPaciente == cita.IdPaciente)
                    .OrderByDescending(o => o.FechaRegistro)
                    .FirstOrDefault();

                // Marcar cita como atendida
                cita.Estado = "Atendida";
                cita.FechaModificacion = DateTime.Now;

                int idOdontogramaFinal;

                if (odontogramaPrevio != null)
                {
                    // PACIENTE CON HISTORIAL: crear nuevo registro vinculado a esta cita
                    // pero copiar el estado actual de todas las piezas del odontograma previo
                    var nuevoOdontograma = new Odontograma
                    {
                        IdCita = idCita,
                        IdPaciente = cita.IdPaciente,
                        FechaRegistro = DateTime.Now
                    };
                    ctx.Odontogramas.Add(nuevoOdontograma);
                    ctx.SaveChanges();

                    // Copiar el estado actual de cada pieza del odontograma previo
                    var piezasPrevias = ctx.DientesEstado
                        .Where(d => d.IdOdontograma == odontogramaPrevio.IdOdontograma)
                        .ToList();

                    foreach (var pieza in piezasPrevias)
                    {
                        ctx.DientesEstado.Add(new DienteEstado
                        {
                            IdOdontograma = nuevoOdontograma.IdOdontograma,
                            NumeroPieza = pieza.NumeroPieza,
                            Superficie = pieza.Superficie,
                            Estado = pieza.Estado,
                            Observacion = pieza.Observacion,
                            FechaRegistro = DateTime.Now
                        });
                    }

                    idOdontogramaFinal = nuevoOdontograma.IdOdontograma;
                }
                else
                {
                    // PRIMERA CITA: crear odontograma vacío
                    var nuevoOdontograma = new Odontograma
                    {
                        IdCita = idCita,
                        IdPaciente = cita.IdPaciente,
                        FechaRegistro = DateTime.Now
                    };
                    ctx.Odontogramas.Add(nuevoOdontograma);
                    ctx.SaveChanges();
                    idOdontogramaFinal = nuevoOdontograma.IdOdontograma;
                }

                // Registrar historial de la cita
                ctx.HistorialEstadosCita.Add(new HistorialEstadoCita
                {
                    IdCita = idCita,
                    EstadoAnterior = "Pendiente",
                    EstadoNuevo = "Atendida",
                    Motivo = odontogramaPrevio != null
                        ? "Cita atendida — odontograma actualizado desde historial previo"
                        : "Cita atendida — primer odontograma del paciente",
                    FechaCambio = DateTime.Now
                });

                ctx.SaveChanges();
                return idOdontogramaFinal;
            }
        }

        /// <summary>
        /// Registra o actualiza el estado de una pieza dental específica.
        /// </summary>
        public void RegistrarPieza(int idOdontograma, byte numeroPieza, string estado,
                                    string superficie, string observacion)
        {
            if (!EstadosValidos.Contains(estado))
                throw new InvalidOperationException($"Estado inválido: {estado}");
            if (string.IsNullOrWhiteSpace(superficie))
                superficie = "Completo";
            if (!SuperficiesValidas.Contains(superficie))
                throw new InvalidOperationException($"Superficie inválida: {superficie}");
            if (!PiezasFDI.Contains(numeroPieza))
                throw new InvalidOperationException($"Número de pieza FDI inválido: {numeroPieza}");

            using (var ctx = new OdontoContext())
            {
                var odontograma = ctx.Odontogramas.FirstOrDefault(o => o.IdOdontograma == idOdontograma);
                if (odontograma == null)
                    throw new InvalidOperationException("Odontograma no encontrado");

                string estadoAnterior = null;

                var existente = ctx.DientesEstado.FirstOrDefault(d =>
                    d.IdOdontograma == idOdontograma &&
                    d.NumeroPieza == numeroPieza &&
                    d.Superficie == superficie);

                if (existente != null)
                {
                    estadoAnterior = existente.Estado; // guardar para el historial
                    existente.Estado = estado;
                    existente.Observacion = observacion;
                    existente.FechaRegistro = DateTime.Now;
                }
                else
                {
                    ctx.DientesEstado.Add(new DienteEstado
                    {
                        IdOdontograma = idOdontograma,
                        NumeroPieza = numeroPieza,
                        Superficie = superficie,
                        Estado = estado,
                        Observacion = observacion,
                        FechaRegistro = DateTime.Now
                    });
                }

                // Registrar en historial solo si el estado cambió
                if (estadoAnterior != estado)
                {
                    ctx.HistorialDientesEstado.Add(new HistorialDienteEstado
                    {
                        IdPaciente = odontograma.IdPaciente,
                        NumeroPieza = numeroPieza,
                        Superficie = superficie,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = estado,
                        FechaCambio = DateTime.Now,
                        IdCita = odontograma.IdCita,
                        Observacion = observacion
                    });
                }

                ctx.SaveChanges();
            }
        }

        public void EliminarPieza(int idDienteEstado)
        {
            using (var ctx = new OdontoContext())
            {
                var diente = ctx.DientesEstado.FirstOrDefault(d => d.IdDienteEstado == idDienteEstado);
                if (diente == null)
                    throw new InvalidOperationException("Registro no encontrado");
                ctx.DientesEstado.Remove(diente);
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Devuelve el estado de cada pieza como diccionario para que el SVG lo renderice fácil.
        /// </summary>
        public Dictionary<byte, string> ObtenerEstadosPorPieza(int idOdontograma)
        {
            using (var ctx = new OdontoContext())
            {
                // Traer todos los registros a memoria PRIMERO con ToList,
                // luego agrupar y tomar el último por pieza
                var dientes = ctx.DientesEstado
                    .Where(d => d.IdOdontograma == idOdontograma)
                    .ToList()
                    .GroupBy(d => d.NumeroPieza)
                    .Select(g => g.OrderByDescending(d => d.FechaRegistro).First())
                    .ToList();

                return dientes.ToDictionary(d => d.NumeroPieza, d => d.Estado);
            }
        }

        /// <summary>
        /// Devuelve el odontograma más reciente del paciente.
        /// Usado para consultar el estado actual sin necesidad de atender una cita.
        /// </summary>
        public Odontograma ObtenerOdontogramaActualPorPaciente(int idPaciente)
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                return ctx.Odontogramas
                          .Include(o => o.Paciente)
                          .Include(o => o.Cita.Odontologo)
                          .Include(o => o.DientesEstado)
                          .Where(o => o.IdPaciente == idPaciente)
                          .OrderByDescending(o => o.FechaRegistro)
                          .FirstOrDefault();
            }
        }

    }
}