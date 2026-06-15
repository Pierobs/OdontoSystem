using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OdontoSystem.BLL.Services
{
    public class PlanTratamientoService
    {
        public IEnumerable<PlanTratamiento> Listar()
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                return ctx.PlanesTratamiento
                          .Include(p => p.Paciente)
                          .Include(p => p.Detalles)
                          .OrderByDescending(p => p.FechaCreacion)
                          .ToList();
            }
        }

        public IEnumerable<PlanTratamiento> ListarPorPaciente(int idPaciente)
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                return ctx.PlanesTratamiento
                          .Include(p => p.Detalles.Select(d => d.Tratamiento))
                          .Include(p => p.Paciente)
                          .Where(p => p.IdPaciente == idPaciente)
                          .OrderByDescending(p => p.FechaCreacion)
                          .ToList();
            }
        }

        public PlanTratamiento ObtenerPorId(int id)
        {
            using (var ctx = new OdontoContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                ctx.Configuration.ProxyCreationEnabled = false;

                return ctx.PlanesTratamiento
                          .Include(p => p.Paciente)
                          .Include(p => p.Detalles.Select(d => d.Tratamiento))
                          .Include(p => p.Pagos)
                          .FirstOrDefault(p => p.IdPlan == id);
            }
        }

        public int Crear(int idPaciente, List<PlanDetalle> detalles)
        {
            if (detalles == null || !detalles.Any())
                throw new InvalidOperationException("Debe agregar al menos un tratamiento al plan");

            using (var ctx = new OdontoContext())
            {
                var paciente = ctx.Pacientes.FirstOrDefault(p => p.IdPaciente == idPaciente);
                if (paciente == null)
                    throw new InvalidOperationException("Paciente no encontrado");
                if (paciente.Estado != "A")
                    throw new InvalidOperationException("No se puede crear un plan para un paciente inactivo");

                decimal montoTotal = 0;
                foreach (var d in detalles)
                {
                    if (d.Cantidad <= 0)
                        throw new InvalidOperationException("La cantidad de cada tratamiento debe ser mayor a 0");
                    if (d.PrecioUnitario < 0)
                        throw new InvalidOperationException("El precio no puede ser negativo");

                    var trat = ctx.CatalogoTratamientos.FirstOrDefault(t => t.IdTratamiento == d.IdTratamiento);
                    if (trat == null || trat.Estado != "A")
                        throw new InvalidOperationException($"El tratamiento ID {d.IdTratamiento} no existe o está inactivo");

                    montoTotal += d.Cantidad * d.PrecioUnitario;
                }

                var plan = new PlanTratamiento
                {
                    IdPaciente = idPaciente,
                    FechaCreacion = DateTime.Now,
                    MontoTotal = montoTotal,
                    MontoAbonado = 0,
                    Estado = "Activo"
                };

                ctx.PlanesTratamiento.Add(plan);
                ctx.SaveChanges();

                foreach (var d in detalles)
                {
                    d.IdPlan = plan.IdPlan;
                    ctx.PlanDetalles.Add(d);
                }

                ctx.SaveChanges();
                return plan.IdPlan;
            }
        }

        public void AgregarDetalle(int idPlan, PlanDetalle detalle)
        {
            using (var ctx = new OdontoContext())
            {
                var plan = ctx.PlanesTratamiento.FirstOrDefault(p => p.IdPlan == idPlan);
                if (plan == null)
                    throw new InvalidOperationException("Plan no encontrado");
                if (plan.Estado != "Activo")
                    throw new InvalidOperationException("Solo se pueden modificar planes activos");

                if (detalle.Cantidad <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0");

                var trat = ctx.CatalogoTratamientos.FirstOrDefault(t => t.IdTratamiento == detalle.IdTratamiento);
                if (trat == null || trat.Estado != "A")
                    throw new InvalidOperationException("Tratamiento inválido");

                detalle.IdPlan = idPlan;
                ctx.PlanDetalles.Add(detalle);

                plan.MontoTotal += detalle.Cantidad * detalle.PrecioUnitario;
                ctx.SaveChanges();
            }
        }

        public void QuitarDetalle(int idPlanDetalle)
        {
            using (var ctx = new OdontoContext())
            {
                var detalle = ctx.PlanDetalles
                                 .Include(d => d.Plan)
                                 .FirstOrDefault(d => d.IdPlanDetalle == idPlanDetalle);

                if (detalle == null)
                    throw new InvalidOperationException("Detalle no encontrado");
                if (detalle.Plan.Estado != "Activo")
                    throw new InvalidOperationException("Solo se pueden modificar planes activos");

                int idPlan = detalle.IdPlan;
                decimal montoARestar = detalle.Cantidad * detalle.PrecioUnitario;

                ctx.PlanDetalles.Remove(detalle);

                var plan = ctx.PlanesTratamiento.First(p => p.IdPlan == idPlan);
                plan.MontoTotal -= montoARestar;

                if (plan.MontoTotal < 0) plan.MontoTotal = 0;

                ctx.SaveChanges();

                bool tieneDetalles = ctx.PlanDetalles.Any(d => d.IdPlan == idPlan);
                if (!tieneDetalles)
                    throw new InvalidOperationException(
                        "No se puede dejar el plan sin tratamientos. Si desea eliminarlo, cancele el plan completo.");
            }
        }

        public void Cancelar(int id)
        {
            using (var ctx = new OdontoContext())
            {
                var plan = ctx.PlanesTratamiento.FirstOrDefault(p => p.IdPlan == id);
                if (plan == null)
                    throw new InvalidOperationException("Plan no encontrado");

                if (plan.Estado == "Cancelado")
                    throw new InvalidOperationException("El plan ya fue cancelado");
                if (plan.Estado == "Pagado")
                    throw new InvalidOperationException("No se puede cancelar un plan completamente pagado");
                if (plan.MontoAbonado > 0)
                    throw new InvalidOperationException(
                        $"No se puede cancelar un plan con abonos registrados (S/. {plan.MontoAbonado:N2}). " +
                        "Debe gestionar la devolución antes.");

                plan.Estado = "Cancelado";
                ctx.SaveChanges();
            }
        }

        public void Cerrar(int id)
        {
            using (var ctx = new OdontoContext())
            {
                var plan = ctx.PlanesTratamiento.FirstOrDefault(p => p.IdPlan == id);
                if (plan == null)
                    throw new InvalidOperationException("Plan no encontrado");

                if (plan.Estado != "Activo")
                    throw new InvalidOperationException("Solo planes activos pueden cerrarse");

                if (plan.Saldo > 0)
                    throw new InvalidOperationException(
                        $"No se puede cerrar el plan. Saldo pendiente: S/. {plan.Saldo:N2}");

                plan.Estado = "Pagado";
                ctx.SaveChanges();
            }
        }
    }
}