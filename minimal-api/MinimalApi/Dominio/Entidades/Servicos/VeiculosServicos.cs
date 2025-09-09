using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Entidades.Servicos;

public class VeiculosServicos : IVeiculosServicos
{
    private readonly DbContexto _contexto;

    public VeiculosServicos(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public void Incluir(Veiculo veiculo)
    {
        _contexto.Veiculos.Add(veiculo);
        _contexto.SaveChanges();
    }

    public void Atualizar(int id, Veiculo veiculo)
    {
        _contexto.Veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public void Apagar(Veiculo veiculo)
    {
        _contexto.Veiculos.Remove(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscaPorId(int id)
    {
        return _contexto.Veiculos.FirstOrDefault(v => v.Id == id);
    }

    public List<Veiculo> Todos(string? nome = "null", string? marca = "null", int pagina = 1)
    {
        var query = _contexto.Veiculos.AsQueryable();

        if (!string.IsNullOrEmpty(nome) && nome != "null")
            query = query.Where(v => v.Nome.Contains(nome));

        if (!string.IsNullOrEmpty(marca) && marca != "null")
            query = query.Where(v => v.Marca.Contains(marca));

        int pageSize = 10;
        query = query.Skip((pagina - 1) * pageSize).Take(pageSize);

        return query.ToList();
    }
}
