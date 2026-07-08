using System;
using System.Collections.Generic;
using System.Linq;
using OdontoSystem.DAL.Context;
using OdontoSystem.DAL.Repositories;
using OdontoSystem.Entities;

namespace OdontoSystem.BLL.Services
{
    public class CatalogoTratamientoService
    {
        public IEnumerable<CatalogoTratamiento> Listar(bool soloActivos = false)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new CatalogoTratamientoRepository(ctx);
                if (soloActivos)
                    return repo.Find(t => t.Estado == "A").OrderBy(t => t.Nombre).ToList();
                return repo.GetAll().OrderBy(t => t.Nombre).ToList();
            }
        }

        public CatalogoTratamiento ObtenerPorId(int id)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new CatalogoTratamientoRepository(ctx);
                return repo.GetById(id);
            }
        }

        public void Registrar(CatalogoTratamiento tratamiento)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new CatalogoTratamientoRepository(ctx);

                if (repo.ExisteNombre(tratamiento.Nombre))
                    throw new InvalidOperationException("Ya existe un tratamiento con ese nombre");

                if (tratamiento.PrecioBase < 0)
                    throw new ArgumentException("El precio no puede ser negativo");

                tratamiento.Estado = "A";
                tratamiento.FechaRegistro = DateTime.Now;

                repo.Add(tratamiento);
                repo.SaveChanges();
            }
        }

        public void Actualizar(CatalogoTratamiento tratamiento)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new CatalogoTratamientoRepository(ctx);

                if (repo.ExisteNombre(tratamiento.Nombre, tratamiento.IdTratamiento))
                    throw new InvalidOperationException("Ya existe otro tratamiento con ese nombre");

                repo.Update(tratamiento);
                repo.SaveChanges();
            }
        }

        public void CambiarEstado(int id)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new CatalogoTratamientoRepository(ctx);
                var t = repo.GetById(id);
                if (t == null)
                    throw new InvalidOperationException("Tratamiento no encontrado");

                t.Estado = (t.Estado == "A") ? "I" : "A";
                repo.Update(t);
                repo.SaveChanges();
            }
        }

        public IEnumerable<CatalogoTratamiento> Buscar(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                return Listar();

            criterio = criterio.ToLower();
            using (var db = new OdontoContext())
            {
                return db.CatalogoTratamientos
                    .Where(t => t.Nombre.ToLower().Contains(criterio))
                    .OrderBy(t => t.Nombre)
                    .ToList();
            }
        }
    }
}