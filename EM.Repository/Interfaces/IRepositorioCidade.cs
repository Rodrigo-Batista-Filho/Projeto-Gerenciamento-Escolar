using System.Collections.Generic;
using EM.Domain;

namespace EM.Repository.Interfaces
{
    public interface IRepositorioCidade : IRepositorioBase<Cidade>
    {
        Cidade? GetByCodigo(int codigo);
        IEnumerable<Cidade> GetByUF(string uf);
        IEnumerable<Cidade> GetByNome(string nome);
        IEnumerable<string> GetUFs();
        bool CidadeTemAlunos(int codigoCidade);
    }
}

