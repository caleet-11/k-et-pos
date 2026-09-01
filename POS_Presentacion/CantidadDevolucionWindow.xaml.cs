using System;
using System.Windows;
using System.Windows.Input;

namespace POS_Presentacion
{
    public partial class CantidadDevolucionWindow : Window
    {
        // Esta propiedad guardará el número final para que la otra ventana lo lea
        public decimal CantidadSeleccionada { get; private set; }

        // OPTIMIZACIÓN: Protegemos el valor para que no cambie tras la inicialización
        private readonly decimal maximoPermitido;

        public CantidadDevolucionWindow(string nombreProducto, decimal maximo)
        {
            InitializeComponent();
            maximoPermitido = maximo;
            lblInstruccion.Text = $"¿Cuántas unidades de '{nombreProducto}' deseas devolver? (Máximo: {maximo:0.###})";

            txtCantidad.Text = maximo.ToString("0.###"); // Ponemos el máximo por defecto
            txtCantidad.Focus();
            txtCantidad.SelectAll();
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            ConfirmarCantidad();
        }

        private void TxtCantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmarCantidad();
            }
        }

        // OPTIMIZACIÓN: Centralizamos la lógica de validación
        private void ConfirmarCantidad()
        {
            // Validamos que sea un número válido y que no sea negativo o cero
            if (decimal.TryParse(txtCantidad.Text.Trim(), out decimal cant) && cant > 0)
            {
                if (cant > maximoPermitido)
                {
                    MessageBox.Show($"No puedes devolver más unidades de las disponibles ({maximoPermitido:0.###}).", "Cantidad Excedida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CantidadSeleccionada = cant;
                this.DialogResult = true; // Esto cierra la ventana e indica éxito
            }
            else
            {
                MessageBox.Show("Por favor, ingresa una cantidad numérica válida y mayor a cero.", "Dato Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}