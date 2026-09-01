using POS_Entidades;
using POS_Logica;
using System;
using System.Windows;

namespace POS_Presentacion
{
    public partial class UsuarioWindow : Window
    {
        private readonly UsuarioLogica logica = new UsuarioLogica();

        public UsuarioWindow()
        {
            InitializeComponent();
            CargarRoles();
            CargarTablaUsuarios();
            txtNombre.Focus();
        }

        private async void CargarRoles()
        {
            cmbRoles.ItemsSource = (System.Collections.IEnumerable)await logica.ListarRolesAsync();
        }

        private async void CargarTablaUsuarios()
        {
            dgUsuarios.ItemsSource = await logica.ListarActivosAsync();
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // OPTIMIZACIÓN: Validación temprana (Fail-Fast)
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Por favor, llena todos los campos de texto para registrar al usuario.", "Faltan Datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbRoles.SelectedValue == null)
            {
                MessageBox.Show("Por favor, selecciona un Rol (Administrador o Cajero) para el usuario.", "Faltan Datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbRoles.Focus();
                return;
            }

            try
            {
                Usuario nuevo = new Usuario
                {
                    Nombre = txtNombre.Text.Trim(),
                    NombreUsuario = txtUsername.Text.Trim(),
                    Contrasena = txtPassword.Password.Trim(),
                    IdRol = Convert.ToInt32(cmbRoles.SelectedValue)
                };

                // Pasamos ambas contraseñas a la lógica para que las compare
                if (await logica.RegistrarUsuarioAsync(nuevo, txtConfirmar.Password.Trim()))
                {
                    MessageBox.Show("Usuario registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFormulario();
                    CargarTablaUsuarios(); // Refrescamos la tabla visual
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Limpiamos las contraseñas si hubo un error (ej. no coincidían)
                txtPassword.Clear();
                txtConfirmar.Clear();
                txtPassword.Focus();
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is Usuario seleccionado)
            {
                // Protegemos a tu usuario maestro para que no lo borres por accidente
                if (seleccionado.NombreUsuario.ToLower() == "admin")
                {
                    MessageBox.Show("No puedes eliminar al Administrador principal del sistema.", "Acción Bloqueada", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // OPTIMIZACIÓN: Candado para evitar que el usuario se borre a sí mismo
                if (SesionGlobal.UsuarioActual != null && seleccionado.IdUsuario == SesionGlobal.UsuarioActual.IdUsuario)
                {
                    MessageBox.Show("No puedes eliminar tu propia cuenta mientras tienes una sesión activa.", "Acción Bloqueada", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var confirmacion = MessageBox.Show($"¿Estás seguro de que deseas dar de baja a {seleccionado.Nombre}?",
                                                   "Confirmar Baja", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirmacion == MessageBoxResult.Yes)
                {
                    if (await logica.EliminarAsync(seleccionado.IdUsuario))
                    {
                        MessageBox.Show("Usuario dado de baja exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarTablaUsuarios();
                    }
                }
            }
            else
            {
                MessageBox.Show("Primero debes seleccionar un usuario de la tabla.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmar.Clear();
            cmbRoles.SelectedItem = null;
            txtNombre.Focus();
        }
    }
}