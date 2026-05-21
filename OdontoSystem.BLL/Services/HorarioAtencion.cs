using System;
using System.Collections.Generic;

namespace OdontoSystem.BLL.Services
{
    public static class HorarioAtencion
    {
        // Horario laboral del consultorio
        public static readonly TimeSpan HoraApertura = new TimeSpan(9, 0, 0);   // 9:00 AM
        public static readonly TimeSpan HoraCierre = new TimeSpan(19, 0, 0);  // 7:00 PM

        // Duración de cada cita en minutos
        public const int DuracionCitaMinutos = 30;

        // Capacidad máxima por slot (cantidad de consultorios disponibles)
        public const int CapacidadPorSlot = 2;

        /// <summary>
        /// Devuelve la lista de horarios válidos donde se puede agendar una cita.
        /// </summary>
        public static List<TimeSpan> ObtenerSlotsDisponibles()
        {
            var slots = new List<TimeSpan>();
            var hora = HoraApertura;

            // La última cita debe terminar antes o exactamente en HoraCierre
            while (hora.Add(TimeSpan.FromMinutes(DuracionCitaMinutos)) <= HoraCierre)
            {
                slots.Add(hora);
                hora = hora.Add(TimeSpan.FromMinutes(DuracionCitaMinutos));
            }

            return slots;
        }

        /// <summary>
        /// Verifica si una hora pertenece a los slots permitidos.
        /// </summary>
        public static bool EsSlotValido(TimeSpan hora)
        {
            return ObtenerSlotsDisponibles().Contains(hora);
        }

        /// <summary>
        /// Texto descriptivo del horario para mensajes de error.
        /// </summary>
        public static string DescripcionHorario =>
            $"{HoraApertura:hh\\:mm} a {HoraCierre:hh\\:mm} en intervalos de {DuracionCitaMinutos} minutos";
    }
}