using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace POS_Datos
{
    public class ClienteDatos
    {
        private readonly Conexion conexion = new Conexion();

        public async Task<List<Cliente>> BuscarClientesPorNombreAsync(string texto)
        {
            List<Cliente> lista = new List<Cliente>();

            // Usamos LIKE para que encuentre coincidencias parciales (ej. "Juan" encuentra "Juan Perez")
            string query = @"SELECT IdCliente, Nombre, Telefono, LimiteCredito, DeudaActual, Activo 
                             FROM Clientes 
                             WHERE Activo = 1 AND Nombre LIKE @texto 
                             ORDER BY Nombre ASC LIMIT 50";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@texto", "%" + texto + "%");
                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(reader["IdCliente"]),
                            Nombre = reader["Nombre"].ToString(),
                            Telefono = reader["Telefono"].ToString(),
                            LimiteCredito = Convert.ToDecimal(reader["LimiteCredito"]),
                            DeudaActual = Convert.ToDecimal(reader["DeudaActual"]),
                            Activo = Convert.ToBoolean(reader["Activo"])
                        });
                    }
                }
            }
            return lista;
        }
        public async Task<List<Venta>> ObtenerTicketsPendientesAsync(int idCliente)
        {
            List<Venta> lista = new List<Venta>();
            string query = @"SELECT IdVenta, Folio, FechaHora, Total, EstadoPago 
                     FROM Ventas 
                     WHERE IdCliente = @idCliente AND EstadoPago = 'Pendiente' 
                     ORDER BY FechaHora ASC";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Venta
                        {
                            IdVenta = Convert.ToInt32(reader["IdVenta"]),
                            Folio = reader["Folio"].ToString(),
                            FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                            Total = Convert.ToDecimal(reader["Total"]),
                            EstadoPago = reader["EstadoPago"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
        // Método para registrar un nuevo cliente en la base de datos
        public async Task<bool> InsertarClienteAsync(Cliente cliente)
        {
            // Usamos parámetros para evitar inyecciones SQL
            string query = @"INSERT INTO Clientes 
                             (Nombre, Telefono, CorreoElectronico, RFC, Direccion, LimiteCredito, DeudaActual, Activo) 
                             VALUES 
                             (@nombre, @telefono, @correo, @rfc, @direccion, @limiteCredito, @deudaActual, @activo)";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                cmd.Parameters.AddWithValue("@telefono", cliente.Telefono);
                cmd.Parameters.AddWithValue("@correo", cliente.CorreoElectronico);
                cmd.Parameters.AddWithValue("@rfc", cliente.RFC);
                cmd.Parameters.AddWithValue("@direccion", cliente.Direccion);
                cmd.Parameters.AddWithValue("@limiteCredito", cliente.LimiteCredito);

                // Un cliente nuevo siempre arranca con deuda cero por lógica de negocio
                cmd.Parameters.AddWithValue("@deudaActual", 0m);

                // Convertimos el booleano de C# a 1 o 0 para MySQL
                cmd.Parameters.AddWithValue("@activo", cliente.Activo ? 1 : 0);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}