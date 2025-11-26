using Microsoft.AspNetCore.Mvc;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository;
using Em.Web.Models;

namespace EM.Web.Controllers
{
    public class AlunoController : Controller
    {
        private readonly RepositorioAluno _repositorioAluno;
        private readonly RepositorioCidade _repositorioCidade;

        public AlunoController(RepositorioAluno repositorioAluno, RepositorioCidade repositorioCidade)
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
                    CPF = a.CPF,
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
                    if (!Validations.ValidarCPF(model.CPF))
                    {
                        ModelState.AddModelError("CPF", "CPF inválido");
                        CarregarViewBags();
                        return View(model);
                    }


                    if (_repositorioAluno.CPFExiste(model.CPF))
                    {
                        ModelState.AddModelError("CPF", "CPF já cadastrado");
                        CarregarViewBags();
                        return View(model);
                    }
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
                    CPF = model.CPF?.Replace(".", "").Replace("-", "") ?? string.Empty,
                    Nascimento = model.Nascimento,
                    Sexo = model.Sexo,
                    CidadeCodigo = model.CidadeCodigo
                };

                try
                {
                    _repositorioAluno.Add(aluno);
                    TempData["Success"] = "Aluno cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao cadastrar aluno: {ex.Message}");
                    CarregarViewBags();
                    return View(model);
                }
            }

            CarregarViewBags();
            return View(model);
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
                CPF = aluno.CPF,
                Nascimento = aluno.Nascimento,
                Sexo = aluno.Sexo,
                CidadeCodigo = aluno.CidadeCodigo
            };

            CarregarViewBags();
            return View(model);
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
                    if (!Validations.ValidarCPF(model.CPF))
                    {
                        ModelState.AddModelError("CPF", "CPF inválido");
                        CarregarViewBags();
                        return View(model);
                    }


                    if (_repositorioAluno.CPFExiste(model.CPF, model.Matricula))
                    {
                        ModelState.AddModelError("CPF", "CPF já cadastrado para outro aluno");
                        CarregarViewBags();
                        return View(model);
                    }
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
                    CPF = model.CPF?.Replace(".", "").Replace("-", "") ?? string.Empty,
                    Nascimento = model.Nascimento,
                    Sexo = model.Sexo,
                    CidadeCodigo = model.CidadeCodigo
                };

                try
                {
                    _repositorioAluno.Update(aluno);
                    TempData["Success"] = "Aluno atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao atualizar aluno: {ex.Message}");
                    CarregarViewBags();
                    return View(model);
                }
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
                CPF = aluno.CPF,
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
            try
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
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao excluir aluno: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Search(string searchType, string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue))
                return RedirectToAction(nameof(Index));

            try
            {
                var alunos = searchType?.ToLower() switch
                {
                    "matricula" when int.TryParse(searchValue, out int matricula) =>
                        new[] { _repositorioAluno.GetByMatricula(matricula) }.Where(a => a != null),
                    "cpf" =>
                        new[] { _repositorioAluno.GetByCPF(searchValue) }.Where(a => a != null),
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
                    CPF = a.CPF,
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
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro na pesquisa: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
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