using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using OdontoSystem.DAL.Context;
using OdontoSystem.DAL.Repositories;
using OdontoSystem.Entities;

namespace OdontoSystem.BLL.Services
{
    public class CitaService
    {
        public IEnumerable<Cita> Listar()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Citas
                          .Include(c => c.Paciente)
                          .Include(c => c.Odontologo)
                          .OrderByDescending(c => c.FechaCita)
                          .ThenByDescending(c => c.HoraCita)
                          .ToList();
            }
        }

        public Cita ObtenerPorId(int id)
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Citas
                          .Include(c => c.Paciente)
                          .Include(c => c.Odontologo)
                          .FirstOrDefault(c => c.IdCita == id);
            }
        }

        public void Agendar(Cita cita)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new CitaRepository(ctx);

                if (repo.ExisteConflicto(cita.IdOdontologo, cita.FechaCita, cita.HoraCita))
                    throw new InvalidOperationException("Horario no disponible para ese odontólogo");

                cita.Estado = "Pendiente";
                cita.FechaRegistro = DateTime.Now;
                repo.Add(cita);
                repo.SaveChanges();
            }
        }

        public void Cancelar(int id)
        {
            using (var ctx = new OdontoContext())
            {
                var repo = new CitaRepository(ctx);
                var cita = repo.GetById(id);
                if (cita == null) throw new InvalidOperationException("Cita no encontrada");
                if (cita.Estado == "Atendida")
                    throw new InvalidOperationException("No se puede modificar una cita ya atendida");

                cita.Estado = "Cancelada";
                cita.FechaModificacion = DateTime.Now;
                repo.Update(cita);
                repo.SaveChanges();
            }
        }

        // Datos para los dropdowns del formulario
        public IEnumerable<Paciente> ListarPacientesActivos()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Pacientes
                          .Where(p => p.Estado == "A")
                          .OrderBy(p => p.ApellidoPaterno)
                          .ToList();
            }
        }

        public IEnumerable<Usuario> ListarOdontologosActivos()
        {
            using (var ctx = new OdontoContext())
            {
                return ctx.Usuarios
                          .Where(u => u.Estado == "A" && u.Rol.Descripcion == "Odontologo")
                          .ToList();
            }
        }
    }
}