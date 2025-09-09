
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace MinimalApi.Dominio.Entidades.Servicos;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;

    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    // Cadastro via DTO
    public Administrador Incluir(AdministradorDTO administradorDTO)
    {
        var administrador = new Administrador
        {
            Email = administradorDTO.Email,
            Senha = administradorDTO.Senha,
            Perfil = administradorDTO.Perfil.ToString()
        };

        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();
        return administrador;
    }

    // Cadastro via objeto completo
    public Administrador Incluir(Administrador administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();
        return administrador;
    }

    // Login
    public List<Administrador> Login(LoginDTO loginDTO)
    {
        var email = loginDTO.Email.ToLower().Trim();
        var senha = loginDTO.Senha.Trim();

        return _contexto.Administradores
            .Where(a => a.Email.ToLower() == email && a.Senha == senha)
            .ToList();
    }

    // Listagem com paginação
    public List<Administrador> Todos(int? pagina)
    {
        int paginaAtual = pagina ?? 1;
        int tamanhoPagina = 10;

        return _contexto.Administradores
            .Skip((paginaAtual - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();
    }

    // Busca por ID
    public Administrador? BuscaPorId(int id)
    {
        return _contexto.Administradores.FirstOrDefault(a => a.Id == id);
    }
}
