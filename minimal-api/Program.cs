using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Entidades.Serviços;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Entidades;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculosServicos, VeiculosServicos>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("mysql");

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

var app = builder.Build();
#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Veiculos
ErrosdeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var mensagens = new ErrosdeValidacao();
    mensagens.Mensagens = new List<string>();

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        mensagens.Mensagens.Add("O nome do veículo é obrigatório.");
    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        mensagens.Mensagens.Add("A marca do veículo é obrigatória.");
    if (!int.TryParse(veiculoDTO.Ano, out int ano) || ano < 1950)
        mensagens.Mensagens.Add("O ano do veículo deve ser um número maior que 1950.");

    return mensagens;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculosServicos veiculosServicos) =>
{
    var mensagens = validaDTO(veiculoDTO);
    if (mensagens.Mensagens.Count > 0)
        return Results.BadRequest(mensagens);

    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculosServicos.Incluir(veiculo);

    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculosServicos veiculosServicos) =>
{
    var veiculos = veiculosServicos.Todos(pagina: pagina ?? 1);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculosServicos veiculosServicos) =>
{
    var veiculo = veiculosServicos.BuscaPorId(id);

    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculosServicos veiculosServicos) =>
{
    var mensagens = validaDTO(veiculoDTO);
    if (mensagens.Mensagens.Count > 0)
        return Results.BadRequest(mensagens);

    var veiculo = veiculosServicos.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculosServicos.Atualizar(id, veiculo);
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculosServicos veiculosServicos) =>
{
    var veiculo = veiculosServicos.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();
    veiculosServicos.Apagar(veiculo);
    return Results.NoContent();
}).WithTags("Veiculos");
#endregion

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContexto>();
    db.Database.EnsureCreated();
}

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var resultado = administradorServico.Login(loginDTO);

    if (resultado.Any())
    {
        return Results.Ok("Login com sucesso");
    }
    return Results.Unauthorized();
}).WithTags("Administradores");
#endregion

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
