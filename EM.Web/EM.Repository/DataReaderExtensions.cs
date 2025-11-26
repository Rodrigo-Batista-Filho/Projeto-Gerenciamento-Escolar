public static class DataReaderExtensions
{
    public static int? ToIntNullable(this object valor)
    {
        if (valor == null || valor == DBNull.Value)
            return null;

        if (int.TryParse(valor.ToString(), out int resultado))
            return resultado;

        return null;
    }

    public static string ToStringSafe(this object valor)
    {
        return valor?.ToString() ?? string.Empty;
    }
}