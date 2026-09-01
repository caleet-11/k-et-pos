using System.Windows;

namespace POS_Presentacion
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Cierra esta ventana flotante y regresa al Punto de Venta
            this.Close();
        }
    }
}