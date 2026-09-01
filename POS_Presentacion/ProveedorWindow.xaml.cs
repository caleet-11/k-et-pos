using POS_Entidades;
using POS_Logica;
using System;
using System.Windows;

namespace POS_Presentacion
{
    public partial class ProveedorWindow : Window
    {
        // OPTIMIZACIÓN: Protegemos la instancia de la lógica en memoria
        private readonly ProveedorLogica logica = new ProveedorLogica();

        public ProveedorWindow()
        {
            InitializeComponent();
            txtEmpresa.Focus(); // El cursor espera en la primera caja de texto
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // OPTIMIZACIÓN: Validación temprana para evitar viajes innecesarios a la base de datos
            if (string.IsNullOrWhiteSpace(txtEmpresa.Text))
            {
                MessageBox.Show("El nombre de la empresa o marca es obligatorio para registrar al proveedor.", "Faltan Datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmpresa.Focus();
                return;
            }

            try
            {
                Proveedor nuevoProveedor = new Proveedor
                {
                    Empresa = txtEmpresa.Text.Trim(),
                    NombreContacto = txtContacto.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Activo = true
                };

                // Llamamos a la capa lógica para procesar la 
                if (await logica.GuardarProveedor(nuevoProveedor))
                {
                    MessageBox.Show("Proveedor registrado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // OPTIMIZACIÓN: Avisamos a la ventana padre (si existe) que el guardado fue exitoso
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                // Si la capa lógica lanza un error (ej. código duplicado, error de conexión), lo atrapamos aquí
                MessageBox.Show(ex.Message, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}