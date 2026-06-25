using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OdontoSystem.BLL.Services
{
    public class EvolucionService
    {
        /// <summary>
        /// Registra una nueva evolución para un tratamiento específico del plan.
        /// Opcionalmente actualiza el estado del tratamiento en el plan.
        /// </summary>
        public int Registrar(int idPlan, int idTratamiento, int idPlanDetalle,
                              int idOdontologo, string descripcion,
                              DateTime fechaEvolucion, int? idCita,
                              string nuevoEstadoTratamiento)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new InvalidOperationException("La descripción de la evolución es obligatoria");

            using (var ctx = new OdontoContext())
            {
                var plan = ctx.PlanesTratamiento
                              .Include(p => p.Detalles)
                              .FirstOrDefault(p => p.IdPlan == idPlan);

                if (plan == null)
                    throw new InvalidOperationException("Plan no encontrado");

                if (plan.Estado == "Cancelado")
                    throw new InvalidOperationException("No se puede registrar evolución en un plan cancelado");

                // Buscar por IdPlanDetalle específico (no por IdTratamiento)
                var detalle = plan.Detalles.FirstOrDefault(d => d.IdPlanDetalle == idPlanDetalle);
                if (detalle == null)
                    throw new InvalidOperationException("El tratamiento seleccionado no pertenece a este plan");

                var evolucion = new Evolucion
                {
                    IdPlan = idPlan,
                    IdTratamiento = idTratamiento,
                    IdPlanDetalle = idPlanDetalle,
                    IdOdontologo = idOdontologo,
                    Descripcion = descripcion.Trim(),
                    FechaEvolucion = fechaEvolucion,
                    IdCita = idCita
                };
                ctx.Evoluciones.Add(evolucion);

                // Actualizar estado del tratamiento específico
                if (!string.IsNullOrWhiteSpace(nuevoEstadoTratamiento)
                    && nuevoEstadoTratamiento != detalle.EstadoTratamiento)
                {
                    detalle.EstadoTratamiento = nuevoEstadoTratamiento;
                }

                ctx.SaveChanges();
                return evolucion.IdEvolucion;
            }
        }

        public class EvolucionDto
        {
            public int IdEvolucion { get; set; }
            public int IdPlanDetalle { get; set; }
            public int IdTratamiento { get; set; }
            public DateTime FechaEvolucion { get; set; }
            public string Descripcion { get; set; }
            public string NombreTratamiento { get; set; }
            public string ApellidoOdontologo { get; set; }
            public string NombreOdontologo { get; set; }
            public int? IdCita { get; set; }
            public byte? NumeroPieza { get; set; }
            public string Superficie { get; set; }
        }

        public IEnumerable<EvolucionDto> ListarPorPlan(int idPlan)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Evoluciones
                          .Where(e => e.IdPlan == idPlan)
                          .Select(e => new EvolucionDto
                          {
                              IdEvolucion = e.IdEvolucion,
                              IdPlanDetalle = e.IdPlanDetalle ?? 0,
                              IdTratamiento = e.IdTratamiento,
                              FechaEvolucion = e.FechaEvolucion,
                              Descripcion = e.Descripcion,
                              NombreTratamiento = e.Tratamiento.Nombre,
                              ApellidoOdontologo = e.Odontologo.ApellidoPaterno,
                              NombreOdontologo = e.Odontologo.Nombres,
                              IdCita = e.IdCita,
                              NumeroPieza = e.PlanDetalle.NumeroPieza,
                              Superficie = e.PlanDetalle.Superficie
                          })
                          .OrderByDescending(e => e.FechaEvolucion)
                          .ToList();
            }
        }

        /// <summary>
        /// Lista las citas atendidas de un paciente para vincular opcionalmente a la evolución.
        /// </summary>
        public IEnumerable<Cita> ListarCitasAtendidas(int idPaciente)
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                return ctx.Citas
                          .Where(c => c.IdPaciente == idPaciente && c.Estado == "Atendida")
                          .OrderByDescending(c => c.FechaCita)
                          .ToList();
            }
        }
    }
}