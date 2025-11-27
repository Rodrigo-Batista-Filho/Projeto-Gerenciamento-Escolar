using EM.Domain;
using EM.Repository;
using EM.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers
{
    public class RelatorioController : Controller
    {
        private readonly IRelatorioService _relatorioService;
        private readonly RepositorioAluno _repoAluno;
        private readonly RepositorioCidade _repoCidade;

        public RelatorioController(IRelatorioService relatorioService, RepositorioAluno repoAluno, RepositorioCidade repoCidade)
        {
            _relatorioService = relatorioService;
            _repoAluno = repoAluno;
            _repoCidade = repoCidade;
        }


        [HttpGet]
        public IActionResult AlunosPDF(string? searchType, string? searchValue)
        {
            IEnumerable<Aluno> alunos;

            var tipoBusca = (searchType ?? string.Empty).ToLowerInvariant();
            var valorBusca = (searchValue ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(tipoBusca) || string.IsNullOrWhiteSpace(valorBusca))
            {
                alunos = _repoAluno.GetAll();
            }
            else
            {
                alunos = tipoBusca switch
                {
                    "matricula" when int.TryParse(valorBusca, out var matricula) =>
                        new[] { _repoAluno.GetByMatricula(matricula) }.Where(a => a != null).Cast<Aluno>(),
                    "cpf" =>
                        new[] { _repoAluno.GetByCPF(valorBusca) }.Where(a => a != null).Cast<Aluno>(),
                    "sexo" when Enum.TryParse<EnumeradorSexo>(valorBusca, true, out var sexo) =>
                        _repoAluno.GetBySexo(sexo),
                    "cidade" =>
                        BuscarPorCidades(valorBusca),
                    _ => _repoAluno.GetByConteudoNoNome(valorBusca)
                };
            }

            alunos = (alunos ?? Enumerable.Empty<Aluno>())
                .Select(PreencherCidade);

            var listaAlunos = alunos.ToList();
            var bytes = _relatorioService.GerarRelatorioAlunosPDF(listaAlunos);
            Response.Headers["Content-Disposition"] = "inline; filename=Relatorio_Alunos.pdf";
            return File(bytes, "application/pdf");
        }

        private IEnumerable<Aluno> BuscarPorCidades(string valorBusca)
        {
            return _repoAluno.GetByConteudoNoNome(valorBusca);
        }

        private Aluno PreencherCidade(Aluno? aluno)
        {
            if (aluno?.CidadeCodigo is int codigoCidade)
            {
                var cidade = _repoCidade.GetByCodigo(codigoCidade);
                if (cidade != null)
                {
                    aluno.CidadeNome = cidade.Nome;
                    aluno.UF = cidade.UF;
                }
            }
            return aluno!;
        }
    }
}
