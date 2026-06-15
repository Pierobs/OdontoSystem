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
        // Días que dura una contraseña antes de tener que cambiarla
        public const int DIAS_VIGENCIA_PASSWORD = 180;
        public const int DIAS_AVISO_PREVIO = 15;

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

                // NUEVO: forzar cambio en primer login
                usuario.DebeCambiarPassword = true;
                usuario.FechaUltimoCambioPassword = null;

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

                    // NUEVO: admin cambió la clave → forzar al usuario a cambiarla otra vez en su próximo login
                    u.DebeCambiarPassword = true;
                    u.FechaUltimoCambioPassword = null;
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

        // ============================================================
        // NUEVOS MÉTODOS PARA POLÍTICA DE CONTRASEÑAS
        // ============================================================

        /// <summary>
        /// El usuario cambia SU PROPIA contraseña (debe conocer la actual).
        /// </summary>
        public void CambiarPasswordPropio(int idUsuario, string passwordActual, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(passwordActual))
                throw new ArgumentException("Debe ingresar su contraseña actual");

            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
                throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres");

            using (var ctx = new OdontoContext())
            {
                var repo = new UsuarioRepository(ctx);
                var u = repo.GetById(idUsuario);
                if (u == null)
                    throw new InvalidOperationException("Usuario no encontrado");

                // Verificar contraseña actual
                bool valida = false;
                try
                {
                    valida = BCrypt.Net.BCrypt.Verify(passwordActual, u.PasswordHash);
                }
                catch
                {
                    valida = false;
                }

                if (!valida)
                    throw new InvalidOperationException("La contraseña actual es incorrecta");

                // Verificar que la nueva no sea igual a la actual
                if (BCrypt.Net.BCrypt.Verify(nuevaPassword, u.PasswordHash))
                    throw new InvalidOperationException("La nueva contraseña no puede ser igual a la actual");

                // Cambiar
                u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                u.DebeCambiarPassword = false;
                u.FechaUltimoCambioPassword = DateTime.Now;

                repo.Update(u);
                repo.SaveChanges();
            }
        }

        /// <summary>
        /// Verifica si un usuario debe cambiar su contraseña.
        /// Razones: flag activado por admin, primer login, o pasaron 180 días.
        /// </summary>
        public bool RequiereCambioPassword(int idUsuario)
        {
            using (var ctx = new OdontoContext())
            {
                var u = ctx.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
                if (u == null) return false;

                // Razón 1: bandera activada por admin o primer login
                if (u.DebeCambiarPassword) return true;

                // Razón 2: nunca registró un cambio (edge case)
                if (!u.FechaUltimoCambioPassword.HasValue) return true;

                // Razón 3: pasaron más de 180 días desde último cambio
                var diasPasados = (DateTime.Now - u.FechaUltimoCambioPassword.Value).TotalDays;
                return diasPasados > DIAS_VIGENCIA_PASSWORD;
            }
        }

        /// <summary>
        /// Devuelve cuántos días faltan para que expire la contraseña.
        /// Si ya expiró, devuelve un número negativo.
        /// Si no tiene fecha registrada, devuelve null.
        /// </summary>
        public int? DiasParaExpiracion(int idUsuario)
        {
            using (var ctx = new OdontoContext())
            {
                var u = ctx.Usuarios.FirstOrDefault(x => x.IdUsuario == idUsuario);
                if (u?.FechaUltimoCambioPassword == null) return null;

                var diasPasados = (DateTime.Now - u.FechaUltimoCambioPassword.Value).TotalDays;
                return (int)(DIAS_VIGENCIA_PASSWORD - diasPasados);
            }
        }
    }
}