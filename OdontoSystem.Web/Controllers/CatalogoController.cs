using OdontoSystem.BLL.Services;
using OdontoSystem.Entities;
using OdontoSystem.Web.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OdontoSystem.Web.Controllers
{
    [Autenticado]
    public class CatalogoController : Controller
    {
        private readonly CatalogoTratamientoService _service = new CatalogoTratamientoService();

        public ActionResult Index()
        {
            var tratamientos = _service.Listar();
            return View(tratamientos);
        }
        [HttpGet]
        public ActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public ActionResult Crear(CatalogoTratamiento modelo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _service.Registrar(modelo);

                    return RedirectToAction("Index");
                }
                return View(modelo);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(modelo);
            }
        }
        [HttpGet]
        public ActionResult Editar(int id)
        {
            var tratamiento = _service.ObtenerPorId(id);

            if (tratamiento == null)
            {
                TempData["Error"] = "El tratamiento que intentas editar no existe.";
                return RedirectToAction("Index");
            }

            return View(tratamiento);
        }

        // 2. Método POST: Recibe los datos modificados cuando le das a "Guardar"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(CatalogoTratamiento modelo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _service.Actualizar(modelo);
                    TempData["Exito"] = "Tratamiento actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                return View(modelo);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(modelo);
            }
        }
        [HttpGet]
        public ActionResult CambiarEstado(int id)
        {
            try
            {
                _service.CambiarEstado(id);
                TempData["Exito"] = "El estado del tratamiento se actualizó correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}