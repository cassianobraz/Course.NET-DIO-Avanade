using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Servicos;
using minimal_api.Infra.Db;
using System.Reflection;

namespace Minimal.Api.Teste.Domain.Servicos;

[TestClass]
public class AdministradorServicoTest
{
    private DbContextDio CriarContextoDeTeste()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));

        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContextDio(configuration);
    }

    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Admins");

        var adm = new Admin();
        adm.Id = 1;
        adm.Email = "admin@teste.com";
        adm.Senha = "123456";
        adm.Perfil = "Admin";
        var administradorServico = new AdminServico(context);

        // Act
        administradorServico.Incluir(adm);

        // Assert
        Assert.AreEqual(1, administradorServico.Todos(1).Count());
    }

    [TestMethod]
    public void TestandoBuscaPorId()
    {
        // Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Admins");

        var adm = new Admin();
        adm.Id = 1;
        adm.Email = "admin@teste.com";
        adm.Senha = "123456";
        adm.Perfil = "Admin";
        var administradorServico = new AdminServico(context);

        // Act
        var admin = administradorServico.BuscarPorId(adm.Id);

        // Assert
        Assert.AreEqual(1, admin.Id);
    }
}
