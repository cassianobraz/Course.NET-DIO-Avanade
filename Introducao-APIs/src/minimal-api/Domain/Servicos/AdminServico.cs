using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entity;
using minimal_api.Domain.Interfaces;
using minimal_api.Infra.Db;

namespace minimal_api.Domain.Servicos;

public class AdminServico : IAdminServico
{
    private readonly DbContextDio _context;
    public AdminServico(DbContextDio context) => _context = context;

    public Admin? Login(LoginDTO loginDTO)
    {
        var admin = _context.Admins.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        return admin;
    }

    public Admin Incluir(Admin admin)
    {
        _context.Admins.Add(admin);
        _context.SaveChanges();

        return admin;
    }

    public List<Admin> Todos(int? pagina)
    {
        var query = _context.Admins.AsQueryable();

        int itensPorPagina = 10;

        if (pagina != null)
            query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

        return query.ToList();
    }

    public Admin? BuscarPorId(int? id)
    {
        return _context.Admins.Where(x => x.Id == id).FirstOrDefault();
    }
}
