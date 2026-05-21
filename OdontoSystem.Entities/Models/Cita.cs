using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    public class Cita
    {
        [Key]
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public int IdOdontologo { get; set; }
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraCita { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public string Motivo { get; set; }   // NUEVO — motivo de cancelación/reprogramación
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public virtual Paciente Paciente { get; set; }
        public virtual Usuario Odontologo { get; set; }
        public virtual ICollection<HistorialEstadoCita> Historial { get; set; }
    }
}