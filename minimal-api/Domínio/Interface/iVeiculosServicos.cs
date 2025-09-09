using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IVeiculosServicos
{
    List<Veiculo> Todos(string? nome = "null", string? marca = "null", int pagina = 1);
    Veiculo? BuscaPorId(int id);

    void Incluir(Veiculo veiculo);
    void Atualizar(int id, Veiculo veiculo);

    void Apagar(Veiculo veiculo);
}
