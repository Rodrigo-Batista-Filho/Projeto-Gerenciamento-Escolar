namespace EM.Web.Utils
{
    public static class FormatadorCPF
    {
        public static string Limpar(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;
            return cpf.Replace(".", "").Replace("-", "").Trim();
        }

        public static string Formatar(string cpf)
        {
            var limpo = Limpar(cpf);
            if (limpo.Length != 11) return cpf ?? string.Empty;
            return $"{limpo.Substring(0, 3)}.{limpo.Substring(3, 3)}.{limpo.Substring(6, 3)}-{limpo.Substring(9, 2)}";
        }
    }
}

