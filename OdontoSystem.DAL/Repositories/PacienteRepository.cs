using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdontoSystem.DAL.Repositories
{
    public class PacienteRepository : Repository<Paciente>
    {
        public PacienteRepository(OdontoContext context) : base(context) { }
        public bool ExisteDocumento(string numeroDocumento) =>
            _dbSet.Any(p => p.NumeroDocumento == numeroDocumento);
        public IQueryable<Paciente> Buscar(string criterio)
        {
            return _dbSet.Where(p =>
                p.NumeroDocumento.Contains(criterio) ||
                p.Nombres.Contains(criterio) ||
                p.ApellidoPaterno.Contains(criterio) ||
                p.ApellidoMaterno.Contains(criterio));
        }
    }
}