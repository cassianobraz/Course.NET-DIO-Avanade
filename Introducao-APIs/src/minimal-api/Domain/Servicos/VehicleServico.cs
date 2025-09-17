using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Interfaces;
using minimal_api.Infra.Db;

namespace minimal_api.Domain.Servicos;

public class VehicleServico : IVehicleServico
{
    private readonly DbContextDio _context;

    public VehicleServico(DbContextDio context) => _context = context;

    public void Apagar(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();
    }

    public void Atualizar(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
        _context.SaveChanges();
    }

    public Vehicle? BuscarPorId(int id)
    {
        return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();
    }

    public void Incluir(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();
    }

    public List<Vehicle> Todos(int? pagina = 1, string? nome = null, string? marca = null)
    {
        var query = _context.Vehicles.AsQueryable();
        if (!string.IsNullOrEmpty(nome))
            query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome}%"));

        int itensPorPagina = 10;

        if(pagina != null)
            query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

        return query.ToList();
    }
}
