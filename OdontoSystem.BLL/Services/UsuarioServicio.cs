using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using OdontoSystem.DAL.Context;
using OdontoSystem.DAL.Repositories;
using OdontoSystem.Entities;

namespace OdontoSystem.BLL.Services
{
    public class UsuarioService
    {
        public IEnumerable<Usuario> Listar()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Usuarios
                          .Include(u => u.Rol)
                          .OrderBy(u => u.ApellidoPaterno)
                          .ToList();
            }
        }

        public Usuario ObtenerPorId(int id)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Usuarios
                          .Include(u => u.Rol)
                          .FirstOrDefault(u => u.IdUsuario == id);
            }
        }

        public IEnumerable<Rol> ListarRoles()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Roles.Where(r => r.Estado == "A").ToList();
            }
        }

        public void Registrar(Usuario usuario, string passwordPlano)
        {
            if (string.IsNullOrWhiteSpace(passwordPlano) || passwordPlano.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres");

            using (var ctx = new OdontoContext())
            {
                var repo = new UsuarioRepository(ctx);

                if (repo.ExisteCorreo(usuario.CorreoInstitucional))
                    throw new InvalidOperationException(
                        "El correo institucional ya está registrado para otro usuario");

                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano);
                usuario.Estado = "A";
                usuario.FechaCreacion = DateTime.Now;

                repo.Add(usuario);
                repo.SaveChanges();
            }
        }

        public void Actualizar(int id, string nombres, string apPaterno, string apMaterno,
                               byte idRol, string nuevoPassword = null)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new UsuarioRepository(ctx);
                var u = repo.GetById(id);
                if (u == null)
                    throw new InvalidOperationException("Usuario no encontrado");

                u.Nombres = nombres;
                u.ApellidoPaterno = apPaterno;
                u.ApellidoMaterno = apMaterno;
                u.IdRol = idRol;

                // Solo actualizar la contraseña si se proporcionó una nueva
                if (!string.IsNullOrWhiteSpace(nuevoPassword))
                {
                    if (nuevoPassword.Length < 6)
                        throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres");
                    u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevoPassword);
                }

                repo.Update(u);
                repo.SaveChanges();
            }
        }

        public void CambiarEstado(int id)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new UsuarioRepository(ctx);
                var u = repo.GetById(id);
                if (u == null)
                    throw new InvalidOperationException("Usuario no encontrado");

                u.Estado = (u.Estado == "A") ? "I" : "A";
                repo.Update(u);
                repo.SaveChanges();
            }
        }
    }
}