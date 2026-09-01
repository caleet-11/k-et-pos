using POS_Entidades;
using POS_Logica;
using System;
using System.Windows;

namespace POS_Presentacion
{
    public partial class LoginWindow : Window
    {
        // OPTIMIZACIÓN: Blindaje de la capa lógica en memoria
        private readonly UsuarioLogica logica = new UsuarioLogica();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // BLINDAJE ASÍNCRONO: Apagamos el botón para evitar doble clics
            btnLogin.IsEnabled = false;

            try
            {
                // 1. Validamos las credenciales en la base de datos de forma asíncrona
                Usuario usuarioValidado = await logica.AutenticarLoginAsync(txtUsuario.Text.Trim(), txtPassword.Password.Trim());

                // Asignamos la sesión de manera global
                SesionGlobal.UsuarioActual = usuarioValidado;

                // =================================================================
                // 2. CANDADO DE SEGURIDAD: VERIFICACIÓN DE FONDO DE CAJA
                // =================================================================

                if (usuarioValidado.IdRol != 1)
                {
                    CajaLogica cajaLogica = new CajaLogica();
                    DateTime? aperturaTurno = await cajaLogica.ObtenerUltimaAperturaAsync(usuarioValidado.IdUsuario);

                    if (!aperturaTurno.HasValue)
                    {
                        MessageBox.Show($"Hola {usuarioValidado.Nombre}.\n\nAntes de entrar al sistema, por favor cuenta y confirma el dinero físico que tienes como fondo de caja inicial.",
                                        "Confirmar Fondo de Caja", MessageBoxButton.OK, MessageBoxImage.Information);

                        CajaWindow ventanaCaja = new CajaWindow();
                        ventanaCaja.ShowDialog();

                        aperturaTurno = await cajaLogica.ObtenerUltimaAperturaAsync(usuarioValidado.IdUsuario);

                        if (!aperturaTurno.HasValue)
                        {
                            MessageBox.Show("Acceso cancelado. Es obligatorio aceptar y registrar el fondo de caja para poder operar el sistema.",
                                            "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);

                            SesionGlobal.UsuarioActual = null;
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Continuando tu turno activo.\nTurno abierto hoy a las: {aperturaTurno.Value.ToString("HH:mm")}",
                                        "Turno en Curso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                // 3. Acceso concedido: Abrimos el Punto de Venta
                VentasWindow principal = new VentasWindow();
                principal.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                string errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show(errorReal, "Acceso Denegado (Modo Debug)", MessageBoxButton.OK, MessageBoxImage.Error);

                txtPassword.Clear();
                txtPassword.Focus();
            }
            finally
            {
                // Si hay un error y no entramos, volvemos a encender el botón
                btnLogin.IsEnabled = true;
            }
        }
    }
}