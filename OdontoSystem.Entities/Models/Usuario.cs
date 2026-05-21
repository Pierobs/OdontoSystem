using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string CorreoInstitucional { get; set; }
        public string PasswordHash { get; set; }

        public byte IdRol { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public virtual Rol Rol { get; set; }
        public virtual ICollection<Cita> CitasComoOdontologo { get; set; }
        public virtual ICollection<Evolucion> Evoluciones { get; set; }
        public virtual ICollection<Pago> PagosRegistrados { get; set; }
        public Usuario()
        {
            CitasComoOdontologo = new HashSet<Cita>();
            Evoluciones = new HashSet<Evolucion>();
            PagosRegistrados = new HashSet<Pago>();
        }
    }
}