namespace POS_Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int IdRol { get; set; }

        public string NombreRol { get; set; } = string.Empty;
        public int NumeroFila { get; set; }
        public int IntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }

    }
}