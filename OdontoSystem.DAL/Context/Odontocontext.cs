using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations.Schema;


namespace OdontoSystem.DAL.Context
{
    /// <summary>
    /// Contexto principal de Entity Framework 6 para la base de datos OdontoSystem.
    /// La BD ya existe (creada con script 01_DespliegueBD_OdontoSystem_v3.sql),
    /// por eso se desactiva cualquier inicializador automático.
    /// </summary>
    public class OdontoContext : DbContext
    {
        public OdontoContext() : base("name=OdontoSystem")
        {
            // Importante: NO permitir que EF cree, borre o migre la BD.
            // La estructura la controla el equipo vía scripts SQL versionados.
            Database.SetInitializer<OdontoContext>(null);
        }

        // ============================================================
        //  DbSets — uno por entidad
        // ============================================================

        // Maestras
        public DbSet<Sexo> Sexos { get; set; }
        public DbSet<TipoDocumento> TiposDocumento { get; set; }
        public DbSet<Distrito> Distritos { get; set; }
        public DbSet<CatalogoTratamiento> CatalogoTratamientos { get; set; }

        // Seguridad
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }

        // Operativas
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Odontograma> Odontogramas { get; set; }
        public DbSet<DienteEstado> DientesEstado { get; set; }

        // Transaccionales
        public DbSet<PlanTratamiento> PlanesTratamiento { get; set; }
        public DbSet<PlanDetalle> PlanDetalles { get; set; }
        public DbSet<Evolucion> Evoluciones { get; set; }
        public DbSet<Pago> Pagos { get; set; }


        // ============================================================
        //  Configuración Fluent API
        // ============================================================
        protected override void OnModelCreating(DbModelBuilder mb)
        {
            // EF6 pluraliza por defecto los nombres de tabla. Lo desactivamos
            // porque nuestras tablas tienen nombres explícitos en la BD.
            mb.Conventions.Remove<PluralizingTableNameConvention>();

            // --------------------------------------------------------
            // 1) Mapeo de cada entidad a su tabla exacta de la BD
            // --------------------------------------------------------
            mb.Entity<Rol>().ToTable("Rol");
            mb.Entity<Sexo>().ToTable("Sexo");
            mb.Entity<TipoDocumento>().ToTable("TipoDocumento");
            mb.Entity<Distrito>().ToTable("Distrito");
            mb.Entity<CatalogoTratamiento>().ToTable("CatalogoTratamientos");
            mb.Entity<Usuario>().ToTable("Usuarios");
            mb.Entity<Paciente>().ToTable("Pacientes");
            mb.Entity<Cita>().ToTable("Citas");
            mb.Entity<Odontograma>().ToTable("Odontogramas");
            mb.Entity<DienteEstado>().ToTable("DientesEstado");
            mb.Entity<PlanTratamiento>().ToTable("PlanesTratamiento");
            mb.Entity<PlanDetalle>().ToTable("PlanDetalle");
            mb.Entity<Evolucion>().ToTable("Evoluciones");
            mb.Entity<Pago>().ToTable("Pagos");

            // --------------------------------------------------------
            // 2) Columnas COMPUTADAS en la BD (PERSISTED)
            //    EF las debe leer pero NUNCA escribir.
            // --------------------------------------------------------
            mb.Entity<PlanTratamiento>()
              .Property(p => p.Saldo)
              .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            mb.Entity<PlanDetalle>()
              .Property(d => d.Subtotal)
              .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // --------------------------------------------------------
            // 3) Relaciones — desactivamos cascada donde la BD no la
            //    tiene, para que EF no genere errores de "multiple
            //    cascade paths" al borrar entidades.
            // --------------------------------------------------------

            // Usuario → Rol
            mb.Entity<Usuario>()
              .HasRequired(u => u.Rol)
              .WithMany(r => r.Usuarios)
              .HasForeignKey(u => u.IdRol)
              .WillCascadeOnDelete(false);

            // Paciente → TipoDocumento / Sexo / Distrito
            mb.Entity<Paciente>()
              .HasRequired(p => p.TipoDocumento)
              .WithMany()
              .HasForeignKey(p => p.IdTipoDocumento)
              .WillCascadeOnDelete(false);

            mb.Entity<Paciente>()
              .HasRequired(p => p.Sexo)
              .WithMany()
              .HasForeignKey(p => p.IdSexo)
              .WillCascadeOnDelete(false);

            mb.Entity<Paciente>()
              .HasOptional(p => p.Distrito)
              .WithMany()
              .HasForeignKey(p => p.IdDistrito)
              .WillCascadeOnDelete(false);

            // Cita → Paciente / Odontólogo
            mb.Entity<Cita>()
              .HasRequired(c => c.Paciente)
              .WithMany(p => p.Citas)
              .HasForeignKey(c => c.IdPaciente)
              .WillCascadeOnDelete(false);

            mb.Entity<Cita>()
              .HasRequired(c => c.Odontologo)
              .WithMany(u => u.CitasComoOdontologo)
              .HasForeignKey(c => c.IdOdontologo)
              .WillCascadeOnDelete(false);

            // Odontograma → Cita / Paciente
            // (1:1 con Cita gracias al UQ en BD, pero EF lo trata como N:1 sin colección)
            mb.Entity<Odontograma>()
              .HasRequired(o => o.Cita)
              .WithMany()
              .HasForeignKey(o => o.IdCita)
              .WillCascadeOnDelete(false);

            mb.Entity<Odontograma>()
              .HasRequired(o => o.Paciente)
              .WithMany(p => p.Odontogramas)
              .HasForeignKey(o => o.IdPaciente)
              .WillCascadeOnDelete(false);

            // DienteEstado → Odontograma (con cascada en BD)
            mb.Entity<DienteEstado>()
              .HasRequired(d => d.Odontograma)
              .WithMany(o => o.DientesEstado)
              .HasForeignKey(d => d.IdOdontograma)
              .WillCascadeOnDelete(true);

            // PlanTratamiento → Paciente
            mb.Entity<PlanTratamiento>()
              .HasRequired(pt => pt.Paciente)
              .WithMany(p => p.Planes)
              .HasForeignKey(pt => pt.IdPaciente)
              .WillCascadeOnDelete(false);

            // PlanDetalle → PlanTratamiento (con cascada) y CatalogoTratamiento
            mb.Entity<PlanDetalle>()
              .HasRequired(pd => pd.Plan)
              .WithMany(p => p.Detalles)
              .HasForeignKey(pd => pd.IdPlan)
              .WillCascadeOnDelete(true);

            mb.Entity<PlanDetalle>()
              .HasRequired(pd => pd.Tratamiento)
              .WithMany(ct => ct.PlanDetalles)
              .HasForeignKey(pd => pd.IdTratamiento)
              .WillCascadeOnDelete(false);

            // Evolucion → PlanTratamiento / Usuario / CatalogoTratamiento
            mb.Entity<Evolucion>()
              .HasRequired(e => e.Plan)
              .WithMany(p => p.Evoluciones)
              .HasForeignKey(e => e.IdPlan)
              .WillCascadeOnDelete(false);

            mb.Entity<Evolucion>()
              .HasRequired(e => e.Odontologo)
              .WithMany(u => u.Evoluciones)
              .HasForeignKey(e => e.IdOdontologo)
              .WillCascadeOnDelete(false);

            mb.Entity<Evolucion>()
              .HasRequired(e => e.Tratamiento)
              .WithMany(ct => ct.Evoluciones)
              .HasForeignKey(e => e.IdTratamiento)
              .WillCascadeOnDelete(false);

            // Pago → PlanTratamiento / Usuario
            mb.Entity<Pago>()
              .HasRequired(pg => pg.Plan)
              .WithMany(p => p.Pagos)
              .HasForeignKey(pg => pg.IdPlan)
              .WillCascadeOnDelete(false);

            mb.Entity<Pago>()
              .HasRequired(pg => pg.UsuarioRegistro)
              .WithMany(u => u.PagosRegistrados)
              .HasForeignKey(pg => pg.IdUsuarioRegistro)
              .WillCascadeOnDelete(false);

            base.OnModelCreating(mb);
        }
    }
}
