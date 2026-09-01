using MySql.Data.MySqlClient;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Datos
{
    public class UsuarioDatos
    {
        private readonly Conexion conexion = new Conexion();

        public async Task<Usuario> ObtenerUsuarioPorUsernameAsync(string nombreUsuario)
        {
            Usuario u = null;

            // Ajustado a las columnas y propiedades reales de tu clase Usuario
            string query = "SELECT IdUsuario, Nombre, NombreUsuario, Contrasena, IdRol FROM Usuarios WHERE NombreUsuario = @user";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@user", nombreUsuario);

                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        u = new Usuario
                        {
                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                            Nombre = reader["Nombre"].ToString(),
                            NombreUsuario = reader["NombreUsuario"].ToString(),
                            Contrasena = reader["Contrasena"].ToString(), // Aquí se lee el Hash guardado en BD
                            IdRol = Convert.ToInt32(reader["IdRol"])
                        };
                    }
                }
            }
            return u;
        }

        public async Task<bool> InsertarUsuarioAsync(Usuario u)
        {
            // Ajustado para usar tus nombres de propiedades originales
            string query = "INSERT INTO Usuarios (Nombre, NombreUsuario, Contrasena, IdRol) VALUES (@nombre, @user, @pass, @idRol)";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nombre", u.Nombre);
                cmd.Parameters.AddWithValue("@user", u.NombreUsuario);
                cmd.Parameters.AddWithValue("@pass", u.Contrasena); // Aquí viaja el Hash ya encriptado
                cmd.Parameters.AddWithValue("@idRol", u.IdRol);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
        // Agrégalos al final de tu clase UsuarioDatos.cs

        public async Task<List<Usuario>> ListarActivosAsync()
        {
            List<Usuario> lista = new List<Usuario>();
            // Ajusta la consulta a tus nombres de columnas reales si difieren
            string query = "SELECT IdUsuario, Nombre, NombreUsuario, IdRol FROM Usuarios";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Usuario
                        {
                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                            Nombre = reader["Nombre"].ToString(),
                            NombreUsuario = reader["NombreUsuario"].ToString(),
                            IdRol = Convert.ToInt32(reader["IdRol"])
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<bool> EliminarUsuarioAsync(int idUsuario)
        {
            string query = "DELETE FROM Usuarios WHERE IdUsuario = @id";
            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", idUsuario);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<List<Rol>> ListarRolesAsync()
        {
            List<Rol> lista = new List<Rol>();
            string query = "SELECT IdRol, NombreRol FROM Roles";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new Rol
                        {
                            IdRol = Convert.ToInt32(reader["IdRol"]),
                            NombreRol = reader["NombreRol"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
        public async Task ActualizarIntentosFallidosAsync(int idUsuario, int intentos, bool bloqueado)
        {
            string query = "UPDATE Usuarios SET IntentosFallidos = @intentos, Bloqueado = @bloqueado WHERE IdUsuario = @id";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@intentos", intentos);
                cmd.Parameters.AddWithValue("@bloqueado", bloqueado);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}