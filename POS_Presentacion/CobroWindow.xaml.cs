using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace POS_Presentacion
{
    public partial class CobroWindow : Window
    {
        // OPTIMIZACIÓN: Variables protegidas y pinceles precargados en memoria.
        private readonly decimal totalOriginalExacto;
        private readonly SolidColorBrush colorExito = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
        private readonly SolidColorBrush colorError = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D32F2F"));

        public int? IdClienteSeleccionado { get; private set; } = null;
        public string MetodoSeleccionado { get; private set; } = "Efectivo";

        public CobroWindow(decimal totalDeLaVenta)
        {
            InitializeComponent();
            totalOriginalExacto = totalDeLaVenta;

            // Al poner esto en true, WPF manda a llamar a ActualizarMetodoPago() automáticamente
            rbEfectivo.IsChecked = true;
        }

        private void MetodoPago_Checked(object sender, RoutedEventArgs e)
        {
            ActualizarMetodoPago();
        }

        private void ActualizarMetodoPago()
        {
            if (!this.IsInitialized) return;

            if (rbTarjeta.IsChecked == true)
            {
                MetodoSeleccionado = "Tarjeta";
                lblTotalCobrar.Text = totalOriginalExacto.ToString("C");
                txtEfectivo.Text = totalOriginalExacto.ToString("0.00");
                txtEfectivo.IsReadOnly = true;
                lblCambio.Text = "$0.00";

                btnConfirmar.Background = colorExito;
                btnConfirmar.Content = "✅ CONFIRMAR (TARJETA)";
                btnConfirmar.IsHitTestVisible = true;
                btnConfirmar.Opacity = 1.0;
            }
            else if (rbAutoconsumo.IsChecked == true)
            {
                MetodoSeleccionado = "Autoconsumo";
                // En autoconsumo, el total a pagar se refleja pero no entra dinero a caja
                lblTotalCobrar.Text = totalOriginalExacto.ToString("C");
                txtEfectivo.Text = "0.00";
                txtEfectivo.IsReadOnly = true;
                lblCambio.Text = "$0.00";

                btnConfirmar.Background = colorExito;
                btnConfirmar.Content = "✅ REGISTRAR MERMA/CONSUMO";
                btnConfirmar.IsHitTestVisible = true;
                btnConfirmar.Opacity = 1.0;
            }
            else if (rbFiado.IsChecked == true)
            {
                MetodoSeleccionado = "Fiado";
                lblTotalCobrar.Text = totalOriginalExacto.ToString("C");
                txtEfectivo.Text = "0.00";
                txtEfectivo.IsReadOnly = true;
                lblCambio.Text = "$0.00";

                // DEJAMOS LA BASE LISTA: Por ahora solo confirma, después abriremos aquí el selector de clientes
                btnConfirmar.Background = colorExito;
                btnConfirmar.Content = "✅ AUTORIZAR FIADO";
                btnConfirmar.IsHitTestVisible = true;
                btnConfirmar.Opacity = 1.0;
            }
            else if (rbEfectivo.IsChecked == true)
            {
                MetodoSeleccionado = "Efectivo";
                decimal totalRedondeado = Math.Round(totalOriginalExacto, 0, MidpointRounding.AwayFromZero);
                lblTotalCobrar.Text = totalRedondeado.ToString("C");

                txtEfectivo.IsReadOnly = false;
                txtEfectivo.Text = "";
                txtEfectivo.Focus();

                TxtEfectivo_TextChanged(null, null);
            }
        }

        private void TxtEfectivo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.IsInitialized || rbTarjeta.IsChecked == true) return;

            // 1. Calculamos el total exacto en memoria, sin depender del texto visual de la pantalla
            decimal totalAPagar = Math.Round(totalOriginalExacto, 0, MidpointRounding.AwayFromZero);

            // 2. Convertimos el efectivo recibido a número
            decimal efectivoRecibido = 0;
            decimal.TryParse(txtEfectivo.Text, out efectivoRecibido);

            // 3. Calculamos el cambio
            decimal cambio = efectivoRecibido - totalAPagar;

            // 4. Validamos los colores y bloqueos
            if (efectivoRecibido < totalAPagar)
            {
                // NO ALCANZA
                lblCambio.Text = "$0.00";
                btnConfirmar.Background = colorError;
                btnConfirmar.Content = "❌ EFECTIVO INSUFICIENTE";
                btnConfirmar.IsHitTestVisible = false;
                btnConfirmar.Opacity = 0.8;
            }
            else
            {
                // SÍ ALCANZA
                lblCambio.Text = cambio.ToString("C");
                btnConfirmar.Background = colorExito;
                btnConfirmar.Content = "✅ CONFIRMAR PAGO (ENTER)";
                btnConfirmar.IsHitTestVisible = true;
                btnConfirmar.Opacity = 1.0;
            }
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (MetodoSeleccionado == "Fiado")
            {
                // Si es fiado, abrimos el buscador
                ClientesBuscarWindow ventanaClientes = new ClientesBuscarWindow();
                ventanaClientes.Owner = this; // Bloquea la ventana de atrás

                if (ventanaClientes.ShowDialog() == true)
                {
                    // Si el cajero seleccionó un cliente y le dio aceptar
                    IdClienteSeleccionado = ventanaClientes.ClienteSeleccionado.IdCliente;
                    this.DialogResult = true;
                    this.Close();
                }
                // Si el cajero cerró el buscador con la X, simplemente no hace nada y regresa al cobro
            }
            else
            {
                // Si es Efectivo, Tarjeta o Autoconsumo, cerramos normal
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}