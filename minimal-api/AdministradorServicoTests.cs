using Xunit;
using MinimalApi.Dominio.Entidades.Serviços;
using MinimalApi.Dominio.Enuns;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;

namespace MinimalApi.Tests;

public class AdministradorServicoTests
{
    private DbContexto CriarContexto()
    {
        var options = new DbContextOptionsBuilder<DbContexto>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DbContexto(options);
    }

    [Fact]
    public void Deve_Cadastrar_Administrador()
    {
        // Arrange
        var contexto = CriarContexto();
        var servico = new AdministradorServico(contexto);

        var dto = new AdministradorDTO
        {
            Email = "teste@teste.com",
            Senha = "123456",
            Perfil = Perfil.Administrador
        };

        // Act
        var administrador = servico.Incluir(dto);

        // Assert
        Assert.NotNull(administrador);
        Assert.Equal("teste@teste.com", administrador.Email);
    }

    [Fact]
    public void Deve_Fazer_Login_Com_Sucesso()
    {
        // Arrange
        var contexto = CriarContexto();
        var servico = new AdministradorServico(contexto);

        var dto = new AdministradorDTO
        {
            Email = "login@teste.com",
            Senha = "654321",
            Perfil = Perfil.Editor
        };

        var administrador = servico.Incluir(dto);

        var loginDto = new LoginDTO
        {
            Email = "login@teste.com",
            Senha = "654321"
        };

        // Act
        var resultado = servico.Login(loginDto);

        // Assert
        Assert.NotEmpty(resultado);
        Assert.Equal(administrador.Email, resultado.First().Email);
    }

    [Fact]
    public void Nao_Deve_Logar_Com_Senha_Errada()
    {
        // Arrange
        var contexto = CriarContexto();
        var servico = new AdministradorServico(contexto);

        var dto = new AdministradorDTO
        {
            Email = "fail@teste.com",
            Senha = "123456",
            Perfil = Perfil.Editor
        };

        servico.Incluir(dto);

        var loginDto = new LoginDTO
        {
            Email = "fail@teste.com",
            Senha = "senha_errada"
        };

        // Act
        var resultado = servico.Login(loginDto);

        // Assert
        Assert.Empty(resultado);
    }
}
