using System;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Registro de un procedimiento realizado en una sesión de tratamiento.
    /// Vinculado a HU-07 — Registrar evolución de tratamiento.
    /// La cita es opcional: permite registrar sesiones sin cita formal.
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

        /// <summary>
        /// Cita en la que se realizó este procedimiento. Nullable — no siempre hay cita formal.
        /// </summary>
        public int? IdCita { get; set; }

        /// <summary>
        /// Ítem específico del plan al que aplica esta evolución.
        /// Permite distinguir entre dos tratamientos iguales en el mismo plan (ej: 2 incrustaciones).
        /// </summary>
        public int? IdPlanDetalle { get; set; }

        // ===== Navegación =====
        public virtual PlanTratamiento Plan { get; set; }
        public virtual Usuario Odontologo { get; set; }
        public virtual CatalogoTratamiento Tratamiento { get; set; }
        public virtual Cita Cita { get; set; }

        public virtual PlanDetalle PlanDetalle { get; set; }
    }
}