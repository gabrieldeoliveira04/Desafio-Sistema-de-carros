using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Entidades.Serviços;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;

    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public List<Administrador> Login(LoginDTO loginDTO)
    {
        var email = loginDTO.Email.ToLower().Trim();
        var senha = loginDTO.Senha.Trim();

        Console.WriteLine($"[DEBUG] Tentando login com: {email} / {senha}");

        var admins = _contexto.Administradores
            .Where(a => a.Email.ToLower() == email && a.Senha == senha)
            .ToList();

        Console.WriteLine($"[DEBUG] Encontrados: {admins.Count}");

        return admins;
    }
}
