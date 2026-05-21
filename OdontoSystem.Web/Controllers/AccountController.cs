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
                var usuario = _authService.IniciarSesion(correo, password);

                if (usuario != null)
                {
                    // ────────────────────────────────────────────────────────
                    //  GUARDAR DATOS EN SESIÓN
                    //  Los nombres deben coincidir con los que usan
                    //  _Layout.cshtml y los Filtros (SoloAdminAttribute).
                    // ────────────────────────────────────────────────────────
                    Session["IdUsuario"] = usuario.IdUsuario;
                    Session["UsuarioCorreo"] = usuario.CorreoInstitucional;

                    // Nombre completo (lo usa el menú superior derecho)
                    Session["NombreCompleto"] = $"{usuario.Nombres} {usuario.ApellidoPaterno}";

                    // Rol como texto (lo usan los filtros y el menú para mostrar "Usuarios")
                    Session["Rol"] = usuario.Rol?.Descripcion ?? "SinRol";

                    // Por si lo necesitas en algún lado
                    Session["IdRol"] = usuario.IdRol;

                    TempData["Exito"] = $"¡Bienvenido, {usuario.Nombres}!";
                    return RedirectToAction("Index", "Home");
                }

                TempData["Error"] = "Correo o contraseña incorrectos";
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
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