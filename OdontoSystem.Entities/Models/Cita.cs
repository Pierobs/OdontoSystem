using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Cita agendada para un paciente con un odontólogo en una fecha y hora.
    /// Vinculado a HU-03 — Agendar cita y HU-04 — Cancelar / reprogramar cita.
    /// </summary>
    public class Cita
    {
        [Key]
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public int IdOdontologo { get; set; }
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraCita { get; set; }

        /// <summary>Estado de la cita: Pendiente, Atendida, Cancelada.</summary>
        public string Estado { get; set; }

        public string Observaciones { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // ===== Navegación =====
        public virtual Paciente Paciente { get; set; }
        public virtual Usuario Odontologo { get; set; }

        // Nota: la relación con Odontograma es 1:1 a nivel de BD (UQ_Odontogramas_Cita),
        // pero a nivel de EF la consultamos vía DbContext.Odontogramas.FirstOrDefault(...)
        // para simplificar el modelo.
    }
}