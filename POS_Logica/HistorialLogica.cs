using POS_Datos;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Logica
{
    public class HistorialLogica
    {
        // OPTIMIZACIÓN: Se agregó 'readonly' para proteger la instancia y mejorar la gestión de memoria
        private readonly HistorialDatos datos = new HistorialDatos();

        // 1. ASÍNCRONO: Listar últimos tickets
        public async Task<List<Venta>> ListarTicketsAsync()
        {
            // 'await' cede el control mientras la base de datos trabaja
            return await datos.ObtenerTicketsRecientesAsync();
        }

        // 2. ASÍNCRONO: Ver detalles de un ticket
        public async Task<List<VentaDetalle>> VerDetallesAsync(int idVenta)
        {
            return await datos.ObtenerDetalleDeTicketAsync(idVenta);
        }

        // 3. ASÍNCRONO: Buscar por fechas
        public async Task<List<Venta>> BuscarTicketsPorRangoAsync(DateTime inicio, DateTime fin)
        {
            // Regla de negocio: El inicio no puede ser un viaje al futuro comparado con el fin
            if (inicio > fin)
            {
                throw new Exception("La fecha de inicio no puede ser mayor a la fecha de fin.");
            }

            return await datos.ObtenerTicketsPorFechasAsync(inicio, fin);
        }
    }
}