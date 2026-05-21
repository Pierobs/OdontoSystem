using System;
using OdontoSystem.DAL.Context;
using OdontoSystem.DAL.Repositories;
using OdontoSystem.Entities;

namespace OdontoSystem.BLL.Services
{
    public enum ResultadoLogin
    {
        Exito,
        UsuarioNoExiste,
        PasswordIncorrecto,
        UsuarioInactivo
    }

    public class LoginResponse
    {
        public ResultadoLogin Resultado { get; set; }
        public Usuario Usuario { get; set; }
    }

    public class AuthService
    {
        public LoginResponse IniciarSesion(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Correo y contraseña son obligatorios");

            using (var ctx = new OdontoContext())
            {
                var repo = new UsuarioRepository(ctx);
                var usuario = repo.ObtenerPorCorreo(correo);

                if (usuario == null)
                    return new LoginResponse { Resultado = ResultadoLogin.UsuarioNoExiste };

                if (usuario.Estado != "A")
                    return new LoginResponse { Resultado = ResultadoLogin.UsuarioInactivo };

                bool valida = false;
                try
                {
                    valida = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
                }
                catch
                {
                    valida = false;
                }

                if (!valida)
                    return new LoginResponse { Resultado = ResultadoLogin.PasswordIncorrecto };

                return new LoginResponse
                {
                    Resultado = ResultadoLogin.Exito,
                    Usuario = usuario
                };
            }
        }
    }
}