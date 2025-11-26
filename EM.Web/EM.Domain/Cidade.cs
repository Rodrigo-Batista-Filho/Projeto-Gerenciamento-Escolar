using EM.Domain.Interface;

namespace EM.Domain
{
    public class Cidade : IEntidade
    {
        public required string Nome { get; set; }
        public required string UF { get; set; }
        public int Codigo { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Cidade cidade && Nome == cidade.Nome && UF == cidade.UF;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nome, UF);
        }

        public override string ToString()
        {
            return $"{Nome} - {UF}";
        }
    }
}