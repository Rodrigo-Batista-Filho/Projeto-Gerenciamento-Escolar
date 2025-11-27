using EM.Domain;
using EM.Repository.Interfaces;
using EM.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers
{
    public class RelatorioController(IRelatorioService relatorioService, IRepositorioAluno repoAluno, IRepositorioCidade repoCidade) : Controller
    {
        private readonly IRelatorioService _relatorioService = relatorioService;
        private readonly IRepositorioAluno _repoAluno = repoAluno;
        private readonly IRepositorioCidade _repoCidade = repoCidade;

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
            Response.Headers.ContentDisposition = "inline; filename=Relatorio_Alunos.pdf";
            return File(bytes, "application/pdf");
        }

        private IEnumerable<Aluno> BuscarPorCidades(string valorBusca)
        {
            var cidades = _repoCidade.GetByNome(valorBusca)?.ToList() ?? new List<Cidade>();
            if (cidades.Count == 0) return Enumerable.Empty<Aluno>();

            var alunos = new List<Aluno>();
            foreach (var cidade in cidades)
            {
                alunos.AddRange(_repoAluno.GetByCidade(cidade.Codigo));
            }
            return alunos;
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
