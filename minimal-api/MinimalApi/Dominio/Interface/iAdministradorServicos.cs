using MinimalApi.DTOs;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorServico
{
    Administrador Incluir(AdministradorDTO administradorDTO);
    Administrador Incluir(Administrador administrador);
    List<Administrador> Login(LoginDTO loginDTO);
    List<Administrador> Todos(int? pagina);
    Administrador? BuscaPorId(int id);

}
