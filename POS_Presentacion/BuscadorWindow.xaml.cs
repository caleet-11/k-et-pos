using POS_Entidades;
using POS_Logica;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS_Presentacion
{
    public partial class BuscadorWindow : Window
    {
        // OPTIMIZACIÓN: Protegemos la capa de lógica con readonly
        private readonly ProductoLogica logica = new ProductoLogica();

        // Esta variable pública es la que leerá tu ventana principal
        public string CodigoSeleccionado { get; private set; } = null;

        public BuscadorWindow()
        {
            InitializeComponent();
            txtBusqueda.Focus(); // Para que el cursor aparezca listo para escribir
        }

        // Se ejecuta cada vez que tecleas una letra nueva
        private async void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBusqueda.Text.Length > 1) // Buscar a partir de 2 letras
            {
                dgResultados.ItemsSource = await logica.BuscarProductosPorNombreAsync(txtBusqueda.Text);
            }
            else
            {
                dgResultados.ItemsSource = null;
            }
        }

        // Si estás escribiendo y presionas la flecha ABAJO, forzamos a WPF a seleccionar la primera fila
        private void TxtBusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && dgResultados.Items.Count > 0)
            {
                dgResultados.Focus();
                dgResultados.SelectedIndex = 0;

                // Obligamos a WPF a poner el cursor azul sobre la primera fila
                var row = (DataGridRow)dgResultados.ItemContainerGenerator.ContainerFromIndex(0);
                if (row != null) { row.Focus(); }

                e.Handled = true; // Evitamos que el texto haga cosas raras
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        // PREVIEWKeyDown intercepta el Enter ANTES de que el DataGrid lo devore
        private void DgResultados_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // Detenemos el salto de línea del DataGrid
                SeleccionarYCerrar();
            }
        }

        // Nuevo: Si dan doble clic rápido con el ratón, también lo agregamos
        private void DgResultados_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SeleccionarYCerrar();
        }

        // Metodo reutilizable para no repetir código
        private void SeleccionarYCerrar()
        {
            if (dgResultados.SelectedItem is Producto seleccionado)
            {
                CodigoSeleccionado = seleccionado.Codigo;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}