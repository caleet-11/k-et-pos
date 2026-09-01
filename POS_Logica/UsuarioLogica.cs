using POS_Datos;
using POS_Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS_Logica
{
    public class UsuarioLogica
    {
        private readonly UsuarioDatos datos = new UsuarioDatos();

        // ==============================================================
        // REGISTRO DE USUARIOS (CREACIÓN DEL HASH)
        // ==============================================================
        public async Task<bool> RegistrarUsuarioAsync(Usuario usuario, string passwordPlana)
        {
            if (string.IsNullOrWhiteSpace(passwordPlana) || passwordPlana.Length < 6)
            {
                throw new Exception("La contraseña debe tener al menos 6 caracteres.");
            }

            try
            {
                // Encriptamos la contraseña cruda con BCrypt
                string hashGenerado = BCrypt.Net.BCrypt.HashPassword(passwordPlana, workFactor: 11);

                // Guardamos el string loco del Hash directamente en tu propiedad 'Contrasena'
                usuario.Contrasena = hashGenerado;

                return await datos.InsertarUsuarioAsync(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la cuenta de usuario.", ex);
            }
        }

        // ==============================================================
        // LOGIN (VALIDACIÓN DEL HASH)
        // ==============================================================
        public async Task<Usuario> AutenticarLoginAsync(string nombreUsuario, string passwordPlana)
        {
            try
            {
                Usuario usuarioBd = await datos.ObtenerUsuarioPorUsernameAsync(nombreUsuario);

                if (usuarioBd == null)
                {
                    throw new Exception("Usuario o contraseña incorrectos.");
                }

                // BARRERA 1: ¿El usuario ya estaba bloqueado?
                if (usuarioBd.Bloqueado)
                {
                    throw new Exception("Esta cuenta ha sido bloqueada por múltiples intentos fallidos. Contacte al administrador del sistema.");
                }

                bool esValida = BCrypt.Net.BCrypt.Verify(passwordPlana, usuarioBd.Contrasena);

                if (!esValida)
                {
                    // BARRERA 2: Contraseña incorrecta. Sumamos un error a su historial.
                    usuarioBd.IntentosFallidos++;
                    bool bloquearCuenta = usuarioBd.IntentosFallidos >= 3;

                    // Registramos el fallo en MySQL
                    await datos.ActualizarIntentosFallidosAsync(usuarioBd.IdUsuario, usuarioBd.IntentosFallidos, bloquearCuenta);

                    if (bloquearCuenta)
                    {
                        throw new Exception("Ha superado el límite de 3 intentos. La cuenta ha sido bloqueada por seguridad.");
                    }
                    else
                    {
                        int intentosRestantes = 3 - usuarioBd.IntentosFallidos;
                        throw new Exception($"Usuario o contraseña incorrectos. Le quedan {intentosRestantes} intento(s).");
                    }
                }

                // BARRERA 3: ¡Éxito! Si entró correctamente y tenía errores previos, le "limpiamos" el historial a 0.
                if (usuarioBd.IntentosFallidos > 0)
                {
                    await datos.ActualizarIntentosFallidosAsync(usuarioBd.IdUsuario, 0, false);
                }

                return usuarioBd;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("incorrectos") || ex.Message.Contains("bloqueada") || ex.Message.Contains("límite")) throw;

                throw new Exception("Ocurrió un error interno durante la validación de credenciales.", ex);
            }
        }
        // ==============================================================
        // MÉTODOS CRUD RESTAURADOS (VERSIÓN ASÍNCRONA)
        // ==============================================================
        public async Task<List<Usuario>> ListarActivosAsync()
        {
            try
            {
                return await datos.ListarActivosAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar la lista de usuarios.", ex);
            }
        }

        public async Task<List<Rol>> ListarRolesAsync()
        {
            // Llamamos directamente al método que acabamos de corregir en la capa de Datos
            return await datos.ListarRolesAsync();
        }

        public async Task<bool> EliminarAsync(int idUsuario)
        {
            try
            {
                return await datos.EliminarUsuarioAsync(idUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar eliminar el usuario.", ex);
            }
        }
        public string ObtenerHashTemporal()
        {
            // Como estamos dentro de POS_Logica, aquí sí existe BCrypt y no marcará error
            return BCrypt.Net.BCrypt.HashPassword("admin123", 11);
        }
    }
}