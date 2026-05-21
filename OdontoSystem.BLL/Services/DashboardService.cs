using System;
using System.Linq;
using OdontoSystem.DAL.Context;

namespace OdontoSystem.BLL.Services
{
    /// <summary>
    /// Servicio que provee estadísticas resumidas para el dashboard principal.
    /// </summary>
    public class DashboardService
    {
        public DashboardStats ObtenerEstadisticas()
        {
            var stats = new DashboardStats();

            try
            {
                using (var ctx = new OdontoContext())
                {
                    stats.TotalPacientes = ctx.Pacientes.Count(p => p.Estado == "A");
                    stats.CitasPendientes = ctx.Citas.Count(c => c.Estado == "Pendiente");
                    stats.CitasHoy = ctx.Citas.Count(c => c.Estado == "Pendiente"
                                                              && c.FechaCita == DateTime.Today);
                    stats.Tratamientos = ctx.CatalogoTratamientos.Count(t => t.Estado == "A");
                    stats.TotalUsuarios = ctx.Usuarios.Count(u => u.Estado == "A");
                }
            }
            catch (Exception ex)
            {
                stats.Error = "Error al cargar estadísticas: " + ex.Message;
            }

            return stats;
        }
    }

    /// <summary>
    /// DTO con los números para el dashboard.
    /// </summary>
    public class DashboardStats
    {
        public int TotalPacientes { get; set; }
        public int CitasPendientes { get; set; }
        public int CitasHoy { get; set; }
        public int Tratamientos { get; set; }
        public int TotalUsuarios { get; set; }
        public string Error { get; set; }
    }
}