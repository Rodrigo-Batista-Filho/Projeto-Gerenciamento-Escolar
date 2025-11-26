using Em.Web.Models;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository;
using Microsoft.AspNetCore.Mvc;

namespace EM.Web.Controllers
{
    public class CidadeController : Controller
    {
        private readonly RepositorioCidade _repositorioCidade;
        private readonly RepositorioAluno _repositorioAluno;

        public CidadeController(RepositorioCidade repositorioCidade, RepositorioAluno repositorioAluno)
        {
            _repositorioCidade = repositorioCidade;
            _repositorioAluno = repositorioAluno;
        }

        public IActionResult Index()
        {
            var cidades = _repositorioCidade.GetAll().OrderBy(cidade => cidade.Codigo)
                .Select(cidade => new CidadeModel
                {
                    Codigo = cidade.Codigo,
                    Nome = cidade.Nome,
                    UF = cidade.UF
                }).ToList();

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(cidades);
        }

        public IActionResult Create()
        {
            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(new CidadeModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CidadeModel model)
        {
            if (ModelState.IsValid)
            {
                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }

                var cidade = new Cidade
                {
                    Nome = model.Nome?.Trim() ?? string.Empty,
                    UF = model.UF?.ToUpper() ?? string.Empty
                };

                try
                {
                    _repositorioCidade.Add(cidade);
                    TempData["Success"] = "Cidade cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao cadastrar cidade: {ex.Message}");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }
            }

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var cidade = _repositorioCidade.GetByCodigo(id);
            if (cidade == null)
            {
                TempData["Error"] = "Cidade não encontrada";
                return RedirectToAction(nameof(Index));
            }

            var model = new CidadeModel
            {
                Codigo = cidade.Codigo,
                Nome = cidade.Nome,
                UF = cidade.UF
            };

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CidadeModel model)
        {
            if (id != model.Codigo)
            {
                TempData["Error"] = "Cidade não encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                if (!Validations.ValidarNome(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Nome deve ter entre 3 e 100 caracteres");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }

                var cidade = new Cidade
                {
                    Codigo = model.Codigo,
                    Nome = model.Nome?.Trim() ?? string.Empty,
                    UF = model.UF?.ToUpper() ?? string.Empty
                };

                try
                {
                    _repositorioCidade.Update(cidade);
                    TempData["Success"] = "Cidade atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao atualizar cidade: {ex.Message}");
                    ViewBag.UFs = _repositorioCidade.GetUFs();
                    return View(model);
                }
            }

            ViewBag.UFs = _repositorioCidade.GetUFs();
            return View(model);
        }


        public IActionResult Details(int id)
        {
            var cidade = _repositorioCidade.GetByCodigo(id);
            if (cidade == null)
            {
                TempData["Error"] = "Cidade não encontrada";
                return RedirectToAction(nameof(Index));
            }

            var model = new CidadeModel
            {
                Codigo = cidade.Codigo,
                Nome = cidade.Nome,
                UF = cidade.UF
            };


            ViewBag.Alunos = _repositorioAluno.GetByCidade(id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {

                if (_repositorioCidade.CidadeTemAlunos(id))
                {
                    TempData["Error"] = "Não é possível excluir a cidade pois existem alunos vinculados a ela.";
                    return RedirectToAction(nameof(Index));
                }

                var cidade = _repositorioCidade.GetByCodigo(id);
                if (cidade != null)
                {
                    _repositorioCidade.Remove(cidade);
                    TempData["Success"] = "Cidade excluída com sucesso!";
                }
                else
                {
                    TempData["Error"] = "Cidade não encontrada";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao excluir cidade: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Search(string searchType, string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue))
                return RedirectToAction(nameof(Index));

            try
            {
                var cidades = searchType?.ToLower() switch
                {
                    "uf" => _repositorioCidade.GetByUF(searchValue.ToUpper()),
                    _ => _repositorioCidade.GetByNome(searchValue)
                };

                var model = cidades.Select(cidade => new CidadeModel
                {
                    Codigo = cidade.Codigo,
                    Nome = cidade.Nome,
                    UF = cidade.UF
                }).ToList();

                ViewBag.UFs = _repositorioCidade.GetUFs();
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
    }
}