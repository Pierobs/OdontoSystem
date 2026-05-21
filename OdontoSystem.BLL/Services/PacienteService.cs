using OdontoSystem.DAL.Context;
using OdontoSystem.DAL.Repositories;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity; 

namespace OdontoSystem.BLL.Services
{
    public class PacienteService
    {
        public IEnumerable<Paciente> Listar()
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new PacienteRepository(ctx);
                return repo.Find(p => true)
                    .OrderBy(p => p.Estado)
                    .ThenBy(p => p.ApellidoPaterno)
                    .ToList();
            }
        }

        public Paciente Registrar(Paciente paciente)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new PacienteRepository(ctx);
                if (repo.ExisteDocumento(paciente.NumeroDocumento))
                    throw new InvalidOperationException("El número de documento ya está registrado");
                if (!string.IsNullOrWhiteSpace(paciente.Correo))
                {
                    var resultadoCorreo = EmailValidator.Validar(paciente.Correo, obligatorio: false);
                    if (!resultadoCorreo.EsValido)
                        throw new InvalidOperationException(resultadoCorreo.Mensaje);

                    paciente.Correo = paciente.Correo.Trim().ToLower();
                    bool correoYaUsado = ctx.Pacientes.Any(p =>
                        p.Correo == paciente.Correo && p.Estado == "A");
                    if (correoYaUsado)
                        throw new InvalidOperationException(
                            "Este correo ya está registrado para otro paciente");
                }

                paciente.Estado = "A";
                paciente.FechaRegistro = DateTime.Now;
                repo.Add(paciente);
                repo.SaveChanges();
                return paciente;
            }
        }

        public IEnumerable<Paciente> Buscar(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                throw new ArgumentException("Ingrese al menos un criterio de búsqueda");

            using (var ctx = new OdontoContext())
            {
                var repo = new PacienteRepository(ctx);
                return repo.Buscar(criterio).ToList();
            }
        }
    
    public Paciente Actualizar(Paciente paciente)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new PacienteRepository(ctx);
                var existente = repo.GetById(paciente.IdPaciente);

                if (existente == null)
                    throw new InvalidOperationException("Paciente no encontrado");

                // Validar documento duplicado (excluyendo a sí mismo)
                var otroConMismoDoc = ctx.Pacientes.FirstOrDefault(p =>
                    p.NumeroDocumento == paciente.NumeroDocumento &&
                    p.IdPaciente != paciente.IdPaciente);

                if (otroConMismoDoc != null)
                    throw new InvalidOperationException("El número de documento ya está registrado para otro paciente");

                // Validar correo (si lo ingresó)
                if (!string.IsNullOrWhiteSpace(paciente.Correo))
                {
                    var resultadoCorreo = EmailValidator.Validar(paciente.Correo, obligatorio: false);
                    if (!resultadoCorreo.EsValido)
                        throw new InvalidOperationException(resultadoCorreo.Mensaje);

                    // Validar correo duplicado (excluyendo a sí mismo)
                    bool correoYaUsado = ctx.Pacientes.Any(p =>
                        p.Correo == paciente.Correo &&
                        p.IdPaciente != paciente.IdPaciente &&
                        p.Estado == "A");

                    if (correoYaUsado)
                        throw new InvalidOperationException("Este correo ya está registrado para otro paciente");
                }

                // Actualizar campos
                existente.ApellidoPaterno = paciente.ApellidoPaterno;
                existente.ApellidoMaterno = paciente.ApellidoMaterno;
                existente.Nombres = paciente.Nombres;
                existente.IdTipoDocumento = paciente.IdTipoDocumento;
                existente.NumeroDocumento = paciente.NumeroDocumento;
                existente.IdSexo = paciente.IdSexo;
                existente.FechaNacimiento = paciente.FechaNacimiento;
                existente.Telefono = paciente.Telefono;
                existente.Correo = paciente.Correo;
                existente.IdDistrito = paciente.IdDistrito;
                existente.Direccion = paciente.Direccion;

                repo.Update(existente);
                repo.SaveChanges();
                return existente;
            }
        }

        public void CambiarEstado(int id)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new PacienteRepository(ctx);
                var paciente = repo.GetById(id);

                if (paciente == null)
                    throw new InvalidOperationException("Paciente no encontrado");

                // Soft delete: alterna entre Activo e Inactivo
                paciente.Estado = (paciente.Estado == "A") ? "I" : "A";
                repo.Update(paciente);
                repo.SaveChanges();
            }
        }

        /// <summary>
        /// Lista todos los distritos para el dropdown del formulario.
        /// </summary>
        public IEnumerable<Distrito> ListarDistritos()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Distritos
                          .OrderBy(d => d.Departamento)
                          .ThenBy(d => d.Provincia)
                          .ThenBy(d => d.Nombre)
                          .ToList();
            }
        }

        public bool ExisteDocumento(string numero)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Pacientes.Any(p => p.NumeroDocumento == numero);
            }
        }

        public Paciente ObtenerPorId(int id)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Pacientes
                          .Include(p => p.TipoDocumento)
                          .Include(p => p.Sexo)
                          .Include(p => p.Distrito)
                          .FirstOrDefault(p => p.IdPaciente == id);
            }
        }
    }
}