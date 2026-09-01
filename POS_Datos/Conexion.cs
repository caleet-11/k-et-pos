using System;
using MySql.Data.MySqlClient;
// Agrega esta librería arriba
using System.Configuration;

namespace POS_Datos
{
    public class Conexion
    {
        // ¡Adiós a la contraseña quemada en el código!
        private readonly string cadenaConexion;

        public Conexion()
        {
            // El programa irá a buscar la contraseña al App.config de forma dinámica
            cadenaConexion = ConfigurationManager.ConnectionStrings["ConexionPOS"].ConnectionString;
        }

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(cadenaConexion);
        }
    }
}