using POS_Entidades;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace POS_Logica
{
    public class ImpresoraLogica
    {
        private bool modoSimulador = true; // Cambiar a 'false' cuando tengas la impresora

        /// <summary>
        /// Genera el diseño del ticket y lo manda a la impresora o al Bloc de Notas.
        /// </summary>
        public void ImprimirTicket(Venta ventaTicket)
        {
            // OPTIMIZACIÓN: Preasignamos una capacidad de memoria inicial (ej. 1024 caracteres).
            // Esto evita que el recolector de basura redimensione el arreglo interno durante el bucle.
            StringBuilder ticket = new StringBuilder(1024);

            ticket.AppendLine("       ABARROTES EL ESTUDIANTE       ");
            ticket.AppendLine("      La Paz, B.C.S, México          ");
            ticket.AppendLine("=====================================");
            ticket.AppendLine($"Folio: {ventaTicket.Folio}");
            ticket.AppendLine($"Fecha: {ventaTicket.FechaHora.ToString("dd/MM/yyyy HH:mm")}");
            ticket.AppendLine("=====================================");
            ticket.AppendLine("CANT  DESCRIPCION            IMPORTE ");
            ticket.AppendLine("-------------------------------------");

            // 2. Agregamos los artículos
            foreach (var item in ventaTicket.Detalles)
            {
                // Formateamos para que el texto se alinee bonito como en el súper
                string nombreCorto = item.Codigo.Length > 15 ? item.Codigo.Substring(0, 15) : item.Codigo.PadRight(15);
                decimal subtotal = item.Cantidad * item.PrecioVenta;

                ticket.AppendLine($"{item.Cantidad.ToString("0.##").PadRight(4)} {nombreCorto} ${subtotal.ToString("0.00").PadLeft(7)}");
            }

            ticket.AppendLine("-------------------------------------");
            ticket.AppendLine($"TOTAL A PAGAR:          ${ventaTicket.Total.ToString("0.00").PadLeft(8)}");
            ticket.AppendLine("=====================================");
            ticket.AppendLine("      ¡Gracias por su compra!        ");
            ticket.AppendLine("                                     ");
            ticket.AppendLine("                                     "); // Espacio para el corte de papel

            // 3. Decidimos a dónde mandar este texto
            if (modoSimulador)
            {
                // Guardamos el ticket en un archivo temporal y lo abrimos
                string rutaArchivo = Path.Combine(Path.GetTempPath(), $"Ticket_{ventaTicket.Folio}.txt");
                File.WriteAllText(rutaArchivo, ticket.ToString());

                // Simulamos el cajón abriéndose
                Debug.WriteLine(">>> COMANDO ESC/POS ENVIADO: Abriendo cajón de dinero (27, 112, 0, 25, 250) <<<");

                // Abrimos el Bloc de Notas para ver el ticket
                Process.Start(new ProcessStartInfo
                {
                    FileName = rutaArchivo,
                    UseShellExecute = true
                });
            }
            else
            {
                // AQUÍ IRÁ EL CÓDIGO REAL PARA MANDAR RAW BYTES POR USB A LA IMPRESORA TÉRMICA
                // (Lo agregaremos cuando tengas el equipo conectado)
            }
        }
    }
}