using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OdontoSystem.Entities
{
    /// <summary>
    /// Paciente del consultorio. Incluye datos personales y de contacto.
    /// Vinculado a HU-01 — Registrar nuevo paciente y HU-02 — Buscar paciente.
    /// </summary>
    public class Paciente
    {
        [Key]
        public int IdPaciente { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Nombres { get; set; }
        public byte IdTipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public byte IdSexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public int? IdDistrito { get; set; }
        public string Direccion { get; set; }

        /// <summary>'A' = Activo, 'I' = Inactivo (soft delete).</summary>
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }

        // ===== Navegación: hacia las tablas maestras =====
        public virtual TipoDocumento TipoDocumento { get; set; }
        public virtual Sexo Sexo { get; set; }
        public virtual Distrito Distrito { get; set; }

        // ===== Navegación: colecciones inversas =====
        public virtual ICollection<Cita> Citas { get; set; }
        public virtual ICollection<Odontograma> Odontogramas { get; set; }
        public virtual ICollection<PlanTratamiento> Planes { get; set; }

        public Paciente()
        {
            Citas = new HashSet<Cita>();
            Odontogramas = new HashSet<Odontograma>();
            Planes = new HashSet<PlanTratamiento>();
        }
    }
}