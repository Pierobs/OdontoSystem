using System.Linq;
using System.Data.Entity;
using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
namespace OdontoSystem.DAL.Repositories
{
    public class UsuarioRepository : Repository<Usuario>
    {
        public UsuarioRepository(OdontoContext context) : base(context) { }

        public Usuario ObtenerPorCorreo(string correo)
        {
            return _dbSet.Include(u => u.Rol)
                         .FirstOrDefault(u => u.CorreoInstitucional == correo);
        }

        public bool ExisteCorreo(string correo, int? idExcluir = null)
        {
            return _dbSet.Any(u => u.CorreoInstitucional == correo &&
                                   (!idExcluir.HasValue || u.IdUsuario != idExcluir.Value));
        }
    }
}