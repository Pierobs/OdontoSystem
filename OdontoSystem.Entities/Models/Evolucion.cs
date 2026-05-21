using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Registro de un procedimiento realizado en una sesión de tratamiento.
    /// Vinculado a HU-07 — Registrar evolución de tratamiento.
    /// </summary>
    public class Evolucion
    {
        [Key]
        public int IdEvolucion { get; set; }
        public int IdPlan { get; set; }
        public int IdOdontologo { get; set; }
        public int IdTratamiento { get; set; }
        public DateTime FechaEvolucion { get; set; }
        public string Descripcion { get; set; }

        // ===== Navegación =====
        public virtual PlanTratamiento Plan { get; set; }
        public virtual Usuario Odontologo { get; set; }
        public virtual CatalogoTratamiento Tratamiento { get; set; }
    }
}