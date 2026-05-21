using System;
using System.Web.Mvc;
using OdontoSystem.BLL.Services;

namespace OdontoSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService = new AuthService();

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Session["IdUsuario"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string correo, string password, bool recordar = false)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Debe ingresar correo y contraseña";
                return RedirectToAction("Login");
            }

            try
            {
                var respuesta = _authService.IniciarSesion(correo, password);

                switch (respuesta.Resultado)
                {
                    case ResultadoLogin.UsuarioNoExiste:
                        TempData["Error"] = "❌ El correo ingresado no está registrado en el sistema";
                        return RedirectToAction("Login");

                    case ResultadoLogin.PasswordIncorrecto:
                        TempData["Error"] = "🔒 La contraseña es incorrecta";
                        return RedirectToAction("Login");

                    case ResultadoLogin.UsuarioInactivo:
                        TempData["Error"] = "⛔ Esta cuenta está desactivada. Contacte al administrador";
                        return RedirectToAction("Login");

                    case ResultadoLogin.Exito:
                        var usuario = respuesta.Usuario;

                        Session["IdUsuario"] = usuario.IdUsuario;
                        Session["UsuarioCorreo"] = usuario.CorreoInstitucional;
                        Session["NombreCompleto"] = $"{usuario.Nombres} {usuario.ApellidoPaterno}";
                        Session["Rol"] = usuario.Rol?.Descripcion ?? "SinRol";
                        Session["IdRol"] = usuario.IdRol;

                        TempData["Exito"] = $"¡Bienvenido, {usuario.Nombres}!";
                        return RedirectToAction("Index", "Home");
                }

                TempData["Error"] = "Error desconocido al iniciar sesión";
                return RedirectToAction("Login");
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al iniciar sesión. Intente nuevamente.";
                return RedirectToAction("Login");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            TempData["Exito"] = "Sesión cerrada correctamente";
            return RedirectToAction("Login");
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }
    }
}