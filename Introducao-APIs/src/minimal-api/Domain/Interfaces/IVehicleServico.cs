using minimal_api.Domain.Entity;

namespace minimal_api.Domain.Interfaces;

public interface IVehicleServico
{
    List<Vehicle> Todos(int? pagina = 1, string? nome = null, string? marca = null);
    Vehicle? BuscarPorId(int id);
    void Incluir(Vehicle vehicle);
    void Atualizar(Vehicle vehicle);
    void Apagar(Vehicle vehicle);
}
