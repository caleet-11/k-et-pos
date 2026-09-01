using POS_Entidades;
using System;
using MySql.Data.MySqlClient;

namespace POS_Datos
{
    public class DevolucionDatos
    {
        private readonly Conexion conexion = new Conexion();

        // Asegúrate de que los parámetros coincidan (el último debe ser string folioVenta)
        public bool GuardarDevolucion(int idDetalle, string codigo, decimal cantidad, decimal monto, int idUsuario, string folioVenta)
        {
            bool exito = false;

            using (MySqlConnection conn = conexion.ObtenerConexion())
            {
                conn.Open();

                // Usamos una transacción: si falla el stock o el ticket, se cancela todo para no perder dinero
                using (MySqlTransaction transaccion = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. REGISTRAR LA DEVOLUCIÓN EN EL HISTORIAL
                        string queryInsert = @"INSERT INTO Devoluciones (IdDetalle, Codigo, Cantidad, MontoReembolsado, FechaHora, IdUsuario, FolioVenta) 
                                               VALUES (@idDetalle, @codigo, @cantidad, @monto, @fecha, @idUsuario, @folio)";

                        using (MySqlCommand cmd1 = new MySqlCommand(queryInsert, conn, transaccion))
                        {
                            cmd1.Parameters.AddWithValue("@idDetalle", idDetalle);
                            cmd1.Parameters.AddWithValue("@codigo", codigo);
                            cmd1.Parameters.AddWithValue("@cantidad", cantidad);
                            cmd1.Parameters.AddWithValue("@monto", monto);
                            cmd1.Parameters.AddWithValue("@fecha", DateTime.Now);

                            // Blindaje: Si por alguna razón el usuario es 0 (ej. no han iniciado sesión), usamos el Admin (1)
                            cmd1.Parameters.AddWithValue("@idUsuario", idUsuario > 0 ? idUsuario : 1);
                            cmd1.Parameters.AddWithValue("@folio", folioVenta);

                            cmd1.ExecuteNonQuery();
                        }

                        // 2. ACTUALIZAR EL TICKET (Marcar que se devolvió cierta cantidad)
                        string queryUpdateDetalle = @"UPDATE Ventas_Detalles 
                                                      SET FueDevuelto = 1, CantidadDevuelta = CantidadDevuelta + @cantidad 
                                                      WHERE IdDetalle = @idDetalle";

                        using (MySqlCommand cmd2 = new MySqlCommand(queryUpdateDetalle, conn, transaccion))
                        {
                            cmd2.Parameters.AddWithValue("@cantidad", cantidad);
                            cmd2.Parameters.AddWithValue("@idDetalle", idDetalle);
                            cmd2.ExecuteNonQuery();
                        }

                        // 3. REGRESAR EL PRODUCTO AL INVENTARIO FISICO (Stock)
                        string queryUpdateStock = @"UPDATE Productos 
                                                    SET Stock = Stock + @cantidad 
                                                    WHERE Codigo = @codigo";

                        using (MySqlCommand cmd3 = new MySqlCommand(queryUpdateStock, conn, transaccion))
                        {
                            cmd3.Parameters.AddWithValue("@cantidad", cantidad);
                            cmd3.Parameters.AddWithValue("@codigo", codigo);
                            cmd3.ExecuteNonQuery();
                        }

                        // Si las 3 consultas pasaron, cerramos el trato
                        transaccion.Commit();
                        exito = true;
                    }
                    catch (Exception ex)
                    {
                        transaccion.Rollback();
                        // Pasamos el error real hacia arriba para que puedas leerlo si vuelve a fallar
                        throw new Exception("Error en MySQL: " + ex.Message);
                    }
                }
            }
            return exito;
        }
    }
}