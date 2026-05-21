using OdontoSystem.DAL.Context;
using OdontoSystem.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdontoSystem.DAL.Repositories
{
    public class CitaRepository : Repository<Cita>
    {
        public CitaRepository(OdontoContext context) : base(context) { }

        // HU-03: validación anti-conflicto
        public bool ExisteConflicto(int idOdontologo, DateTime fecha, TimeSpan hora) =>
            _dbSet.Any(c => c.IdOdontologo == idOdontologo
                         && c.FechaCita == fecha
                         && c.HoraCita == hora
                         && c.FechaCita == fecha
                         && c.HoraCita == hora
                         && c.Estado != "Cancelada");
    }
}