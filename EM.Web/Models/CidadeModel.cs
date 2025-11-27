using System.ComponentModel.DataAnnotations;

namespace Em.Web.Models
{
    public class CidadeModel
    {
        public int Codigo { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string UF { get; set; } = string.Empty;
    }
}

