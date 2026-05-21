using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace OdontoSystem.Entities
{
    public class TipoDocumento
    {
        [Key]
        public byte IdTipoDocumento { get; set; }
        public string Descripcion { get; set; }
        public string Abreviatura { get; set; }
        public string Estado { get; set; }
    }
}