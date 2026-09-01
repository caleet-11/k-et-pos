using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace POS_Logica
{
    public class BasculaLogica
    {
        // CAMBIA ESTO A 'false' CUANDO COMPRES TU BÁSCULA FÍSICA
        private bool modoSimulador = true;

        /// <summary>
        /// Se conecta al puerto COM y extrae el peso exacto del plato.
        /// </summary>
        public decimal ObtenerPeso()
        {
            if (modoSimulador)
            {
                // Fingimos que la báscula tarda medio segundo en pensar
                Thread.Sleep(500);

                // Devolvemos un peso de prueba (1 kilo con 250 gramos)
                return 1.250m;
            }

            // --- ESTE ES EL CÓDIGO REAL PARA TU FUTURA BÁSCULA ---
            decimal pesoFinal = 0m;

            try
            {
                // Configuramos la "frecuencia" a la que hablan casi todas las básculas comerciales (9600 baudios)
                using (SerialPort puerto = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One))
                {
                    puerto.Open();

                    // Mandamos el pulso 'P' para que la báscula nos escupa el número de la pantalla
                    puerto.Write("P");
                    Thread.Sleep(100);

                    string respuesta = puerto.ReadExisting().Trim();

                    // Limpiamos la respuesta (por si la báscula manda "1.250 KG", le quitamos las letras)
                    string soloNumeros = Regex.Replace(respuesta, "[^0-9.]", "");

                    decimal.TryParse(soloNumeros, out pesoFinal);
                }
            }
            catch (Exception ex)
            {
                // OPTIMIZACIÓN: Pasamos la excepción 'ex' para no perder el rastro del error de hardware
                throw new Exception("Error al leer la báscula física", ex);
            }

            return pesoFinal;
        }
    }
}