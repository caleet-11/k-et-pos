using POS_Entidades;
using POS_Logica;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static POS_Presentacion.VentasWindow;

namespace POS_Presentacion
{
    public partial class HistorialWindow : Window
    {
        // OPTIMIZACIÓN: 'readonly' protege la instancia y mejora el manejo del Recolector de Basura
        private readonly HistorialLogica logica = new HistorialLogica();
        private Venta ticketSeleccionado;

        public HistorialWindow()
        {
            InitializeComponent();
            CargarTickets();
        }

        // 1. ASÍNCRONO: La carga inicial de la ventana
        private async void CargarTickets()
        {
            // La pantalla se dibuja al instante, y los datos aparecen en cuanto MySQL responde
            dgTickets.ItemsSource = await logica.ListarTicketsAsync();
        }

        // 2. ASÍNCRONO: Cada vez que tocas un folio, esta función consulta sus artículos
        private async void DgTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTickets.SelectedItem is Venta venta)
            {
                ticketSeleccionado = venta;
                // Llenamos la tablita de la derecha sin congelar la ventana
                dgDetalles.ItemsSource = await logica.VerDetallesAsync(venta.IdVenta);
            }
            else
            {
                ticketSeleccionado = null;
                dgDetalles.ItemsSource = null;
            }
        }

        private void BtnReimprimir_Click(object sender, RoutedEventArgs e)
        {
            if (ticketSeleccionado != null && dgDetalles.Items.Count > 0)
            {
                // OPTIMIZACIÓN: Uso de 'as IEnumerable' para evitar una excepción de casteo (InvalidCastException) 
                var listaVisual = dgDetalles.ItemsSource as IEnumerable<VentaDetalle>;

                if (listaVisual == null) return;

                // Preparamos la lista oficial que la Venta está esperando
                ticketSeleccionado.Detalles = new List<VentaDetalle>();

                // Pasamos los datos de un molde a otro
                foreach (var item in listaVisual)
                {
                    ticketSeleccionado.Detalles.Add(new VentaDetalle
                    {
                        Codigo = item.Codigo,
                        Cantidad = item.Cantidad,
                        PrecioVenta = item.PrecioVenta,
                        Nombre = item.Nombre // <-- Agregamos el nombre para que la impresora sepa qué imprimir
                    });
                }

                // Mandamos a imprimir
                ImpresoraLogica impresora = new ImpresoraLogica();
                impresora.ImprimirTicket(ticketSeleccionado);
            }
            else
            {
                MessageBox.Show("Por favor selecciona un ticket de la lista de la izquierda.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 3. ASÍNCRONO: Al devolver un artículo
        private async void BtnDevolver_Click(object sender, RoutedEventArgs e)
        {
            if (dgDetalles.SelectedItem is VentaDetalle itemSeleccionado && ticketSeleccionado != null)
            {
                // Calculamos cuántas unidades quedan disponibles originalmente
                decimal disponiblesParaDevolver = itemSeleccionado.Cantidad - itemSeleccionado.CantidadDevuelta;

                if (disponiblesParaDevolver <= 0)
                {
                    MessageBox.Show("Este artículo ya fue devuelto en su totalidad en este ticket.", "Acción Bloqueada", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Abrimos la ventana flotante para preguntar la cantidad
                CantidadDevolucionWindow ventanaCantidad = new CantidadDevolucionWindow(itemSeleccionado.Nombre, disponiblesParaDevolver);
                ventanaCantidad.Owner = this;

                // Si el usuario confirma la cantidad dándole al botón azul o Enter
                if (ventanaCantidad.ShowDialog() == true)
                {
                    decimal cantidadAReturnar = ventanaCantidad.CantidadSeleccionada;

                    // Calculamos el reembolso exacto basado ÚNICAMENTE en la cantidad elegida
                    decimal dineroAReembolsar = cantidadAReturnar * itemSeleccionado.PrecioVenta;

                    var confirmacion = MessageBox.Show($"¿Deseas procesar la devolución parcial de {cantidadAReturnar:0.###}x {itemSeleccionado.Nombre}?\n\n" +
                                                       $"Se registrará una salida de caja de {dineroAReembolsar:C} y el stock volverá al inventario.",
                                                       "Confirmar Devolución", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (confirmacion == MessageBoxResult.Yes)
                    {
                        try
                        {
                            DevolucionLogica devolucion = new DevolucionLogica();

                            // Mandamos la cantidad exacta que eligió el usuario
                            bool exito = devolucion.ProcesarDevolucion(
                                itemSeleccionado.IdDetalle,
                                itemSeleccionado.Codigo,
                                cantidadAReturnar,
                                dineroAReembolsar,
                                SesionGlobal.UsuarioActual.IdUsuario,
                                ticketSeleccionado.Folio
                            );

                            if (exito)
                            {
                                MessageBox.Show("Devolución parcial procesada con éxito. Entrega el efectivo al cliente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Refrescamos la tabla de artículos asíncronamente
                                dgDetalles.ItemsSource = await logica.VerDetallesAsync(ticketSeleccionado.IdVenta);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor selecciona un artículo de la lista de la derecha para devolver.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 4. ASÍNCRONO: Filtro por fechas con UX mejorada
        private async void BtnBuscarFechas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar que el cajero sí haya elegido ambas fechas
                if (!dpInicio.SelectedDate.HasValue || !dpFin.SelectedDate.HasValue)
                {
                    MessageBox.Show("Por favor selecciona una Fecha de Inicio y una Fecha de Fin.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime fechaInicio = dpInicio.SelectedDate.Value;
                DateTime fechaFin = dpFin.SelectedDate.Value;

                // Bloqueamos el botón visualmente para evitar dobles clics
                btnBuscarFechas.IsEnabled = false;
                btnBuscarFechas.Content = "⏳...";

                // Traer la información filtrada asíncronamente
                dgTickets.ItemsSource = await logica.BuscarTicketsPorRangoAsync(fechaInicio, fechaFin);

                // Limpiamos la tabla de la derecha por si había algo seleccionado antes
                dgDetalles.ItemsSource = null;
                ticketSeleccionado = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Filtro Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                // Pase lo que pase, regresamos el botón a la normalidad
                btnBuscarFechas.IsEnabled = true;
                btnBuscarFechas.Content = "🔍 FILTRAR";
            }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Si la tecla presionada es Escape, cerramos la ventana actual
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }
    }
}