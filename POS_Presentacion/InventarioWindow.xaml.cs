using POS_Entidades;
using POS_Logica;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks; // Indispensable para las peticiones asíncronas

namespace POS_Presentacion
{
    public partial class InventarioWindow : Window
    {
        private readonly ProductoLogica logica = new ProductoLogica();
        private bool modoEdicion = false;
        private string codigoOriginalEnEdicion = "";

        // =========================================================
        // VARIABLES DE CONTROL DE PAGINACIÓN
        // =========================================================
        private int paginaActual = 1;
        private int totalPaginas = 1;
        private readonly int limitePorPagina = 100;

        // OPTIMIZACIÓN: Caché de pinceles para cuidar la memoria RAM
        private readonly SolidColorBrush colorGuardar = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
        private readonly SolidColorBrush colorActualizar = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));

        public InventarioWindow()
        {
            InitializeComponent();
            _ = CargarInventarioVisual(); // Llamamos al nuevo motor de carga
        }

        // =========================================================
        // MOTOR PRINCIPAL ASÍNCRONO DE CARGA Y PAGINACIÓN
        // =========================================================
        private async Task CargarInventarioVisual()
        {
            try
            {
                // Protegemos la interfaz bloqueando botones mientras carga
                if (btnAnterior != null) btnAnterior.IsEnabled = false;
                if (btnSiguiente != null) btnSiguiente.IsEnabled = false;
                if (lblPaginacion != null) lblPaginacion.Text = "Cargando...";

                // Vamos a la capa Lógica a traer solo la página actual
                var resultado = await logica.CargarPaginaInventarioAsync(paginaActual, limitePorPagina);

                totalPaginas = resultado.TotalPaginas;
                dgInventario.ItemsSource = null;
                dgInventario.Items.Refresh();


                //Envolvemos la lista en una coleccion observable
                dgInventario.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<Producto>(resultado.Productos);
                dgInventario.Items.Refresh();

                // Restauramos la interfaz gráfica con los números reales
                if (lblPaginacion != null) lblPaginacion.Text = $"Página {paginaActual} de {totalPaginas}";
                if (btnAnterior != null) btnAnterior.IsEnabled = (paginaActual > 1);
                if (btnSiguiente != null) btnSiguiente.IsEnabled = (paginaActual < totalPaginas);

                // OPTIMIZACIÓN: Conteo de alertas en una sola pasada usando LINQ
                int alertas = resultado.Productos.Count(prod => prod.ControlaStock && prod.Stock <= prod.StockMinimo);

                // NOTA: Restringimos el aviso solo a la Página 1 para no interrumpir al cajero en cada clic de paginación
                if (alertas > 0 && paginaActual == 1)
                {
                    MessageBox.Show($"¡Atención! Tienes {alertas} producto(s) en esta página que han llegado al stock mínimo o están agotados.\n\nRevisa la tabla para reabastecer.", "Alerta de Inventario", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la tabla: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                await CargarInventarioVisual();
            }
        }

        private async void BtnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                await CargarInventarioVisual();
            }
        }

        // =========================================================
        // OPERACIONES CRUD ASÍNCRONAS
        // =========================================================
        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El Código de Barras y la Descripción son obligatorios.", "Datos Faltantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCodigo.Focus();
                return;
            }

            try
            {
                // BLINDAJE: Extracción segura de números
                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtVenta.Text, out decimal venta);
                decimal.TryParse(txtStock.Text, out decimal stock);
                decimal.TryParse(txtStockMinimo.Text, out decimal stockMinimo);
                decimal.TryParse(txtStockIdeal.Text, out decimal stockIdeal);

                Producto nuevoProducto = new Producto
                {
                    Codigo = txtCodigo.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Marca = txtMarca.Text.Trim(),
                    Proveedor = txtProveedor.Text.Trim(),
                    PrecioCosto = costo,
                    PrecioVenta = venta,
                    Stock = stock,
                    StockMinimo = stockMinimo,
                    StockIdeal = stockIdeal,
                    ControlaStock = chkControlaStock.IsChecked ?? true,
                    SeVendePorUnidad = chkPorPieza.IsChecked ?? true
                };

                // Uso de 'await' para no congelar la pantalla al guardar
                if (await logica.GuardarProductoAsync(nuevoProducto))
                {
                    MessageBox.Show(modoEdicion ? "¡Producto actualizado exitosamente!" : "¡Producto registrado exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    BtnLimpiar_Click(null, null);
                    await CargarInventarioVisual();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Producto productoSelec)
            {
                var resultado = MessageBox.Show($"¿Estás totalmente seguro de eliminar '{productoSelec.Nombre}' del inventario?\n\nEsta acción no se puede deshacer.", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Uso de 'await' para borrar asíncronamente
                        if (await logica.EliminarAsync(productoSelec.Codigo))
                        {
                            MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            dgInventario.SelectedItem = null;
                            await CargarInventarioVisual();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // =========================================================
        // BÚSQUEDA DINÁMICA
        // =========================================================
        private async void TxtBuscarInventario_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string texto = txtBuscarInventario.Text.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                {
                    // Si borran la búsqueda, regresamos a la paginación normal
                    await CargarInventarioVisual();
                    if (btnAnterior != null) btnAnterior.Visibility = Visibility.Visible;
                    if (btnSiguiente != null) btnSiguiente.Visibility = Visibility.Visible;
                    if (lblPaginacion != null) lblPaginacion.Visibility = Visibility.Visible;
                }
                else
                {
                    // Si escriben algo, buscamos en la BD asíncronamente y ocultamos los controles de página
                    dgInventario.ItemsSource = await logica.BuscarProductosPorNombreAsync(texto);

                    if (btnAnterior != null) btnAnterior.Visibility = Visibility.Hidden;
                    if (btnSiguiente != null) btnSiguiente.Visibility = Visibility.Hidden;
                    if (lblPaginacion != null) lblPaginacion.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception)
            {
                // Silenciamos los errores visuales rápidos mientras el usuario escribe
            }
        }

        // =========================================================
        // MANEJO DE INTERFAZ Y FORMULARIO (SIN CAMBIOS)
        // =========================================================
        private void DgInventario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgInventario.SelectedItem is Producto seleccionado)
            {
                LlenarFormulario(seleccionado);
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Producto productoSelec)
            {
                LlenarFormulario(productoSelec);

                modoEdicion = true;
                codigoOriginalEnEdicion = productoSelec.Codigo;

                btnGuardar.Content = "🔄 ACTUALIZAR PRODUCTO";
                btnGuardar.Background = colorActualizar;

                txtCodigo.Focus();
            }
        }

        private void LlenarFormulario(Producto producto)
        {
            txtCodigo.Text = producto.Codigo;
            txtCodigo.IsReadOnly = modoEdicion;
            txtNombre.Text = producto.Nombre;
            txtMarca.Text = producto.Marca;
            txtProveedor.Text = producto.Proveedor;
            txtCosto.Text = producto.PrecioCosto.ToString("0.##");
            txtVenta.Text = producto.PrecioVenta.ToString("0.##");
            txtStock.Text = producto.Stock.ToString("0.##");
            txtStockMinimo.Text = producto.StockMinimo.ToString("0.##");
            txtStockIdeal.Text = producto.StockIdeal.ToString("0.##");
            chkControlaStock.IsChecked = producto.ControlaStock;
            chkPorPieza.IsChecked = producto.SeVendePorUnidad;
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtCodigo.Clear();
            txtCodigo.IsReadOnly = false;
            txtNombre.Clear();
            txtMarca.Clear();
            txtProveedor.Clear();
            txtCosto.Clear();
            txtVenta.Clear();
            txtStock.Clear();
            txtStockMinimo.Clear();
            txtStockIdeal.Clear();

            chkControlaStock.IsChecked = true;
            chkPorPieza.IsChecked = true;

            modoEdicion = false;
            codigoOriginalEnEdicion = "";
            btnGuardar.Content = "💾 GUARDAR PRODUCTO";
            btnGuardar.Background = colorGuardar;

            txtCodigo.Focus();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }
    }
}