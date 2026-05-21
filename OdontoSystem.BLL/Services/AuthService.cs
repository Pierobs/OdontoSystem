using System;
using OdontoSystem.DAL.Context;
using OdontoSystem.DAL.Repositories;
using OdontoSystem.Entities;

namespace OdontoSystem.BLL.Services
{
    public class AuthService
    {
        /// <summary>
        /// Intenta autenticar al usuario. Devuelve el usuario si las credenciales
        /// son correctas, o null si fallan. Lanza excepción si el usuario está inactivo.
        /// </summary>
        public Usuario IniciarSesion(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Correo y contraseña son obligatorios");

            using (var ctx = new OdontoContext())
            {
                var repo = new UsuarioRepository(ctx);
                var usuario = repo.ObtenerPorCorreo(correo);

                if (usuario == null)
                    return null; // Usuario no existe

                if (usuario.Estado != "A")
                    throw new InvalidOperationException(
                        "Acceso denegado: cuenta desactivada. Contacte al administrador.");

                // Verificar la contraseña con BCrypt
                bool valida = false;
                try
                {
                    valida = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
                }
                catch
                {
                    // Si el hash es inválido (ej. el del seed inicial), tratamos como contraseña incorrecta
                    valida = false;
                }

                return valida ? usuario : null;
            }
        }
    }
}