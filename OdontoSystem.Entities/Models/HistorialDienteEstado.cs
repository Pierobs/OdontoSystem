using System;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Registro histórico de cada cambio de estado en el odontograma.
    /// Permite reconstruir el estado dental de un paciente en cualquier momento
    /// del pasado y generar la línea de tiempo del odontograma.
    /// HU-18 (Sprint 3).
    /// </summary>
    public class HistorialDienteEstado
    {
        [Key]
        public int IdHistorial { get; set; }

        public int IdPaciente { get; set; }
        public byte NumeroPieza { get; set; }
        public string Superficie { get; set; }

        /// <summary>
        /// Estado que tenía la superficie ANTES del cambio. 
        /// Puede ser null si es el primer registro de esa superficie.
        /// </summary>
        public string EstadoAnterior { get; set; }

        /// <summary>
        /// Estado que queda DESPUÉS del cambio.
        /// </summary>
        public string EstadoNuevo { get; set; }

        public DateTime FechaCambio { get; set; }

        /// <summary>
        /// Cita en la que se produjo el cambio (opcional).
        /// </summary>
        public int? IdCita { get; set; }

        /// <summary>
        /// Detalle del plan que originó el cambio (opcional).
        /// </summary>
        public int? IdPlanDetalle { get; set; }

        public string Observacion { get; set; }

        // ===== Navegación =====
        public virtual Paciente Paciente { get; set; }
        public virtual Cita Cita { get; set; }
        public virtual PlanDetalle PlanDetalle { get; set; }
    }
}