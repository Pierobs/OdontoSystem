using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace OdontoSystem.Entities
{
    /// <summary>
    /// Catálogo de procedimientos disponibles con su precio base.
    /// Vinculado a HU-11 — Gestionar catálogo de tratamientos.
    /// </summary>
    public class CatalogoTratamiento
    {
        [Key]
        public int IdTratamiento { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioBase { get; set; }

        /// <summary>'A' = Activo, 'I' = Inactivo. Los inactivos no aparecen en nuevos planes.</summary>
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }

        // ===== Colecciones inversas =====
        public virtual ICollection<PlanDetalle> PlanDetalles { get; set; }
        public virtual ICollection<Evolucion> Evoluciones { get; set; }

        public CatalogoTratamiento()
        {
            PlanDetalles = new HashSet<PlanDetalle>();
            Evoluciones = new HashSet<Evolucion>();
        }
    }
}