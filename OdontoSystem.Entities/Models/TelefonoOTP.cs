using System;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    public class TelefonoOTP
    {
        [Key]
        public int IdOTP { get; set; }
        public string Telefono { get; set; }
        public string Codigo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Verificado { get; set; }
        public int Intentos { get; set; }
    }
}
