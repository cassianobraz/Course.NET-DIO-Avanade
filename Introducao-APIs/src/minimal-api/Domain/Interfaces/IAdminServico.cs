using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entity;

namespace minimal_api.Domain.Interfaces;

public interface IAdminServico
{
    Admin? Login(LoginDTO loginDTO);
    Admin Incluir(Admin admin);
    Admin? BuscarPorId(int? id);
    List<Admin> Todos(int? pagina);
}
