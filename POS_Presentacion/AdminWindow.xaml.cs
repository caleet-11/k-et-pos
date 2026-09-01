using POS_Logica;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace POS_Presentacion
{
    public partial class AdminWindow : Window
    {
        private readonly ReporteLogica reporteLogica = new ReporteLogica();

        public AdminWindow()
        {
            InitializeComponent();
            // Ya no cargamos los datos aquí porque los constructores no soportan 'await' de forma natural
        }

        // ====================================================================
        // NUEVO: Evento que se dispara automáticamente cuando la ventana "nace"
        // ====================================================================
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Asignamos la fecha de hoy visualmente en los calendarios
            dpFechaInicio.SelectedDate = DateTime.Today;
            dpFechaFin.SelectedDate = DateTime.Today;

            // 2. Cargamos el reporte de hoy automáticamente para proteger la memoria
            await CargarReporteAsync(DateTime.Today, DateTime.Today);
        }

        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            DateTime inicio = dpFechaInicio.SelectedDate ?? DateTime.Today;
            DateTime fin = dpFechaFin.SelectedDate ?? DateTime.Today;

            // Reciclamos el método para no repetir código
            await CargarReporteAsync(inicio, fin);
        }

        // ====================================================================
        // OPTIMIZACIÓN: Método centralizado para procesar la búsqueda
        // ====================================================================
        private async Task CargarReporteAsync(DateTime inicio, DateTime fin)
        {
            try
            {
                // Bloqueamos el botón visualmente para evitar clics dobles ansiosos
                btnGenerar.IsEnabled = false;

                // Ejecutamos la consulta a través de nuestra arquitectura
                var reporte = await reporteLogica.GenerarReportePorFechasAsync(inicio, fin);

                dgReporte.ItemsSource = reporte;

                // Procesamos los totales con LINQ
                decimal totalIngreso = reporte.Sum(x => x.IngresoBruto);
                decimal totalGanancia = reporte.Sum(x => x.GananciaNeta);

                lblIngresoBruto.Text = totalIngreso.ToString("C");
                lblGananciaNeta.Text = totalGanancia.ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al generar reporte", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                // Liberamos el botón pase lo que pase
                btnGenerar.IsEnabled = true;
            }
        }
    }
}