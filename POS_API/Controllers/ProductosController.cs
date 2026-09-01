using Microsoft.AspNetCore.Mvc;
using POS_Entidades;
using POS_Logica;
using System;
using System.Threading.Tasks;

namespace POS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoLogica logica = new ProductoLogica();

        // RUTA EXPLÍCITA: Ahora escucha perfectamente en /api/productos/registrar
        [HttpPost("registrar")]
        public async Task<IActionResult> Post([FromBody] Producto producto)
        {
            try
            {
                // Enviamos el objeto a la capa lógica
                bool guardadoExitoso = await logica.RegistrarProductoAsync(producto);

                if (guardadoExitoso)
                {
                    return Ok(new { Sucedio = true, Mensaje = "Producto guardado correctamente en el inventario." });
                }
                else
                {
                    return BadRequest(new { Sucedio = false, Mensaje = "Error al intentar guardar el producto." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Sucedio = false, Mensaje = ex.Message });
            }
        }
    }
}