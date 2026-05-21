using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{    public class Odontograma
    {
        [Key]
        public int IdOdontograma { get; set; }
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public DateTime FechaRegistro { get; set; }

        // ===== Navegación =====
        public virtual Cita Cita { get; set; }
        public virtual Paciente Paciente { get; set; }
        public virtual ICollection<DienteEstado> DientesEstado { get; set; }

        public Odontograma()
        {
            DientesEstado = new HashSet<DienteEstado>();
        }
    }
}