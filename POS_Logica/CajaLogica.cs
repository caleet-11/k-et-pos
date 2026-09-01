using POS_Datos;
using POS_Entidades;
using System;
using System.Threading.Tasks;

namespace POS_Logica
{
    public class CajaLogica
    {
        private readonly CajaDatos datos = new CajaDatos();

        // 1. VERIFICAR ESTADO
        public async Task<DateTime?> ObtenerUltimaAperturaAsync(int idUsuario)
        {
            try
            {
                // Retorna la fecha exacta si el turno está abierto, o 'null' si está cerrado
                return await datos.ObtenerUltimaAperturaAsync(idUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al consultar el estado de la caja.", ex);
            }
        }

        // 2. ABRIR EL TURNO
        public async Task<bool> AbrirCajaAsync(int idUsuario, decimal montoInicial)
        {
            // Regla de negocio: El cajero no puede empezar con dinero negativo
            if (montoInicial < 0)
                throw new Exception("El fondo inicial de caja no puede ser negativo.");

            // CORRECCIÓN: Agregamos el 'await' dentro de los paréntesis para obtener el DateTime? real antes del .HasValue
            if ((await ObtenerUltimaAperturaAsync(idUsuario)).HasValue)
                throw new Exception("Ya tienes un turno de caja abierto en este momento.");

            return await datos.AbrirCajaAsync(idUsuario, montoInicial);
        }

        // 3. CERRAR EL TURNO
        // CORRECCIÓN: Convertido a async Task<bool> para poder usar 'await' en la verificación
        public async Task<bool> CerrarCajaAsync(int idUsuario, decimal montoCierre)
        {
            if (montoCierre < 0)
                throw new Exception("El monto de cierre no puede ser negativo.");

            // CORRECCIÓN: Agregamos el 'await' para evaluar correctamente si hay un turno activo
            if (!(await ObtenerUltimaAperturaAsync(idUsuario)).HasValue)
                throw new Exception("La caja ya está cerrada. No hay un turno activo para cortar.");

            // Ajustado para llamar a la capa de datos de forma asíncrona conforme a la arquitectura
            return await datos.CerrarCajaAsync(idUsuario, montoCierre);
        }

        // 4. CÁLCULO DE TOTALES (Dejamos el puente listo para el Corte de Caja)
        public decimal CalcularTotalesDelTurno(int idUsuario)
        {
            // Aquí en el futuro conectaremos la suma de las ventas que hizo 
            // el cajero durante su turno para compararlo con el dinero físico.
            return 0m;
        }

        public void ProcesarMovimiento(decimal monto, TipoMovimientoCaja tipo)
        {
            if (tipo == TipoMovimientoCaja.IngresoVenta || tipo == TipoMovimientoCaja.IngresoExtra)
            {
                // Sumar a la caja
            }
            else if (tipo == TipoMovimientoCaja.RetiroEfectivo || tipo == TipoMovimientoCaja.PagoProveedor)
            {
                // Restar a la caja
            }
        }
    }
}