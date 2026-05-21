using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Procedimiento incluido dentro de un plan de tratamiento.
    /// Vinculado a HU-06 — Crear plan de tratamiento con catálogo.
    /// </summary>
    public class PlanDetalle
    {
        [Key]
        public int IdPlanDetalle { get; set; }
        public int IdPlan { get; set; }
        public int IdTratamiento { get; set; }
        public byte Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Subtotal = Cantidad × PrecioUnitario.
        /// COLUMNA COMPUTADA en la BD (PERSISTED): EF la lee, no la escribe.
        /// Configurado en OdontoContext.OnModelCreating con
        /// DatabaseGeneratedOption.Computed.
        /// </summary>
        public decimal Subtotal { get; set; }

        // ===== Navegación =====
        public virtual PlanTratamiento Plan { get; set; }
        public virtual CatalogoTratamiento Tratamiento { get; set; }
    }
}