using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace OdontoSystem.Entities
{
    public class Distrito
    {
        [Key]
        public int IdDistrito { get; set; }
        public string CodigoINEI { get; set; }
        public string Nombre { get; set; }
        public string Provincia { get; set; }
        public string Departamento { get; set; }
    }
}