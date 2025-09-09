# Desafio: Sistema de Carros

Repositório desenvolvido por **Gabriel de Oliveira** como entrega do desafio de criação de uma Minimal API com CRUD de Administradores e Veículos, usando JWT, autenticação, autorização por roles e documentação via Swagger.

## 🚀 Tecnologias

- .NET 8 Minimal API
- Entity Framework Core
- MySQL
- JWT (Json Web Token)
- Swagger / OpenAPI 3.0

## 📦 Funcionalidades

### Administradores
- `POST /administradores` → Criação de novos administradores (somente Admin)
- `GET /administradores` → Lista todos os administradores com paginação (somente Admin)
- `GET /administradores/{id}` → Busca administrador por ID (somente Admin)
- `POST /login` → Login de administrador e geração de token JWT

### Veículos
- `POST /veiculos` → Cria um veículo (Admin ou Editor)
- `GET /veiculos` → Lista veículos, com filtros opcionais `nome` e `marca` e paginação (Admin ou Editor)
- `GET /veiculos/{id}` → Busca veículo por ID (Admin ou Editor)
- `PUT /veiculos/{id}` → Atualiza veículo existente (Admin ou Editor)
- `DELETE /veiculos/{id}` → Apaga veículo pelo ID (Admin ou Editor)

### Home
- `GET /` → Endpoint inicial que retorna informações básicas da API

## 🔧 Configuração

1. Clone o repositório:
   ```bash
   git clone https://github.com/gabrieldeoliveira04/Desafio-Sistema-de-carros.git
Configure a string de conexão MySQL em appsettings.json:

json
Copiar código
"ConnectionStrings": {
  "mysql": "server=localhost;database=minimalapi;user=root;password=1234"
}
Configure JWT em appsettings.json:

json
Copiar código
"Jwt": {
  "Key": "sua-chave-secreta",
  "Issuer": "SeuProjeto",
  "Audience": "SeuProjeto",
  "ExpireMinutes": "60"
}
Execute a aplicação:

bash
Copiar código
dotnet run
Acesse o Swagger para testar todos os endpoints:

bash
Copiar código
http://localhost:<porta>/swagger
🧪 Testes
Testes de unidade e de request foram implementados para Administrador e Veículos.

Cobertura de persistência e validações implementadas.
