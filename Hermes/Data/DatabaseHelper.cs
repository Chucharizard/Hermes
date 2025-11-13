namespace Hermes.Data
{
    public static class DatabaseHelper
    {
        public static string GetConnectionString()
        {
            return @"Server=PANADERO\PANCITO;Database=HERMES;Trusted_Connection=True;TrustServerCertificate=True;";
        }
    }
}
