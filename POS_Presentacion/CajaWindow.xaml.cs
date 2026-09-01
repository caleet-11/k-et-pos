using POS_Entidades;
using POS_Logica;
using System;
using System.Windows;

namespace POS_Presentacion
{
    public partial class CajaWindow : Window
    {
        private readonly CajaLogica logica = new CajaLogica();

        // CORRECCIÓN: Ahora solo necesitamos saber si hay una fecha de apertura para saber si el turno está activo
        private DateTime? fechaAperturaTurno;
        private decimal totalEsperadoCalculado = 0;

        public CajaWindow()
        {
            InitializeComponent();
            VerificarEstatusCaja();
        }

        private async void VerificarEstatusCaja()
        {
            // CORRECCIÓN: Usamos el nuevo método que creamos
            fechaAperturaTurno = await logica.ObtenerUltimaAperturaAsync(SesionGlobal.UsuarioActual.IdUsuario);

            // Si es nulo, significa que el turno está cerrado
            if (!fechaAperturaTurno.HasValue)
            {
                panelApertura.Visibility = Visibility.Visible;
                panelCierre.Visibility = Visibility.Collapsed;
                txtMontoApertura.Focus();
                txtMontoApertura.SelectAll();
            }
            else
            {
                panelApertura.Visibility = Visibility.Collapsed;
                panelCierre.Visibility = Visibility.Visible;

                // CORRECCIÓN: Llamamos al nuevo puente de cálculo
                totalEsperadoCalculado = logica.CalcularTotalesDelTurno(SesionGlobal.UsuarioActual.IdUsuario);

                // Como ya no usamos CajaFlujo, ponemos textos temporales en lo que programamos la suma de ventas
                lblFondoInicial.Text = "Turno Abierto";
                lblVentasEfectivo.Text = "Cálculo pendiente...";
                lblTotalEsperado.Text = totalEsperadoCalculado.ToString("C");

                txtMontoReal.Focus();
            }
        }

        private async void BtnAbrir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal montoInicial = Convert.ToDecimal(txtMontoApertura.Text);

                // CORRECCIÓN: Conectado a AbrirCaja
                if (await logica.AbrirCajaAsync(SesionGlobal.UsuarioActual.IdUsuario, montoInicial))
                {
                    MessageBox.Show("Turno de caja abierto correctamente en la bitácora.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al abrir", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMontoReal.Text))
                    throw new Exception("Debes ingresar el monto real contado físicamente.");

                decimal montoReal = Convert.ToDecimal(txtMontoReal.Text);
                decimal diferencia = montoReal - totalEsperadoCalculado;

                string textoDiferencia = diferencia == 0 ? "Perfecto" : (diferencia > 0 ? $"Sobrante de {diferencia:C}" : $"Faltante de {Math.Abs(diferencia):C}");
                string motivoReporte = $"Cierre de turno. Esperado: {totalEsperadoCalculado:C}, Real contado: {montoReal:C}. Estatus: {textoDiferencia}.";

                // CORRECCIÓN: Conectado a CerrarCaja (ahora solo pide ID y Monto de cierre)
                if (await logica.CerrarCajaAsync(SesionGlobal.UsuarioActual.IdUsuario, montoReal))
                {
                    MessageBox.Show($"{motivoReporte}\n\nEl sistema se cerrará para el siguiente turno.", "Corte de Caja Procesado", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al cerrar", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}