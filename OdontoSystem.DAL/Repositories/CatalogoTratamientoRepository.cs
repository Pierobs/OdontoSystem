using System.Linq;
using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;

namespace OdontoSystem.DAL.Repositories
{
    public class CatalogoTratamientoRepository : Repository<CatalogoTratamiento>
    {
        public CatalogoTratamientoRepository(OdontoContext context) : base(context) { }
        public bool ExisteNombre(string nombre, int? idExcluir = null)
        {
            return _dbSet.Any(t => t.Nombre == nombre &&
                                   (!idExcluir.HasValue || t.IdTratamiento != idExcluir.Value));
        }
    }
}