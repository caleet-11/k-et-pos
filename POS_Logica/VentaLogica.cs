using POS_Datos;
using POS_Entidades;
using System;
using System.Linq;
using System.Threading.Tasks; // Indispensable para el manejo de hilos asíncronos

namespace POS_Logica
{
    public class VentaLogica
    {
        private readonly VentaDatos ventaDatos = new VentaDatos();

        /// <summary>
        /// Valida el ticket y lo envía a la capa de datos para la transacción segura de forma asíncrona.
        /// </summary>
        public async Task<(bool Sucedio, string Mensaje)> ProcesarVentaAsync(Venta venta)
        {
            // 1. Validar que el ticket no esté vacío
            if (venta == null || venta.Detalles == null || !venta.Detalles.Any())
            {
                return (false, "El ticket está vacío. Escanea productos antes de cobrar.");
            }

            // 2. Validar que el total tenga sentido
            if (venta.Total <= 0)
            {
                return (false, "El total de la venta debe ser mayor a cero.");
            }

            // 3. Procesar en la base de datos de forma asíncrona
            try
            {
                // OPTIMIZACIÓN: Esperamos la respuesta de la base de datos sin congelar la UI
                bool resultado = await ventaDatos.GuardarVentaAsync(venta);

                if (resultado)
                {
                    return (true, "¡Venta completada con éxito!");
                }
                else
                {
                    return (false, "No se pudo guardar la venta. Intenta nuevamente.");
                }
            }
            catch (Exception ex)
            {
                // Si la transacción falla en MySQL, devolvemos el error controlado a la pantalla
                return (false, "Error al procesar la transacción: " + ex.Message);
            }
        }
    }
}