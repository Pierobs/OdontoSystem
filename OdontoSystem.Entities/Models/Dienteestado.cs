using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Estado clínico de una pieza dental dentro de un odontograma.
    /// Vinculado a HU-05 — Odontograma digital interactivo por pieza.
    /// </summary>
    public class DienteEstado
    {
        [Key]
        public int IdDienteEstado { get; set; }
        public int IdOdontograma { get; set; }

        /// <summary>Número de pieza dental según notación FDI (11-48, 51-85).</summary>
        public byte NumeroPieza { get; set; }

        /// <summary>Superficie afectada: Oclusal, Vestibular, Lingual, Mesial, Distal.</summary>
        public string Superficie { get; set; }

        /// <summary>
        /// Estado clínico: Sano, Caries, Curacion, Extraccion, Corona,
        /// Implante, Ausente, Endodoncia, Fractura, Perno.
        /// </summary>
        public string Estado { get; set; }

        public string Observacion { get; set; }
        public DateTime FechaRegistro { get; set; }

        // ===== Navegación =====
        public virtual Odontograma Odontograma { get; set; }
    }
}