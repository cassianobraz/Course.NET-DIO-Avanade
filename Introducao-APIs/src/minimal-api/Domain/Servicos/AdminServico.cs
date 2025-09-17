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
}
