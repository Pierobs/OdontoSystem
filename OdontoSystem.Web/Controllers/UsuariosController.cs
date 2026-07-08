using System;
using System.Web.Mvc;
using OdontoSystem.BLL.Services;
using OdontoSystem.Entities;
using OdontoSystem.Web.Filters;

namespace OdontoSystem.Web.Controllers
{
    [SoloAdmin]
    public class UsuariosController : Controller
    {
        private readonly UsuarioService _service = new UsuarioService();

        public ActionResult Index()
        {
            return View(_service.Listar());
        }

        public ActionResult Buscar(string criterio)
        {
            ViewBag.Criterio = criterio;
            return View("Index", _service.Buscar(criterio));
        }

        public ActionResult Crear()
        {
            ViewBag.Roles = _service.ListarRoles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Usuario usuario, string Password)
        {
            try
            {
                _service.Registrar(usuario, Password);
                TempData["Exito"] = "Usuario registrado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Roles = _service.ListarRoles();
                return View(usuario);
            }
        }

        public ActionResult Editar(int id)
        {
            var u = _service.ObtenerPorId(id);
            if (u == null) return HttpNotFound();
            ViewBag.Roles = _service.ListarRoles();
            return View(u);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int IdUsuario, string Nombres, string ApellidoPaterno,
                                   string ApellidoMaterno, byte IdRol, string NuevoPassword)
        {
            try
            {
                _service.Actualizar(IdUsuario, Nombres, ApellidoPaterno, ApellidoMaterno,
                                    IdRol, NuevoPassword);
                TempData["Exito"] = "Usuario actualizado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Editar", new { id = IdUsuario });
            }
        }

        public ActionResult CambiarEstado(int id)
        {
            try
            {
                _service.CambiarEstado(id);
                TempData["Exito"] = "Estado cambiado correctamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
        public ActionResult CambiarPassword(int id)
        {
            var u = _service.ObtenerPorId(id);
            if (u == null) return HttpNotFound();
            return View(u);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarPassword(int IdUsuario, string NuevaPassword)
        {
            try
            {
                var u = _service.ObtenerPorId(IdUsuario);
                if (u == null) return HttpNotFound();

                // Reusamos el método Actualizar pasando solo el password
                _service.Actualizar(IdUsuario, u.Nombres, u.ApellidoPaterno, u.ApellidoMaterno,
                                    u.IdRol, NuevaPassword);

                TempData["Exito"] = $"Contraseña de {u.Nombres} {u.ApellidoPaterno} actualizada correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("CambiarPassword", new { id = IdUsuario });
            }
        }
    }
}