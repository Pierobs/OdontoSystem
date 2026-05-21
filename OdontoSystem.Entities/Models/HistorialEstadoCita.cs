using System;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    public class HistorialEstadoCita
    {
        [Key]
        public int IdHistorial { get; set; }
        public int IdCita { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaCambio { get; set; }
        public int? IdUsuario { get; set; }

        public virtual Cita Cita { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}