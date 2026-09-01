using MySql.Data.MySqlClient;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Datos
{
    public class ProveedorDatos
    {
        private readonly Conexion conexion = new Conexion();

        // ==============================================================
        // 1. CREATE: Insertar un nuevo proveedor
        // ==============================================================
        public async Task<bool> InsertarProveedorAsync(Proveedor p)
        {
            // CAMBIO: Se agrega la columna Activo al INSERT
            string query = @"INSERT INTO Proveedores (Empresa, NombreContacto, Telefono, Activo) 
                             VALUES (@empresa, @contacto, @telefono, @activo)";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@empresa", p.Empresa);
                cmd.Parameters.AddWithValue("@contacto", p.NombreContacto);
                cmd.Parameters.AddWithValue("@telefono", p.Telefono);

                // Convertimos el booleano a 1 (true) o 0 (false) para MySQL
                cmd.Parameters.AddWithValue("@activo", p.Activo ? 1 : 0);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        // ==============================================================
        // 2. READ: Listar todos los proveedores (Para alimentar la tabla visual)
        // ==============================================================
        public async Task<List<Proveedor>> ObtenerProveedoresAsync()
        {
            List<Proveedor> lista = new List<Proveedor>();

            // CAMBIO: Se agrega "WHERE Activo = 1" para filtrar la baja lógica y se extrae el campo Activo
            string query = "SELECT IdProveedor, Empresa, NombreContacto, Telefono, Activo FROM Proveedores WHERE Activo = 1 ORDER BY Empresa ASC";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Proveedor
                        {
                            IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                            Empresa = reader["Empresa"].ToString(),
                            NombreContacto = reader["NombreContacto"].ToString(),
                            Telefono = reader["Telefono"].ToString(),
                            // Mapeamos el estatus de MySQL a nuestra entidad
                            Activo = Convert.ToBoolean(reader["Activo"])
                        });
                    }
                }
            }
            return lista;
        }

        // ==============================================================
        // 3. UPDATE: Actualizar datos de un proveedor existente
        // ==============================================================
        public async Task<bool> ActualizarProveedorAsync(Proveedor p)
        {
            string query = @"UPDATE Proveedores 
                             SET Empresa = @empresa, NombreContacto = @contacto, Telefono = @telefono 
                             WHERE IdProveedor = @id";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", p.IdProveedor);
                cmd.Parameters.AddWithValue("@empresa", p.Empresa);
                cmd.Parameters.AddWithValue("@contacto", p.NombreContacto);
                cmd.Parameters.AddWithValue("@telefono", p.Telefono);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        // ==============================================================
        // 4. DELETE: Dar de baja/Eliminar un proveedor de la base de datos
        // ==============================================================
        public async Task<bool> EliminarProveedorAsync(int idProveedor)
        {
            // CAMBIO MAGISTRAL: El DELETE físico se convierte en un UPDATE lógico
            string query = "UPDATE Proveedores SET Activo = 0 WHERE IdProveedor = @id";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", idProveedor);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}