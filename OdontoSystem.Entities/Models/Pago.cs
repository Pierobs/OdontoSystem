using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Pago registrado contra un plan de tratamiento.
    /// Vinculado a HU-08 — Registrar pagos y calcular saldo automático.
    /// </summary>
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }
        public int IdPlan { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }

        /// <summary>Método de pago: Efectivo, Transferencia, Tarjeta, Yape, Plin.</summary>
        public string MetodoPago { get; set; }

        public int IdUsuarioRegistro { get; set; }

        // ===== Navegación =====
        public virtual PlanTratamiento Plan { get; set; }
        public virtual Usuario UsuarioRegistro { get; set; }
    }
}