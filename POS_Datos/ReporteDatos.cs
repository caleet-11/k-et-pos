using MySql.Data.MySqlClient;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Datos
{
    public class ReporteDatos
    {
        private readonly Conexion conexion = new Conexion();

        public async Task<List<ReporteVenta>> ObtenerReportePorFechasAsync(DateTime inicioAjustado, DateTime finAjustado)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            // OPTIMIZACIÓN EXTREMA: JOIN con agrupación para calcular la utilidad neta directo en el motor
            string query = @"SELECT v.IdVenta, v.Folio, v.FechaHora, v.Total AS IngresoBruto,
                             SUM((d.Cantidad - d.CantidadDevuelta) * (d.PrecioVenta - d.PrecioCosto)) AS GananciaNeta
                             FROM Ventas v
                             INNER JOIN Ventas_Detalles d ON v.IdVenta = d.IdVenta
                             WHERE v.FechaHora BETWEEN @inicio AND @fin
                             GROUP BY v.IdVenta, v.Folio, v.FechaHora, v.Total
                             ORDER BY v.FechaHora DESC";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@inicio", inicioAjustado);
                cmd.Parameters.AddWithValue("@fin", finAjustado);

                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new ReporteVenta
                        {
                            IdVenta = Convert.ToInt32(reader["IdVenta"]),
                            Folio = reader["Folio"].ToString(),
                            Reloj = Convert.ToDateTime(reader["FechaHora"]),
                            IngresoBruto = Convert.ToDecimal(reader["IngresoBruto"]),
                            // Si el resultado del SUM es NULL (por ejemplo, si no hubiera detalles), ponemos 0
                            GananciaNeta = reader["GananciaNeta"] != DBNull.Value ? Convert.ToDecimal(reader["GananciaNeta"]) : 0
                        });
                    }
                }
            }
            return lista;
        }
    }
}