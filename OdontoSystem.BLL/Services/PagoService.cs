using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OdontoSystem.BLL.Services
{
    public class PagoService
    {
        public static readonly string[] MetodosPago =
        {
            "Efectivo", "Transferencia", "Tarjeta", "Yape", "Plin"
        };

        /// <summary>
        /// Registra un pago contra un plan de tratamiento.
        /// Actualiza MontoAbonado del plan automáticamente.
        /// </summary>
        public int Registrar(int idPlan, decimal monto, string metodoPago, int idUsuarioRegistro)
        {
            if (monto <= 0)
                throw new InvalidOperationException("El monto del pago debe ser mayor a cero");

            if (!MetodosPago.Contains(metodoPago))
                throw new InvalidOperationException($"Método de pago inválido: {metodoPago}");

            using (var ctx = new OdontoContext())
            {
                var plan = ctx.PlanesTratamiento
                              .FirstOrDefault(p => p.IdPlan == idPlan);

                if (plan == null)
                    throw new InvalidOperationException("Plan de tratamiento no encontrado");

                if (plan.Estado == "Cancelado")
                    throw new InvalidOperationException("No se puede registrar un pago en un plan cancelado");

                if (monto > plan.Saldo)
                    throw new InvalidOperationException(
                        $"El monto (S/. {monto:N2}) supera el saldo pendiente (S/. {plan.Saldo:N2})");

                var pago = new Pago
                {
                    IdPlan = idPlan,
                    Monto = monto,
                    MetodoPago = metodoPago,
                    Fecha = DateTime.Now,
                    IdUsuarioRegistro = idUsuarioRegistro
                };
                ctx.Pagos.Add(pago);

                // Actualizar monto abonado del plan
                plan.MontoAbonado += monto;

                // Si el saldo queda en 0, cerrar el plan automáticamente
                if (plan.MontoAbonado >= plan.MontoTotal)
                {
                    plan.Estado = "Pagado";
                }

                ctx.SaveChanges();
                return pago.IdPago;
            }
        }

        /// <summary>
        /// Lista todos los pagos de un plan ordenados por fecha.
        /// </summary>
        public class PagoDto
        {
            public int IdPago { get; set; }
            public DateTime Fecha { get; set; }
            public decimal Monto { get; set; }
            public string MetodoPago { get; set; }
            public string NombresUsuario { get; set; }
            public string ApellidoUsuario { get; set; }
        }

        public IEnumerable<PagoDto> ListarPorPlan(int idPlan)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Pagos
                          .Where(p => p.IdPlan == idPlan)
                          .Select(p => new PagoDto
                          {
                              IdPago = p.IdPago,
                              Fecha = p.Fecha,
                              Monto = p.Monto,
                              MetodoPago = p.MetodoPago,
                              NombresUsuario = p.UsuarioRegistro.Nombres,
                              ApellidoUsuario = p.UsuarioRegistro.ApellidoPaterno
                          })
                          .OrderByDescending(p => p.Fecha)
                          .ToList();
            }
        }
    }
}