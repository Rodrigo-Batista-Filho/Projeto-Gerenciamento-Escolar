using System.Linq;
using EM.Domain;
using EM.Web.Interfaces;
using EM.Web.Utils;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace EM.Web.Services
{
    public class RelatorioService : IRelatorioService
    {
        public byte[] GerarRelatorioAlunosPDF(IEnumerable<Aluno> alunos)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var pdfWriter = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(pdfWriter);
                var document = new Document(pdfDocument, PageSize.A4);
                document.SetMargins(36, 36, 54, 36);

                var fontePadrao = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var fonteNegrito = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var headerTable = new Table(1).UseAllAvailableWidth();
            var headerCell = new Cell()
                .Add(new Paragraph("ESCOLAR MANAGER\nRELATÓRIO DE ALUNOS").SetFont(fonteNegrito).SetFontSize(14))
                .SetBackgroundColor(new DeviceRgb(220, 53, 69))
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(10)
                .SetBorder(Border.NO_BORDER);
            headerTable.AddCell(headerCell);
            document.Add(headerTable);
            document.Add(new Paragraph($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFont(fontePadrao).SetFontSize(10).SetTextAlignment(TextAlignment.RIGHT));
            var tabelaAlunos = new Table(new float[] { 12, 28, 18, 14, 10, 18, 10 })
                .UseAllAvailableWidth();

            AddHeader(tabelaAlunos, "Matrícula", fonteNegrito);
            AddHeader(tabelaAlunos, "Nome", fonteNegrito);
            AddHeader(tabelaAlunos, "CPF", fonteNegrito);
            AddHeader(tabelaAlunos, "Nascimento", fonteNegrito);
            AddHeader(tabelaAlunos, "Sexo", fonteNegrito);
            AddHeader(tabelaAlunos, "Cidade", fonteNegrito);
            AddHeader(tabelaAlunos, "UF", fonteNegrito);

            var listaAlunos = alunos?.ToList() ?? new List<Aluno>();
            
            if (!listaAlunos.Any())
            {
                var celulaSemDados = new Cell(1, 7)
                    .Add(new Paragraph("Nenhum aluno encontrado para os filtros aplicados.")
                        .SetFont(fontePadrao).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(10);
                tabelaAlunos.AddCell(celulaSemDados);
            }
            else
            {
                foreach (var aluno in listaAlunos)
                {
                    AddCell(tabelaAlunos, aluno.Matricula.ToString(), fontePadrao);
                    AddCell(tabelaAlunos, aluno.Nome ?? "—", fontePadrao);
                    AddCell(tabelaAlunos, FormatadorCPF.Formatar(aluno.CPF), fontePadrao);
                    var nascimentoTexto = (aluno.Nascimento == default ? "—" : aluno.Nascimento.ToString("dd/MM/yyyy"));
                    AddCell(tabelaAlunos, nascimentoTexto, fontePadrao);
                    AddCell(tabelaAlunos, aluno.Sexo.ToString(), fontePadrao);
                    AddCell(tabelaAlunos, aluno.CidadeNome ?? "—", fontePadrao);
                    AddCell(tabelaAlunos, aluno.UF ?? "—", fontePadrao);
                }
            }

            document.Add(tabelaAlunos);

            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var pagina = pdfDocument.GetPage(i);
                var tamanhoPagina = pagina.GetPageSize();
                var canvas = new PdfCanvas(pagina);
                var rodape = new Paragraph($"Página {i} de {pdfDocument.GetNumberOfPages()}")
                    .SetFont(fontePadrao).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER);
                new Canvas(canvas, tamanhoPagina)
                    .ShowTextAligned(rodape, tamanhoPagina.GetWidth() / 2, 20, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
            }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception)
            {
                using var erroStream = new MemoryStream();
                var erroWriter = new PdfWriter(erroStream);
                var erroPdf = new PdfDocument(erroWriter);
                var erroDoc = new Document(erroPdf, PageSize.A4);
                
                var fonteErro = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                erroDoc.Add(new Paragraph("Erro ao gerar relatório")
                    .SetFont(fonteErro).SetFontSize(16).SetTextAlignment(TextAlignment.CENTER));
                erroDoc.Add(new Paragraph("Ocorreu um problema na geração do relatório. Tente novamente mais tarde.")
                    .SetFont(fonteErro).SetFontSize(12));
                
                erroDoc.Close();
                return erroStream.ToArray();
            }
        }



        private void AddHeader(Table table, string text, PdfFont font)
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(10))
                .SetBackgroundColor(new DeviceRgb(230, 230, 230))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(6));
        }

        private void AddCell(Table table, string text, PdfFont font)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(10))
                .SetPadding(5));
        }

        
    }
}
