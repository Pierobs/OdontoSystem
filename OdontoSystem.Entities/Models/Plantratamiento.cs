using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Plan de tratamiento de un paciente. Agrupa los procedimientos seleccionados,
    /// el total a pagar y los abonos.
    /// Vinculado a HU-06 — Crear plan de tratamiento.
    /// </summary>
    public class PlanTratamiento
    {
        [Key]
        public int IdPlan { get; set; }
        public int IdPaciente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoAbonado { get; set; }

        /// <summary>
        /// Saldo pendiente = MontoTotal - MontoAbonado.
        /// COLUMNA COMPUTADA en la BD (PERSISTED): EF la lee, no la escribe.
        /// Configurado en OdontoContext.OnModelCreating con
        /// DatabaseGeneratedOption.Computed.
        /// </summary>
        public decimal Saldo { get; set; }

        /// <summary>Estado del plan: Activo, Pagado, Cancelado.</summary>
        public string Estado { get; set; }

        // ===== Navegación =====
        public virtual Paciente Paciente { get; set; }
        public virtual ICollection<PlanDetalle> Detalles { get; set; }
        public virtual ICollection<Evolucion> Evoluciones { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; }

        public PlanTratamiento()
        {
            Detalles = new HashSet<PlanDetalle>();
            Evoluciones = new HashSet<Evolucion>();
            Pagos = new HashSet<Pago>();
        }
    }
}