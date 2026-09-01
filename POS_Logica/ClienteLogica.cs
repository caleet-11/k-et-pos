using POS_Entidades;
using POS_Datos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Logica
{
    public class ClienteLogica
    {
        private readonly ClienteDatos datos = new ClienteDatos();

        public async Task<List<Cliente>> BuscarClientesAsync(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<Cliente>();

            return await datos.BuscarClientesPorNombreAsync(texto.Trim());
        }
        public async Task<List<Venta>> ObtenerTicketsPendientesAsync(int idCliente)
        {
            if (idCliente <= 0) return new List<Venta>();
            return await datos.ObtenerTicketsPendientesAsync(idCliente);
        }
    }
}