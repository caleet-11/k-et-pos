namespace POS_Entidades
{
    public static class Sesion
    {
        // Al ser estáticas, puedes acceder a estas variables desde CUALQUIER ventana
        public static int IdUsuarioActual { get; set; }
        public static string NombreUsuarioActual { get; set; }
        public static int IdRolActual { get; set; }

        // Método opcional para limpiar la sesión cuando hagan LogOut
        public static void CerrarSesion()
        {
            IdUsuarioActual = 0;
            NombreUsuarioActual = string.Empty;
            IdRolActual = 0;
        }
    }
}