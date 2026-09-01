using POS_Entidades;
using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace POS_Datos
{
    public class VentaDatos
    {
        private readonly Conexion conexion = new Conexion();

        /// <summary>
        /// Registra la venta, sus detalles y actualiza el inventario de forma asíncrona en una sola transacción segura.
        /// </summary>
        public async Task<bool> GuardarVentaAsync(Venta venta)
        {
            bool exito = false;

            using (MySqlConnection conn = conexion.ObtenerConexion())
            {
                await conn.OpenAsync();

                // Iniciamos la transacción de forma asíncrona
                using (MySqlTransaction transaccion = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // =========================================================================
                        // 1. OBTENER FOLIO (Bloqueo Atómico Multicaja)
                        // =========================================================================
                        int consecutivo = 1;

                        // OPTIMIZACIÓN: Usamos COALESCE + MAX para obtener el último ID real.
                        // FOR UPDATE bloquea la fila a nivel de motor (InnoDB) evitando condiciones de carrera.
                        string queryFolio = "SELECT COALESCE(MAX(IdVenta), 0) + 1 FROM Ventas FOR UPDATE;";

                        using (MySqlCommand cmdFolio = new MySqlCommand(queryFolio, conn, transaccion))
                        {
                            consecutivo = Convert.ToInt32(await cmdFolio.ExecuteScalarAsync());
                        }

                        string nuevoFolio = "TKT-" + consecutivo.ToString("D5");

                        // 2. INSERTAR ENCABEZADO DE LA VENTA (Ahora incluye EstadoPago)
                        string queryVenta = @"INSERT INTO Ventas (Folio, FechaHora, Total, IdUsuario, TipoOperacion, IdCliente, EstadoPago) 
                                              VALUES (@folio, @fechaHora, @total, @idUsuario, @tipoOperacion, @idCliente, @estadoPago);
                                              SELECT LAST_INSERT_ID();";

                        int idVentaGenerado = 0;
                        string tipoOp = string.IsNullOrEmpty(venta.TipoOperacion) ? "Efectivo" : venta.TipoOperacion;
                        string estadoPago = (tipoOp == "Fiado") ? "Pendiente" : "Pagado";

                        using (MySqlCommand cmdVenta = new MySqlCommand(queryVenta, conn, transaccion))
                        {
                            cmdVenta.Parameters.AddWithValue("@folio", nuevoFolio);
                            cmdVenta.Parameters.AddWithValue("@fechaHora", DateTime.Now);

                            // Si es Autoconsumo, el total para la caja es 0, si no, es el total normal
                            cmdVenta.Parameters.AddWithValue("@total", tipoOp == "Autoconsumo" ? 0 : venta.Total);

                            cmdVenta.Parameters.AddWithValue("@idUsuario", venta.IdUsuario > 0 ? venta.IdUsuario : 1);

                            cmdVenta.Parameters.AddWithValue("@tipoOperacion", tipoOp);
                            cmdVenta.Parameters.AddWithValue("@idCliente", venta.IdCliente.HasValue ? venta.IdCliente.Value : (object)DBNull.Value);
                            cmdVenta.Parameters.AddWithValue("@estadoPago", estadoPago);

                            idVentaGenerado = Convert.ToInt32(await cmdVenta.ExecuteScalarAsync());
                        }

                        // =========================================================================
                        // 2.5 ACTUALIZAR DEUDA DEL CLIENTE (Solo si es Fiado)
                        // =========================================================================
                        if (tipoOp == "Fiado" && venta.IdCliente.HasValue)
                        {
                            string queryDeuda = "UPDATE Clientes SET DeudaActual = DeudaActual + @totalFiado WHERE IdCliente = @idClienteFiado;";
                            using (MySqlCommand cmdDeuda = new MySqlCommand(queryDeuda, conn, transaccion))
                            {
                                cmdDeuda.Parameters.AddWithValue("@totalFiado", venta.Total);
                                cmdDeuda.Parameters.AddWithValue("@idClienteFiado", venta.IdCliente.Value);
                                await cmdDeuda.ExecuteNonQueryAsync();
                            }
                        }

                        // 3. CONSULTAS PREPARADAS (DETALLES E INVENTARIO)
                        string queryDetalle = @"INSERT INTO Ventas_Detalles (IdVenta, Codigo, Nombre, Cantidad, PrecioCosto, PrecioVenta, Subtotal) 
                                                VALUES (@idVenta, @codigo, @nombre, @cantidad, @precioCosto, @precioVenta, @subtotal);";

                        string queryActualizarStock = @"UPDATE Productos 
                                                       SET Stock = Stock - @cantidad 
                                                       WHERE Codigo = @codigo;";

                        using (MySqlCommand cmdDetalle = new MySqlCommand(queryDetalle, conn, transaccion))
                        using (MySqlCommand cmdStock = new MySqlCommand(queryActualizarStock, conn, transaccion))
                        {
                            cmdDetalle.Parameters.AddWithValue("@idVenta", 0);
                            cmdDetalle.Parameters.AddWithValue("@codigo", "");
                            cmdDetalle.Parameters.AddWithValue("@nombre", "");
                            cmdDetalle.Parameters.AddWithValue("@cantidad", 0m);
                            cmdDetalle.Parameters.AddWithValue("@precioCosto", 0m);
                            cmdDetalle.Parameters.AddWithValue("@precioVenta", 0m);
                            cmdDetalle.Parameters.AddWithValue("@subtotal", 0m);

                            cmdStock.Parameters.AddWithValue("@cantidad", 0m);
                            cmdStock.Parameters.AddWithValue("@codigo", "");

                            foreach (var detalle in venta.Detalles)
                            {
                                decimal subtotalCalculado = detalle.Cantidad * detalle.PrecioVenta;

                                cmdDetalle.Parameters["@idVenta"].Value = idVentaGenerado;
                                cmdDetalle.Parameters["@codigo"].Value = detalle.Codigo;
                                cmdDetalle.Parameters["@nombre"].Value = detalle.Nombre;
                                cmdDetalle.Parameters["@cantidad"].Value = detalle.Cantidad;
                                cmdDetalle.Parameters["@precioCosto"].Value = detalle.PrecioCosto;
                                cmdDetalle.Parameters["@precioVenta"].Value = detalle.PrecioVenta;
                                cmdDetalle.Parameters["@subtotal"].Value = subtotalCalculado;

                                // Ejecución asíncrona dentro del ciclo
                                await cmdDetalle.ExecuteNonQueryAsync();

                                cmdStock.Parameters["@cantidad"].Value = detalle.Cantidad;
                                cmdStock.Parameters["@codigo"].Value = detalle.Codigo;

                                // Ejecución asíncrona del inventario
                                await cmdStock.ExecuteNonQueryAsync();
                            }
                        }

                        // Confirmamos la transacción
                        await transaccion.CommitAsync();
                        exito = true;
                    }
                    catch (Exception ex)
                    {
                        // Si algo explota, deshacemos todo de forma segura
                        await transaccion.RollbackAsync();
                        throw new Exception("Error al guardar la venta en la base de datos.", ex);
                    }
                }
            }

            return exito;
        }
    }
}