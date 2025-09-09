using MinimalApi.Dominio.Enuns;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Tests;

public class AdministradorPersistenciaTests
{
    private DbContexto CriarContexto(string dbName)
    {
        var options = new DbContextOptionsBuilder<DbContexto>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new DbContexto(options);
    }

    [Fact]
    public void Deve_Incluir_Administrador()
    {
        using var contexto = CriarContexto("DbIncluir");
        var servico = new AdministradorServico(contexto);

        var dto = new AdministradorDTO
        {
            Email = "teste@teste.com",
            Senha = "123456",
            Perfil = Perfil.Administrador
        };

        var admin = servico.Incluir(dto);

        Assert.NotNull(admin);
        Assert.Equal("teste@teste.com", admin.Email);
    }

    [Fact]
    public void Deve_Listar_Administradores()
    {
        using var contexto = CriarContexto("DbListar");
        var servico = new AdministradorServico(contexto);

        servico.Incluir(new AdministradorDTO { Email = "a@a.com", Senha = "123456", Perfil = Perfil.Administrador });
        servico.Incluir(new AdministradorDTO { Email = "b@b.com", Senha = "654321", Perfil = Perfil.Editor });

        var lista = servico.Todos(1);

        Assert.NotEmpty(lista);
        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public void Deve_Buscar_Administrador_Por_Id()
    {
        using var contexto = CriarContexto("DbBuscar");
        var servico = new AdministradorServico(contexto);

        var admin = servico.Incluir(new AdministradorDTO { Email = "buscar@teste.com", Senha = "123456", Perfil = Perfil.Editor });
        var encontrado = servico.BuscaPorId(admin.Id);

        Assert.NotNull(encontrado);
        Assert.Equal("buscar@teste.com", encontrado.Email);
    }
}
