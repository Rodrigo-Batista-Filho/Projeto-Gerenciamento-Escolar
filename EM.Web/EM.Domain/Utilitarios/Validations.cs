using System.Text.RegularExpressions;

namespace EM.Domain.Utilitarios
{
    public static class Validations
    {
        public static bool ValidarCPF(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            var cpfNumerico = Regex.Replace(cpf, @"[^\d]", "");

            if (cpfNumerico.Length != 11)
                return false;

            bool todosDigitosIguais = true;
            for (int indice = 1; indice < 11 && todosDigitosIguais; indice++)
            {
                if (cpfNumerico[indice] != cpfNumerico[0])
                    todosDigitosIguais = false;
            }

            if (todosDigitosIguais)
                return false;

            int somaDigitos = 0;
            for (int indice = 0; indice < 9; indice++)
                somaDigitos += int.Parse(cpfNumerico[indice].ToString()) * (10 - indice);

            int restoDivisao = somaDigitos % 11;
            int primeiroDigito = restoDivisao < 2 ? 0 : 11 - restoDivisao;

            if (int.Parse(cpfNumerico[9].ToString()) != primeiroDigito)
                return false;

            somaDigitos = 0;
            for (int indice = 0; indice < 10; indice++)
                somaDigitos += int.Parse(cpfNumerico[indice].ToString()) * (11 - indice);

            restoDivisao = somaDigitos % 11;
            int segundoDigito = restoDivisao < 2 ? 0 : 11 - restoDivisao;

            return int.Parse(cpfNumerico[10].ToString()) == segundoDigito;
        }

        public static bool ValidarNome(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            return nome.Trim().Length >= 3 && nome.Length <= 100;
        }
    }
}