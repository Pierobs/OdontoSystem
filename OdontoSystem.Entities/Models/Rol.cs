using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    public class Rol
    {
        [Key]
        public byte IdRol { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
