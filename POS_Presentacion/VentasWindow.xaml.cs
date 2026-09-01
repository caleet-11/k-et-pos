using POS_Entidades;
using POS_Logica;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;

namespace POS_Presentacion
{
    public partial class VentasWindow : Window
    {
        // OPTIMIZACIÓN: Protección readonly
        private readonly ProductoLogica productoLogica = new ProductoLogica();

        // Diccionario que guardará nuestros 3 carritos independientes
        private readonly Dictionary<string, ObservableCollection<ItemTicket>> carritosDeVenta;

        // Variable para saber en qué pestaña estamos parados
        private string carritoActual;

        public VentasWindow()
        {
            InitializeComponent();

            // 1. Inicializamos los contenedores en memoria primero
            carritosDeVenta = new Dictionary<string, ObservableCollection<ItemTicket>>
            {
                { "Venta 1", new ObservableCollection<ItemTicket>() },
                { "Venta 2", new ObservableCollection<ItemTicket>() },
                { "Venta 3", new ObservableCollection<ItemTicket>() }
            };

            // 2. Establecemos el carrito por defecto y conectamos la tabla
            carritoActual = "Venta 1";
            dgTicket.ItemsSource = carritosDeVenta[carritoActual];

            txtCodigoBarras.Focus();
        }

        private void TxtCodigoBarras_KeyDown(object sender, KeyEventArgs e)
        {
            // Atrapamos el 'Enter' automático del lector de códigos
            if (e.Key == Key.Enter)
            {
                string codigoEscaneado = txtCodigoBarras.Text.Trim();

                if (!string.IsNullOrWhiteSpace(codigoEscaneado))
                {
                    ProcesarEscaneo(codigoEscaneado);

                    // Limpiamos la casilla instantáneamente para el siguiente artículo
                    txtCodigoBarras.Clear();
                }
            }
        }

        private async void ProcesarEscaneo(string codigo)
        {
            // 1. Vamos a buscar el producto a la base de datos MySQL
            Producto producto = await productoLogica.BuscarProductoAsync(codigo);

            if (producto != null)
            {
                var carritoActivo = carritosDeVenta[carritoActual];
                var itemExistente = carritoActivo.FirstOrDefault(x => x.Codigo == producto.Codigo);

                decimal cantidadAAgregar = 1;

                // Si el producto NO se vende por unidad, activamos la báscula
                if (!producto.SeVendePorUnidad)
                {
                    BasculaLogica bascula = new BasculaLogica();
                    try
                    {
                        cantidadAAgregar = bascula.ObtenerPeso();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Fallo en Báscula", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // Cancelamos agregar el producto si la báscula falló
                    }
                }

                if (itemExistente != null)
                {
                    // Sumamos las piezas O sumamos los kilos leídos
                    itemExistente.Cantidad += cantidadAAgregar;
                    dgTicket.Items.Refresh();
                }
                else
                {
                    // Agregamos el producto nuevo al carrito activo
                    carritoActivo.Add(new ItemTicket
                    {
                        Codigo = producto.Codigo,
                        Nombre = producto.Nombre,
                        PrecioVenta = producto.PrecioVenta,
                        PrecioCosto = producto.PrecioCosto,
                        Cantidad = cantidadAAgregar
                    });
                }

                ActualizarTotal();
            }
            else
            {
                MessageBox.Show("El producto no existe en la base de datos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ActualizarTotal()
        {
            // Seguro por si la interfaz gráfica no ha terminado de cargar
            if (carritosDeVenta == null || string.IsNullOrEmpty(carritoActual) || lblTotal == null) return;

            // Uso de LINQ para sumar eficientemente
            decimal total = carritosDeVenta[carritoActual].Sum(item => item.Subtotal);

            lblTotal.Text = total.ToString("C");
        }

        private async void BtnCobrar_Click(object sender, RoutedEventArgs e)
        {
            // Verificamos si el carrito actual está vacío
            if (carritosDeVenta[carritoActual].Count == 0) return;

            // OPTIMIZACIÓN: Extracción de dinero basada en la cultura local
            if (!decimal.TryParse(lblTotal.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal totalExactoDeVenta) || totalExactoDeVenta <= 0)
            {
                MessageBox.Show($"Ocurrió un problema al procesar el total: '{lblTotal.Text}'", "Error Matemático", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CobroWindow ventanaCobro = new CobroWindow(totalExactoDeVenta);
            ventanaCobro.Owner = this;

            // Si el cobro visual (dar el cambio o fiar) es exitoso, procedemos a guardar en MySQL
            if (ventanaCobro.ShowDialog() == true)
            {
                try
                {
                    // ========================================================================
                    // 1. ARMAMOS EL OBJETO DE LA VENTA (¡AQUÍ ESTÁ LA CONEXIÓN CLAVE!)
                    // ========================================================================
                    Venta nuevaVenta = new Venta
                    {
                        Total = totalExactoDeVenta,
                        IdUsuario = 1, // ID del cajero temporal

                        // Extraemos las decisiones que el cajero tomó en la ventana de cobro:
                        TipoOperacion = ventanaCobro.MetodoSeleccionado,
                        IdCliente = ventanaCobro.IdClienteSeleccionado,

                        Detalles = new List<VentaDetalle>()
                    };

                    // 2. TRASLADAMOS LOS PRODUCTOS DEL CARRITO VISUAL A LA VENTA OFICIAL
                    foreach (var item in carritosDeVenta[carritoActual])
                    {
                        nuevaVenta.Detalles.Add(new VentaDetalle
                        {
                            Codigo = item.Codigo,
                            Nombre = item.Nombre,
                            Cantidad = item.Cantidad,
                            PrecioCosto = item.PrecioCosto,
                            PrecioVenta = item.PrecioVenta
                        });
                    }

                    // 3. ENVIAMOS LA ORDEN A LA CAPA LÓGICA
                    POS_Logica.VentaLogica logicaVentas = new POS_Logica.VentaLogica();

                    var resultado = await logicaVentas.ProcesarVentaAsync(nuevaVenta);

                    if (resultado.Sucedio)
                    {
                        // 4. ¡ÉXITO! AHORA SÍ PODEMOS BORRAR EL CARRITO
                        carritosDeVenta[carritoActual].Clear();
                        ActualizarTotal();
                        txtCodigoBarras.Focus();

                        MessageBox.Show(resultado.Mensaje, "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultado.Mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ocurrió un error al guardar en la base de datos:\n\n" + ex.Message, "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F2)
            {
                BtnF2_Click(null, null);
            }
            else if (e.Key == Key.F3)
            {
                BtnF3_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F4)
            {
                BtnF4_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F6)
            {
                if (SesionGlobal.UsuarioActual.IdRol != 1) { MostrarAccesoDenegado(); return; }
                ProveedorWindow ventanaProveedor = new ProveedorWindow();
                ventanaProveedor.Owner = this;
                ventanaProveedor.ShowDialog();
                e.Handled = true;
            }
            else if (e.Key == Key.F7)
            {
                if (SesionGlobal.UsuarioActual.IdRol != 1) { MostrarAccesoDenegado(); return; }
                UsuarioWindow ventanaUsuario = new UsuarioWindow();
                ventanaUsuario.Owner = this;
                ventanaUsuario.ShowDialog();
                e.Handled = true;
            }
            else if (e.Key == Key.F8)
            {
                BtnF8_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F9)
            {
                BtnF9_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F10 || (e.Key == Key.System && e.SystemKey == Key.F10))
            {
                BtnF10_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                BtnF11_Click(null, null);
                e.Handled = true;
            }
            // Si presionan Enter (y NO están en la caja de texto del código de barras)
            else if (e.Key == Key.Enter && !txtCodigoBarras.IsFocused)
            {
                BtnCobrar_Click(null, null);
                e.Handled = true;
            }
        }
        private void BtnF2_Click(object sender, RoutedEventArgs e)
        {
            ClientesWindow ventanaClientes = new ClientesWindow();
            ventanaClientes.ShowDialog();
        }

        private void BtnF3_Click(object sender, RoutedEventArgs e)
        {
            BuscadorWindow buscador = new BuscadorWindow();
            buscador.Owner = this;

            if (buscador.ShowDialog() == true)
            {
                ProcesarEscaneo(buscador.CodigoSeleccionado);
                txtCodigoBarras.Clear();
            }
        }

        private void BtnF4_Click(object sender, RoutedEventArgs e)
        {
            ProductoRapidoWindow ventanaRapida = new ProductoRapidoWindow();
            ventanaRapida.Owner = this;

            if (ventanaRapida.ShowDialog() == true)
            {
                var carritoActivo = carritosDeVenta[carritoActual];

                carritoActivo.Add(new ItemTicket
                {
                    Codigo = "999999",
                    Nombre = $"[RÁPIDO] {ventanaRapida.NombreProducto}",
                    PrecioVenta = ventanaRapida.PrecioProducto,
                    PrecioCosto = ventanaRapida.PrecioProducto,
                    Cantidad = 1
                });

                ActualizarTotal();
                txtCodigoBarras.Focus();
            }
        }

        private void BtnF8_Click(object sender, RoutedEventArgs e)
        {
            if (SesionGlobal.UsuarioActual.IdRol != 1)
            {
                MostrarAccesoDenegado();
                return;
            }

            InventarioWindow ventanaInv = new InventarioWindow();
            ventanaInv.Owner = this;
            ventanaInv.ShowDialog();
        }

        private void BtnF9_Click(object sender, RoutedEventArgs e)
        {
            if (carritosDeVenta[carritoActual].Count > 0)
            {
                var confirmacion = MessageBox.Show($"¿Estás seguro de cancelar toda la {carritoActual}?",
                                                   "Cancelar Venta", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmacion == MessageBoxResult.Yes)
                {
                    carritosDeVenta[carritoActual].Clear();
                    ActualizarTotal();
                    MessageBox.Show($"{carritoActual} cancelada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCodigoBarras.Focus();
                }
            }
        }

        private void BtnF10_Click(object sender, RoutedEventArgs e)
        {
            HistorialWindow ventanaHistorial = new HistorialWindow();
            ventanaHistorial.Owner = this;
            ventanaHistorial.ShowDialog();
        }

        private void BtnF11_Click(object sender, RoutedEventArgs e)
        {
            CajaWindow ventanaCaja = new CajaWindow();
            ventanaCaja.Owner = this;
            ventanaCaja.ShowDialog();
        }
        private void BtnF12_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow ventanaInfo = new InfoWindow();
            ventanaInfo.ShowDialog(); // ShowDialog congela la pantalla de atrás hasta que cierren la info
        }

        private void MostrarAccesoDenegado()
        {
            MessageBox.Show("No tienes permisos suficientes para acceder a este módulo. Solicita acceso a un Administrador.",
                            "Acceso Restringido", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void TabVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Evitamos que pestañas anidadas disparen el evento
            if (e.OriginalSource != tabVentas) return;

            if (tabVentas == null || carritosDeVenta == null || !this.IsLoaded) return;

            if (tabVentas.SelectedIndex == 0) carritoActual = "Venta 1";
            else if (tabVentas.SelectedIndex == 1) carritoActual = "Venta 2";
            else if (tabVentas.SelectedIndex == 2) carritoActual = "Venta 3";

            dgTicket.ItemsSource = carritosDeVenta[carritoActual];
            ActualizarTotal();
            txtCodigoBarras.Focus();
        }

        private void BtnAumentar_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is ItemTicket producto)
            {
                producto.Cantidad += 1;

                // La tabla se actualiza sola gracias a INotifyPropertyChanged
                ActualizarTotal();
                txtCodigoBarras.Focus();
            }
        }

        private void BtnDisminuir_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is ItemTicket producto)
            {
                if (producto.Cantidad > 1)
                {
                    producto.Cantidad -= 1;

                    // La tabla se actualiza sola gracias a INotifyPropertyChanged
                    ActualizarTotal();
                }
                else
                {
                    BtnEliminar_Click(sender, e);
                }

                txtCodigoBarras.Focus();
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is ItemTicket producto)
            {
                carritosDeVenta[carritoActual].Remove(producto);
                ActualizarTotal();
                txtCodigoBarras.Focus();
            }
        }
    }
}