using System.Collections.Generic;
using EM.Domain;

namespace EM.Repository.Interfaces
{
    public interface IRepositorioAluno : IRepositorioBase<Aluno>
    {
        Aluno? GetByMatricula(int matricula);
        IEnumerable<Aluno> GetByConteudoNoNome(string termoNome);
        Aluno? GetByCPF(string cpf);
        IEnumerable<Aluno> GetBySexo(EnumeradorSexo sexo);
        IEnumerable<Aluno> GetByCidade(int codigoCidade);
        bool CPFExiste(string cpf, int? matriculaExcluir = null);
    }
}

