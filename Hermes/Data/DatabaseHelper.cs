namespace Hermes.Data
{
    public static class DatabaseHelper
    {
        public static string GetConnectionString()
        {
            return @"Server=pan\SQLEXPRESS;Database=HERMES;Trusted_Connection=True;TrustServerCertificate=True;";
        }
    }
}
