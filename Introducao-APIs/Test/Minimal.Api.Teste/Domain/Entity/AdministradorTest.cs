using minimal_api.Domain.Entity;

namespace Minimal.Api.Teste.Domain.Entity;

[TestClass]
public class AdministradorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var adm = new Admin();

        // Act
        adm.Id = 1;
        adm.Email = "admin@teste.com";
        adm.Senha = "123456";
        adm.Perfil = "Admin";

        // Assert
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("admin@teste.com", adm.Email);
        Assert.AreEqual("123456", adm.Senha);
        Assert.AreEqual("Admin", adm.Perfil);
    }
}
