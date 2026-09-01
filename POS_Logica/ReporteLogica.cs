using POS_Datos;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Logica
{
    public class ReporteLogica
    {
        private readonly ReporteDatos datos = new ReporteDatos();

        public async Task<List<ReporteVenta>> GenerarReportePorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            DateTime inicioAjustado = fechaInicio.Date;
            DateTime finAjustado = fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            TimeSpan diferencia = finAjustado - inicioAjustado;
            if (diferencia.TotalDays > 366)
            {
                throw new Exception("Por motivos de rendimiento, no se pueden generar reportes de más de 1 año en una sola consulta. Por favor, acota las fechas.");
            }

            if (inicioAjustado > finAjustado)
            {
                throw new Exception("El rango es inválido: La fecha de inicio no puede ser mayor a la fecha final.");
            }

            try
            {
                return await datos.ObtenerReportePorFechasAsync(inicioAjustado, finAjustado);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al intentar generar el reporte de ventas desde la base de datos.", ex);
            }
        }
    }
}