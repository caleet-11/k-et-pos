using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System;

namespace POS_Datos
{
    public class CajaDatos
    {
        private readonly Conexion conexion = new Conexion();

        public async Task<DateTime?> ObtenerUltimaAperturaAsync(int idUsuario)
        {
            DateTime? fechaApertura = null;

            // Adaptado a tu tabla Caja_Turnos y columna EstadoActivo
            string query = @"SELECT FechaApertura FROM Caja_Turnos 
                             WHERE IdUsuario = @idUsuario AND EstadoActivo = 1 
                             ORDER BY FechaApertura DESC LIMIT 1";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                await conn.OpenAsync();

                object resultado = await cmd.ExecuteScalarAsync();

                if (resultado != null && resultado != DBNull.Value)
                {
                    fechaApertura = Convert.ToDateTime(resultado);
                }
            }
            return fechaApertura;
        }

        public async Task<bool> AbrirCajaAsync(int idUsuario, decimal montoInicial)
        {
            // Adaptado a tu tabla Caja_Turnos y EstadoActivo = 1
            string query = @"INSERT INTO Caja_Turnos (IdUsuario, FechaApertura, MontoInicial, EstadoActivo) 
                             VALUES (@idUsuario, @fechaApertura, @montoInicial, 1)";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@fechaApertura", DateTime.Now);
                cmd.Parameters.AddWithValue("@montoInicial", montoInicial);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> CerrarCajaAsync(int idUsuario, decimal montoCierre)
        {
            // Adaptado a tu tabla Caja_Turnos y cambiando EstadoActivo a 0
            string query = @"UPDATE Caja_Turnos 
                             SET FechaCierre = @fechaCierre, MontoCierre = @montoCierre, EstadoActivo = 0 
                             WHERE IdUsuario = @idUsuario AND EstadoActivo = 1";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@fechaCierre", DateTime.Now);
                cmd.Parameters.AddWithValue("@montoCierre", montoCierre);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}