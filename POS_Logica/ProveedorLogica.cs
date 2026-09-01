using POS_Datos;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; // Indispensable para usar Task

namespace POS_Logica
{
    public class ProveedorLogica
    {
        // OPTIMIZACIÓN: Blindaje de instancia en memoria
        private readonly ProveedorDatos datos = new ProveedorDatos();

        // ==============================================================
        // 1. CREATE
        // ==============================================================
        public async Task<bool> InsertarProveedorAsync(Proveedor proveedor)
        {
            // Regla de negocio básica antes de ir a la BD
            if (string.IsNullOrWhiteSpace(proveedor.Empresa))
            {
                throw new Exception("El nombre de la empresa/marca es obligatorio.");
            }

            // Mantenemos la cadena de excepciones (InnerException) como buena práctica
            try
            {
                return await datos.InsertarProveedorAsync(proveedor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el proveedor en la base de datos.", ex);
            }
        }

        // ==============================================================
        // 2. READ
        // ==============================================================
        public async Task<List<Proveedor>> ObtenerProveedoresAsync()
        {
            try
            {
                return await datos.ObtenerProveedoresAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar la lista de proveedores.", ex);
            }
        }

        // ==============================================================
        // 3. UPDATE
        // ==============================================================
        public async Task<bool> ActualizarProveedorAsync(Proveedor proveedor)
        {
            if (proveedor.IdProveedor <= 0)
            {
                throw new Exception("ID de proveedor inválido para actualización.");
            }

            if (string.IsNullOrWhiteSpace(proveedor.Empresa))
            {
                throw new Exception("El nombre de la empresa/marca no puede quedar vacío.");
            }

            try
            {
                return await datos.ActualizarProveedorAsync(proveedor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar los datos del proveedor.", ex);
            }
        }

        // ==============================================================
        // 4. DELETE
        // ==============================================================
        public async Task<bool> EliminarProveedorAsync(int idProveedor)
        {
            if (idProveedor <= 0)
            {
                throw new Exception("ID de proveedor inválido para eliminación.");
            }

            try
            {
                return await datos.EliminarProveedorAsync(idProveedor);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar dar de baja al proveedor.", ex);
            }
        }

        public async Task<bool> GuardarProveedor(Proveedor nuevoProveedor)
        {
            throw new NotImplementedException();
        }
    }
}