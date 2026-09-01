using POS_Datos;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; // Indispensable para el manejo de Tasks

namespace POS_Logica
{
    public class ProductoLogica
    {
        private readonly ProductoDatos datos = new ProductoDatos();

        // --------------------------------------------------------
        // MÉTODOS DE OPERACIÓN (Caja y Buscador) - ASÍNCRONOS
        // --------------------------------------------------------

        public async Task<Producto> BuscarProductoAsync(string codigo)
        {
            return await datos.ObtenerProductoPorCodigoAsync(codigo);
        }

        public async Task<List<Producto>> BuscarProductosPorNombreAsync(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return new List<Producto>();

            return await datos.BuscarPorNombreAsync(texto.Trim());
        }

        public async Task<bool> RegistrarProductoAsync(Producto producto)
        {
            return await GuardarProductoAsync(producto);
        }

        // --------------------------------------------------------
        // MÉTODOS DE INVENTARIO (Gestión + Paginación) - ASÍNCRONOS
        // --------------------------------------------------------

        // MIGRACIÓN A PAGINACIÓN: Calcula el salto (OFFSET) y obtiene datos en paralelo
        public async Task<(List<Producto> Productos, int TotalPaginas)> CargarPaginaInventarioAsync(int paginaActual, int elementosPorPagina)
        {
            if (paginaActual < 1) paginaActual = 1;

            // Fórmula matemática para saber cuántos registros saltarse
            int offset = (paginaActual - 1) * elementosPorPagina;

            // Ejecutamos ambas consultas en paralelo para máxima velocidad en el servidor
            var taskProductos = datos.ObtenerInventarioPaginadoAsync(elementosPorPagina, offset);
            var taskTotal = datos.ObtenerTotalProductosAsync();

            // Esperamos a que ambas tareas terminen antes de avanzar
            await Task.WhenAll(taskProductos, taskTotal);

            int totalProductos = taskTotal.Result;

            // Calculamos el total de páginas necesarias (Redondeando hacia arriba)
            int totalPaginas = (int)Math.Ceiling((double)totalProductos / elementosPorPagina);

            if (totalPaginas == 0) totalPaginas = 1;

            // Retornamos la tupla con los resultados
            return (taskProductos.Result, totalPaginas);
        }

        public async Task<bool> GuardarProductoAsync(Producto producto)
        {
            // Reglas de negocio intactas (Validaciones obligatorias antes de tocar la BD)
            if (string.IsNullOrWhiteSpace(producto.Codigo) || string.IsNullOrWhiteSpace(producto.Nombre))
                throw new Exception("El código y el nombre son obligatorios.");

            if (producto.PrecioVenta < producto.PrecioCosto)
                throw new Exception("El precio de venta no puede ser menor al costo (¡Perderías dinero!).");

            return await datos.GuardarProductoAsync(producto);
        }

        public async Task<bool> EliminarAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new Exception("El código del producto no es válido.");

            return await datos.EliminarProductoAsync(codigo);
        }

    }
}