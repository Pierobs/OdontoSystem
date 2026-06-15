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
    /// HU-14 (Sprint 3) — Agrega vinculación a pieza/superficie y estado de seguimiento.
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

        // ===== NUEVO Sprint 3 =====

        /// <summary>
        /// Pieza dental (FDI) a la que aplica este tratamiento.
        /// Nullable para mantener compatibilidad con planes antiguos.
        /// </summary>
        public byte? NumeroPieza { get; set; }

        /// <summary>
        /// Superficie de la pieza: Vestibular, Lingual, Mesial, Distal, Oclusal, Completo.
        /// Nullable: si el tratamiento aplica a toda la pieza, puede estar vacío.
        /// </summary>
        public string Superficie { get; set; }

        /// <summary>
        /// Estado del tratamiento dentro del plan: 
        /// Pendiente | EnProceso | Completado | Cancelado.
        /// </summary>
        public string EstadoTratamiento { get; set; }

        // ===== Navegación =====
        public virtual PlanTratamiento Plan { get; set; }
        public virtual CatalogoTratamiento Tratamiento { get; set; }
    }
}