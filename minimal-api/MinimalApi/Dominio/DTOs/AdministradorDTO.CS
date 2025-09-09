using MinimalApi.Dominio.Enuns;
using System.Text.Json.Serialization;

namespace MinimalApi.DTOs;

public class AdministradorDTO
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Perfil Perfil { get; set; } = default!;
}
