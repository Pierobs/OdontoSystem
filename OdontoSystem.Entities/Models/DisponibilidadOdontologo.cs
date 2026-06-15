using System;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Bloque de disponibilidad de un odontólogo en una fecha específica.
    /// Vinculado a HU — Calendario de disponibilidad del odontólogo.
    /// </summary>
    public class DisponibilidadOdontologo
    {
        [Key]
        public int IdDisponibilidad { get; set; }
        public int IdOdontologo { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Estado { get; set; }   // 'A' = Activo, 'I' = Inactivo
        public DateTime FechaRegistro { get; set; }

        // ===== Navegación =====
        public virtual Usuario Odontologo { get; set; }
    }
}