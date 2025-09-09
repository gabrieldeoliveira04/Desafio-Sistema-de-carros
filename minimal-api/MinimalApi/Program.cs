using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Entidades.Servicos;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Serviços
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculosServicos, VeiculosServicos>();

// 🔹 Conexão com DB
var connectionString = builder.Configuration.GetConnectionString("mysql");
builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 🔹 Configuração JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "sua-chave-secreta");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});
builder.Services.AddAuthorization();

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Minimal API",
        Version = "v1",
        Description = "API de exemplo com CRUD de Administradores e Veículos usando JWT"
    });

    // JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

// 🔹 Garantir banco
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContexto>();
    db.Database.EnsureCreated();
}

#region Home
app.MapGet("/", () => Results.Json(new Home()))
   .WithTags("Home")
   .WithName("Home")
   .WithMetadata(new { Description = "Rota inicial que retorna informações básicas da API" });
#endregion

#region Validações
ErrosdeValidacao ValidaVeiculoDTO(VeiculoDTO veiculoDTO)
{
    var erros = new ErrosdeValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        erros.Mensagens.Add("O nome do veículo é obrigatório.");
    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        erros.Mensagens.Add("A marca do veículo é obrigatória.");
    if (!int.TryParse(veiculoDTO.Ano, out int ano) || ano < 1950)
        erros.Mensagens.Add("O ano do veículo deve ser um número maior que 1950.");

    return erros;
}

ErrosdeValidacao ValidaAdministradorDTO(AdministradorDTO administradorDTO)
{
    var erros = new ErrosdeValidacao { Mensagens = new List<string>() };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        erros.Mensagens.Add("O email é obrigatório.");
    if (string.IsNullOrEmpty(administradorDTO.Senha) || administradorDTO.Senha.Length < 6)
        erros.Mensagens.Add("A senha é obrigatória e deve ter pelo menos 6 caracteres.");
    if (!Enum.IsDefined(typeof(Perfil), administradorDTO.Perfil))
        erros.Mensagens.Add("O perfil é obrigatório.");

    return erros;
}
#endregion

#region Token JWT
string GerarToken(Administrador admin, IConfiguration config)
{
    var key = Encoding.ASCII.GetBytes(config["Jwt:Key"] ?? "sua-chave-secreta");
    var tokenHandler = new JwtSecurityTokenHandler();

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, admin.Email),
            new Claim(ClaimTypes.Role, admin.Perfil.ToString())
        }),
        Expires = DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpireMinutes"] ?? "60")),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        Issuer = config["Jwt:Issuer"],
        Audience = config["Jwt:Audience"]
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
#endregion

#region Administradores
app.MapPost("/administradores", [Authorize(Roles = "Administrador")]
([FromBody] AdministradorDTO dto, IAdministradorServico servico) =>
{
    var erros = ValidaAdministradorDTO(dto);
    if (erros.Mensagens.Any())
        return Results.BadRequest(erros);

    var admin = servico.Incluir(dto);
    return Results.Created($"/administradores/{admin.Id}", admin);
})
.WithTags("Administradores")
.WithMetadata(new { Description = "Cria um novo administrador" });

app.MapGet("/administradores", [Authorize(Roles = "Administrador")]
(IAdministradorServico servico, int? pagina) =>
{
    var admins = servico.Todos(pagina);
    return Results.Ok(admins);
})
.WithTags("Administradores")
.WithMetadata(new { Description = "Lista todos os administradores com paginação" });

app.MapGet("/administradores/{id}", [Authorize(Roles = "Administrador")]
(IAdministradorServico servico, int id) =>
{
    var admin = servico.BuscaPorId(id);
    return admin != null ? Results.Ok(admin) : Results.NotFound();
})
.WithTags("Administradores")
.WithMetadata(new { Description = "Busca um administrador pelo ID" });

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico servico, IConfiguration config) =>
{
    var admins = servico.Login(loginDTO);
    if (!admins.Any())
        return Results.Unauthorized();

    var token = GerarToken(admins.First(), config);
    return Results.Ok(new { Token = token });
})
.WithTags("Administradores")
.WithMetadata(new { Description = "Login de administrador e geração de token JWT" });
#endregion

#region Veículos
app.MapPost("/veiculos", [Authorize(Roles = "Administrador,Editor")]
([FromBody] VeiculoDTO dto, IVeiculosServicos servico) =>
{
    var erros = ValidaVeiculoDTO(dto);
    if (erros.Mensagens.Any())
        return Results.BadRequest(erros);

    var veiculo = new Veiculo
    {
        Nome = dto.Nome,
        Marca = dto.Marca,
        Ano = dto.Ano
    };

    servico.Incluir(veiculo);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
})
.WithTags("Veículos")
.WithMetadata(new { Description = "Cria um novo veículo" });

app.MapGet("/veiculos", [Authorize(Roles = "Administrador,Editor")]
(IVeiculosServicos servico, string? nome, string? marca, int pagina = 1) =>
{
    var veiculos = servico.Todos(nome, marca, pagina);
    return Results.Ok(veiculos);
})
.WithTags("Veículos")
.WithMetadata(new { Description = "Lista veículos com filtros opcionais e paginação" });

app.MapGet("/veiculos/{id}", [Authorize(Roles = "Administrador,Editor")]
(IVeiculosServicos servico, int id) =>
{
    var veiculo = servico.BuscaPorId(id);
    return veiculo != null ? Results.Ok(veiculo) : Results.NotFound();
})
.WithTags("Veículos")
.WithMetadata(new { Description = "Busca um veículo pelo ID" });

app.MapPut("/veiculos/{id}", [Authorize(Roles = "Administrador,Editor")]
([FromBody] VeiculoDTO dto, IVeiculosServicos servico, int id) =>
{
    var veiculo = servico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculo.Nome = dto.Nome;
    veiculo.Marca = dto.Marca;
    veiculo.Ano = dto.Ano;

    servico.Atualizar(id, veiculo);
    return Results.Ok(veiculo);
})
.WithTags("Veículos")
.WithMetadata(new { Description = "Atualiza um veículo existente pelo ID" });

app.MapDelete("/veiculos/{id}", [Authorize(Roles = "Administrador,Editor")]
(IVeiculosServicos servico, int id) =>
{
    var veiculo = servico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    servico.Apagar(veiculo);
    return Results.NoContent();
})
.WithTags("Veículos")
.WithMetadata(new { Description = "Apaga um veículo pelo ID" });
#endregion

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API V1");
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();
