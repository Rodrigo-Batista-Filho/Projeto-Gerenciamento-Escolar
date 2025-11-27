using Microsoft.AspNetCore.Mvc;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository;
using EM.Repository.Interfaces;
using Em.Web.Models;
using EM.Web.Utils;

namespace EM.Web.Controllers
{
    public class AlunoController : Controller
    {
        private readonly IRepositorioAluno _repositorioAluno;
        private readonly IRepositorioCidade _repositorioCidade;

        public AlunoController(IRepositorioAluno repositorioAluno, IRepositorioCidade repositorioCidade)
        {
            _repositorioAluno = repositorioAluno;
            _repositorioCidade = repositorioCidade;
        }


        public IActionResult Index()
        {
            var alunos = _repositorioAluno.GetAll().OrderBy(a => a.Matricula)
                .Select(PreencherCidade)
                .Select(a => new AlunoModel
                {
                    Matricula = a.Matricula,
                    Nome = a.Nome,
                    CPF = FormatadorCPF.Formatar(a.CPF),
                    Nascimento = a.Nascimento,
                    Sexo = a.Sexo,
                    CidadeCodigo = a.CidadeCodigo,
                    CidadeNome = a.CidadeNome,
                    UF = a.UF
                }).ToList();

            ViewBag.SearchTypes = new[] { "nome", "matricula" };
            return View(alunos);
        }


        public IActionResult Create()
        {
            ViewBag.Cidades = _repositorioCidade.GetAll()
                .OrderBy(c => c.Nome)
                .ToList();
            ViewBag.Sexos = Enum.GetValues(typeof(EnumeradorSexo))
                .Cast<EnumeradorSexo>()
                .ToList();

            return View(new AlunoModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AlunoModel model)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(model.CPF))
                {
                    string cpfLimpo = FormatadorCPF.Limpar(model.CPF);

                    if (!cpfLimpo.All(char.IsDigit))
                    {
                        ModelState.AddModelError("CPF", "CPF deve conter apenas números");
                        CarregarViewBags();
                        return View(model);
                    }

                    if (cpfLimpo.Length != 11)
                    {
                        ModelState.AddModelError("CPF", "CPF deve conter exatamente 11 dígitos");
                        CarregarViewBags();
                        return View(model);
                    }

                    if (!Validations.ValidarCPF(cpfLimpo))
                    {
                        ModelState.AddModelError("CPF", "CPF inválido");
                        CarregarViewBags();
                        return View(model);
                    }

                    if (_repositorioAluno.CPFExiste(cpfLimpo))
                    {
                        ModelState.AddModelError("CPF", "CPF já cadastrado");
                        CarregarViewBags();
                        return View(model);
                    }

                    model.CPF = cpfLimpo;
                }

                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    CarregarViewBags();
                    return View(model);
                }

                var aluno = new Aluno
                {
                    Nome = model.Nome?.Trim() ?? string.Empty,
                    CPF = model.CPF ?? string.Empty,
                    Nascimento = model.Nascimento,
                    Sexo = model.Sexo,
                    CidadeCodigo = model.CidadeCodigo
                };

                _repositorioAluno.Add(aluno);
                TempData["Success"] = "Aluno cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            CarregarViewBags();
            return View("Create", model);
        }


        private IEnumerable<Aluno> BuscarPorCidades(string nomeCidade)
        {
            var cidades = _repositorioCidade.GetByNome(nomeCidade)?.ToList() ?? new List<Cidade>();
            if (cidades.Count == 0) return Enumerable.Empty<Aluno>();

            var alunos = new List<Aluno>();
            foreach (var cidade in cidades)
            {
                alunos.AddRange(_repositorioAluno.GetByCidade(cidade.Codigo));
            }
            return alunos;
        }

        private Aluno PreencherCidade(Aluno? aluno)
        {
            if (aluno?.CidadeCodigo is int codigoCidade)
            {
                var cidade = _repositorioCidade.GetByCodigo(codigoCidade);
                if (cidade != null)
                {
                    aluno.CidadeNome = cidade.Nome;
                    aluno.UF = cidade.UF;
                }
            }
            return aluno!;
        }

        public IActionResult Edit(int id)
        {
            var aluno = _repositorioAluno.GetByMatricula(id);
            if (aluno == null)
            {
                TempData["Error"] = "Aluno não encontrado";
                return RedirectToAction(nameof(Index));
            }

            var model = new AlunoModel
            {
                Matricula = aluno.Matricula,
                Nome = aluno.Nome,
                CPF = FormatadorCPF.Formatar(aluno.CPF),
                Nascimento = aluno.Nascimento,
                Sexo = aluno.Sexo,
                CidadeCodigo = aluno.CidadeCodigo
            };

            CarregarViewBags();
            return View("Create", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AlunoModel model)
        {
            if (id != model.Matricula)
            {
                TempData["Error"] = "Aluno não encontrado";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(model.CPF))
                {
                    string cpfLimpo = FormatadorCPF.Limpar(model.CPF);

                    if (!cpfLimpo.All(char.IsDigit))
                    {
                        ModelState.AddModelError("CPF", "CPF deve conter apenas números");
                        CarregarViewBags();
                        return View(model);
                    }

                    if (cpfLimpo.Length != 11)
                    {
                        ModelState.AddModelError("CPF", "CPF deve conter exatamente 11 dígitos");
                        CarregarViewBags();
                        return View(model);
                    }

                    if (!Validations.ValidarCPF(cpfLimpo))
                    {
                        ModelState.AddModelError("CPF", "CPF inválido");
                        CarregarViewBags();
                        return View(model);
                    }

                    if (_repositorioAluno.CPFExiste(cpfLimpo, model.Matricula))
                    {
                        ModelState.AddModelError("CPF", "CPF já cadastrado para outro aluno");
                        CarregarViewBags();
                        return View(model);
                    }

                    model.CPF = cpfLimpo;
                }

                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    CarregarViewBags();
                    return View(model);
                }

                var aluno = new Aluno
                {
                    Matricula = model.Matricula,
                    Nome = model.Nome?.Trim() ?? string.Empty,
                    CPF = model.CPF ?? string.Empty,
                    Nascimento = model.Nascimento,
                    Sexo = model.Sexo,
                    CidadeCodigo = model.CidadeCodigo
                };

                _repositorioAluno.Update(aluno);
                TempData["Success"] = "Aluno atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            CarregarViewBags();
            return View(model);
        }


        public IActionResult Details(int id)
        {
            var aluno = _repositorioAluno.GetByMatricula(id);
            if (aluno == null)
            {
                TempData["Error"] = "Aluno não encontrado";
                return RedirectToAction(nameof(Index));
            }

            if (aluno.CidadeCodigo.HasValue)
            {
                var cidade = _repositorioCidade.GetByCodigo(aluno.CidadeCodigo.Value);
                if (cidade != null)
                {
                    aluno.CidadeNome = cidade.Nome;
                    aluno.UF = cidade.UF;
                }
            }

            var model = new AlunoModel
            {
                Matricula = aluno.Matricula,
                Nome = aluno.Nome,
                CPF = FormatadorCPF.Formatar(aluno.CPF),
                Nascimento = aluno.Nascimento,
                Sexo = aluno.Sexo,
                CidadeCodigo = aluno.CidadeCodigo,
                CidadeNome = aluno.CidadeNome,
                UF = aluno.UF
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var aluno = _repositorioAluno.GetByMatricula(id);
            if (aluno != null)
            {
                _repositorioAluno.Remove(aluno);
                TempData["Success"] = "Aluno excluído com sucesso!";
            }
            else
            {
                TempData["Error"] = "Aluno não encontrado";
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Search(string searchType, string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue))
                return RedirectToAction(nameof(Index));

            var alunos = searchType?.ToLower() switch
            {
                "matricula" when int.TryParse(searchValue, out int matricula) =>
                    new[] { _repositorioAluno.GetByMatricula(matricula) }.Where(a => a != null),
                "cpf" =>
                    new[] { _repositorioAluno.GetByCPF(FormatadorCPF.Limpar(searchValue)) }.Where(a => a != null),
                "sexo" when Enum.TryParse<EnumeradorSexo>(searchValue, true, out var sexo) =>
                    _repositorioAluno.GetBySexo(sexo),
                _ => _repositorioAluno.GetByConteudoNoNome(searchValue)
            };

            var model = alunos
                .Select(PreencherCidade)
                .Select(a => new AlunoModel
                {
                    Matricula = a.Matricula,
                    Nome = a.Nome,
                    CPF = FormatadorCPF.Formatar(a.CPF),
                    Nascimento = a.Nascimento,
                    Sexo = a.Sexo,
                    CidadeCodigo = a.CidadeCodigo,
                    CidadeNome = a.CidadeNome,
                    UF = a.UF
                }).ToList();

            ViewBag.SearchTypes = new[] { "nome", "matricula", "cpf", "sexo" };
            ViewBag.SearchType = searchType;
            ViewBag.SearchValue = searchValue;

            return View("Index", model);
        }

        private void CarregarViewBags()
        {
            ViewBag.Cidades = _repositorioCidade.GetAll()
                .OrderBy(c => c.Nome)
                .ToList();
            ViewBag.Sexos = Enum.GetValues(typeof(EnumeradorSexo))
                .Cast<EnumeradorSexo>()
                .ToList();
        }

        
    }
}
