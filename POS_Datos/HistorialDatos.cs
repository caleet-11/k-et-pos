using MySql.Data.MySqlClient;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Datos
{
    public class HistorialDatos
    {
        private readonly Conexion conexion = new Conexion();

        // 1. ASÍNCRONO: Traer los últimos 50 tickets
        public async Task<List<Venta>> ObtenerTicketsRecientesAsync()
        {
            List<Venta> lista = new List<Venta>();
            string query = "SELECT IdVenta, Folio, FechaHora, Total FROM Ventas ORDER BY FechaHora DESC LIMIT 50";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                await conn.OpenAsync();

                // CORRECCIÓN: Agregamos el cast explícito (MySqlDataReader)
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Venta
                        {
                            IdVenta = Convert.ToInt32(reader["IdVenta"]),
                            Folio = reader["Folio"].ToString(),
                            FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                            Total = Convert.ToDecimal(reader["Total"])
                        });
                    }
                }
            }
            return lista;
        }

        // 2. ASÍNCRONO: Traer los artículos vendidos en ese folio específico
        public async Task<List<VentaDetalle>> ObtenerDetalleDeTicketAsync(int idVenta)
        {
            List<VentaDetalle> lista = new List<VentaDetalle>();
            string query = @"SELECT IdDetalle, Codigo, Nombre, Cantidad, PrecioCosto, PrecioVenta, Subtotal, FueDevuelto, CantidadDevuelta 
                             FROM Ventas_Detalles 
                             WHERE IdVenta = @id";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", idVenta);

                await conn.OpenAsync();

                // CORRECCIÓN: Agregamos el cast explícito (MySqlDataReader)
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new VentaDetalle
                        {
                            IdDetalle = Convert.ToInt32(reader["IdDetalle"]),
                            IdVenta = idVenta,
                            Codigo = reader["Codigo"].ToString(),
                            Nombre = reader["Nombre"].ToString(),
                            Cantidad = Convert.ToDecimal(reader["Cantidad"]),
                            PrecioCosto = Convert.ToDecimal(reader["PrecioCosto"]),
                            PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                            Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                            FueDevuelto = Convert.ToBoolean(reader["FueDevuelto"]),
                            CantidadDevuelta = Convert.ToDecimal(reader["CantidadDevuelta"])
                        });
                    }
                }
            }
            return lista;
        }

        // 3. ASÍNCRONO: Buscar por fechas
        public async Task<List<Venta>> ObtenerTicketsPorFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Venta> lista = new List<Venta>();
            DateTime finAjustado = fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            string query = @"SELECT IdVenta, Folio, FechaHora, Total 
                             FROM Ventas 
                             WHERE FechaHora BETWEEN @inicio AND @fin 
                             ORDER BY FechaHora DESC";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@inicio", fechaInicio.Date);
                cmd.Parameters.AddWithValue("@fin", finAjustado);

                await conn.OpenAsync();

                // CORRECCIÓN: Agregamos el cast explícito (MySqlDataReader)
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Venta
                        {
                            IdVenta = Convert.ToInt32(reader["IdVenta"]),
                            Folio = reader["Folio"].ToString(),
                            FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                            Total = Convert.ToDecimal(reader["Total"])
                        });
                    }
                }
            }
            return lista;
        }
    }
}