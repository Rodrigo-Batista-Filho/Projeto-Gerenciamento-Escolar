using System.Globalization;
using EM.Repository.Banco;
using Microsoft.AspNetCore.Localization;
using EM.Web.Interfaces;
using EM.Web.Services;
using EM.Repository;

var builder = WebApplication.CreateBuilder(args);


Environment.SetEnvironmentVariable("ITEXT_BOUNCY_CASTLE_FACTORY_NAME", "bouncy-castle");


builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<RepositorioAluno>();
builder.Services.AddScoped<RepositorioCidade>();

var cadeiaConexao = builder.Configuration.GetConnectionString("FirebirdConnection");
if (string.IsNullOrWhiteSpace(cadeiaConexao))
{
    throw new InvalidOperationException("Connection string 'FirebirdConnection' não configurada");
}
DBHelper.Configure(cadeiaConexao);


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("pt-BR") };
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddMvc(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "O campo {0} é obrigatório.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (value, field) => $"O valor '{value}' não é válido para o campo {field}.");
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Aluno/Index");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Configure localization
app.UseRequestLocalization();

app.UseAuthorization();
app.UseSession();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Aluno}/{action=Index}/{id?}");



app.Run();
