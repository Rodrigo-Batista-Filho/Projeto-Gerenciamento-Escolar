using System;
using System.ComponentModel.DataAnnotations;
using EM.Domain;

namespace Em.Web.Models
{
    public class AlunoModel
    {
        public int Matricula { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        public string CPF { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Nascimento { get; set; } = DateTime.Now.AddYears(-18);

        [Required]
        public EnumeradorSexo Sexo { get; set; }

        public int? CidadeCodigo { get; set; }
        public string CidadeNome { get; set; } = string.Empty;
        public string UF { get; set; } = string.Empty;

        public int Idade
        {
            get
            {
                var hoje = DateTime.Today;
                var idade = hoje.Year - Nascimento.Year;
                if (Nascimento.Date > hoje.AddYears(-idade)) idade--;
                return idade;
            }
        }

        public string CPFFormatado
        {
            get
            {
                var limpo = (CPF ?? string.Empty).Replace(".", "").Replace("-", "");
                if (limpo.Length != 11) return CPF ?? string.Empty;
                return $"{limpo.Substring(0, 3)}.{limpo.Substring(3, 3)}.{limpo.Substring(6, 3)}-{limpo.Substring(9, 2)}";
            }
        }

        public string SexoDescricao => Sexo == EnumeradorSexo.Masculino ? "Masculino" : "Feminino";
    }
}

