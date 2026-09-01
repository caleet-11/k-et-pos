using MySql.Data.MySqlClient;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; 

namespace POS_Datos
{
    public class ProductoDatos
    {
        private readonly Conexion conexion = new Conexion();

        // ==============================================================
        // 1. MÉTODOS DE OPERACIÓN (Asíncronos para el Punto de Venta)
        // ==============================================================

        public async Task<Producto> ObtenerProductoPorCodigoAsync(string codigo)
        {
            Producto p = null;
            string query = @"SELECT Codigo, Nombre, PrecioCosto, PrecioVenta, Stock, SeVendePorUnidad,
                                    Marca, Proveedor, ControlaStock, StockMinimo, StockIdeal 
                             FROM Productos 
                             WHERE Codigo = @codigo";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@codigo", codigo);
                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        p = MapearProducto(reader);
                    }
                }
            }
            return p;
        }

        public async Task<List<Producto>> BuscarPorNombreAsync(string textoBusqueda)
        {
            List<Producto> lista = new List<Producto>();

            // SOLUCIÓN: Agregamos 'Activo = 1 AND ' justo antes del MATCH
            string query = @"SELECT Codigo, Nombre, PrecioCosto, PrecioVenta, Stock, SeVendePorUnidad,
                            Marca, Proveedor, ControlaStock, StockMinimo, StockIdeal 
                     FROM Productos 
                     WHERE Activo = 1 AND MATCH(Nombre, Codigo, Marca, Proveedor) AGAINST(@texto IN BOOLEAN MODE)
                     LIMIT 50";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                string textoBooleano = string.Join(" ", textoBusqueda.Split(' ').Select(word => word + "*"));
                cmd.Parameters.AddWithValue("@texto", textoBooleano);

                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearProducto(reader));
                    }
                }
            }
            return lista;
        }

        // ==============================================================
        // 2. MÉTODOS DE INVENTARIO (Paginación + Asíncronismo)
        // ==============================================================

        // Paso 9: Carga solo un "fragmento" del inventario
        public async Task<List<Producto>> ObtenerInventarioPaginadoAsync(int limite, int offset)
        {
            List<Producto> lista = new List<Producto>();

            // CORRECCIÓN 1: Agregamos el 'WHERE Activo = 1' justo antes del ORDER BY
            string query = @"SELECT Codigo, Nombre, PrecioCosto, PrecioVenta, Stock, SeVendePorUnidad,
                            Marca, Proveedor, ControlaStock, StockMinimo, StockIdeal 
                     FROM Productos 
                     WHERE Activo = 1 
                     ORDER BY Nombre ASC 
                     LIMIT @limite OFFSET @offset";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@limite", limite);
                cmd.Parameters.AddWithValue("@offset", offset);
                await conn.OpenAsync();

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(MapearProducto(reader));
                    }
                }
            }
            return lista;
        }

        // Útil para calcular cuántas páginas mostrar en la interfaz
        public async Task<int> ObtenerTotalProductosAsync()
        {
            // CORRECCIÓN 2: Regresamos al COUNT(*) para que ExecuteScalar funcione y le agregamos el filtro
            string queryCount = "SELECT COUNT(*) FROM Productos WHERE Activo = 1";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(queryCount, conn))
            {
                await conn.OpenAsync();
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        // ==============================================================
        // 3. ESCRITURA Y BORRADO (Asíncronos)
        // ==============================================================

        public async Task<bool> GuardarProductoAsync(Producto p)
        {
            // SOLUCIÓN: Inyectamos explícitamente el '1' en el INSERT y 'Activo=1' en el UPDATE
            string query = @"INSERT INTO Productos 
                     (Codigo, Nombre, PrecioCosto, PrecioVenta, Stock, SeVendePorUnidad, Marca, Proveedor, ControlaStock, StockMinimo, StockIdeal, Activo) 
                     VALUES 
                     (@codigo, @nombre, @costo, @venta, @stock, @unidad, @marca, @proveedor, @controla, @minimo, @ideal, 1)
                     ON DUPLICATE KEY UPDATE 
                     Nombre=@nombre, PrecioCosto=@costo, PrecioVenta=@venta, Stock=@stock, SeVendePorUnidad=@unidad,
                     Marca=@marca, Proveedor=@proveedor, ControlaStock=@controla, StockMinimo=@minimo, StockIdeal=@ideal, Activo=1;";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@codigo", p.Codigo);
                cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                cmd.Parameters.AddWithValue("@costo", p.PrecioCosto);
                cmd.Parameters.AddWithValue("@venta", p.PrecioVenta);
                cmd.Parameters.AddWithValue("@stock", p.Stock);
                cmd.Parameters.AddWithValue("@unidad", p.SeVendePorUnidad);
                cmd.Parameters.AddWithValue("@marca", p.Marca ?? "");
                cmd.Parameters.AddWithValue("@proveedor", p.Proveedor ?? "");
                cmd.Parameters.AddWithValue("@controla", p.ControlaStock);
                cmd.Parameters.AddWithValue("@minimo", p.StockMinimo);
                cmd.Parameters.AddWithValue("@ideal", p.StockIdeal);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> EliminarProductoAsync(string codigo)
        {
            // Ya no hacemos DELETE. Ahora solo lo marcamos como Activo = 0 (Oculto)
            string query = "UPDATE Productos SET Activo = 0 WHERE Codigo = @codigo";

            using (MySqlConnection conn = conexion.ObtenerConexion())
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@codigo", codigo);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        // Helper para evitar repetir código de mapeo
        private Producto MapearProducto(MySqlDataReader reader)
        {
            return new Producto
            {
                Codigo = reader["Codigo"].ToString(),
                Nombre = reader["Nombre"].ToString(),
                PrecioCosto = Convert.ToDecimal(reader["PrecioCosto"]),
                PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                Stock = Convert.ToDecimal(reader["Stock"]),
                SeVendePorUnidad = Convert.ToBoolean(reader["SeVendePorUnidad"]),
                Marca = reader["Marca"].ToString(),
                Proveedor = reader["Proveedor"].ToString(),
                ControlaStock = Convert.ToBoolean(reader["ControlaStock"]),
                StockMinimo = Convert.ToDecimal(reader["StockMinimo"]),
                StockIdeal = Convert.ToDecimal(reader["StockIdeal"])
            };
        }
    }
}