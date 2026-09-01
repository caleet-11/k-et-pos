using POS_Datos;
using System;

namespace POS_Logica
{
    public class DevolucionLogica
    {
        private readonly DevolucionDatos datos = new DevolucionDatos();

        public bool ProcesarDevolucion(int idDetalle, string codigo, decimal cantidad, decimal monto, int idUsuario, string folioVenta)
        {
            if (cantidad <= 0)
            {
                throw new Exception("La cantidad a devolver debe ser mayor a cero.");
            }

            try
            {
                return datos.GuardarDevolucion(idDetalle, codigo, cantidad, monto, idUsuario, folioVenta);
            }
            catch (Exception ex)
            {
                // Ahora sí, si MySQL se queja, la pantalla visual mostrará exactamente el por qué
                throw new Exception("No se pudo procesar la devolución.\n", ex);
            }
        }
    }
}