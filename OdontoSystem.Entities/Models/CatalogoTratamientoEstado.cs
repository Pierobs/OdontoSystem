using System;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Mapeo entre un tratamiento del catálogo y el estado dental que produce
    /// al completarse. Permite que el sistema actualice automáticamente el odontograma
    /// cuando se marca un tratamiento como completado.
    /// HU-16 (Sprint 3).
    /// </summary>
    public class CatalogoTratamientoEstado
    {
        [Key]
        public int IdMapeo { get; set; }

        public int IdTratamiento { get; set; }

        /// <summary>
        /// Estado que queda en la pieza/superficie tras completar el tratamiento.
        /// Valores: Sano, Caries, Curacion, Endodoncia, Corona, Implante, Ausente, Fractura, Perno.
        /// </summary>
        public string EstadoResultante { get; set; }

        /// <summary>
        /// True: el estado se aplica solo a la superficie específica donde se trabajó.
        /// False: el estado se aplica a TODA la pieza (ej. extracción, corona).
        /// </summary>
        public bool AplicaSuperficie { get; set; }

        // ===== Navegación =====
        public virtual CatalogoTratamiento Tratamiento { get; set; }
    }
}