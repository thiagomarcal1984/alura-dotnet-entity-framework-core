# Uso de Banco de Dados

## Usando banco de dados
Para visualizar a estrutura do banco de dados local a partir do Visual Studio 2022, acesse o menu `Exibir -> Pesquisador de objetos do SQL Server...`.

A partir daí, você pode usar uma interface gráfica para se comunicar com o servidor local do SQL Server. Você poderá usar uma GUI para fazer operações CRUD e executar scripts SQL.

## Conectando ao BD
Vamos instalar o pacote NuGet para permitir a conexão com o SQL Server. Acesse o menu `Ferramentas -> Gerenciador de Pacotes do NuGet -> Gerenciar Pacotes do NuGet para a Solução...`.
Em seguida procure pelo pacote `Microsoft.Data.SqlClient`.

> Para o projeto atual, foi necessário fazer o downgrade para a versão 5.2.2.

Na prática, o que a interface gráfica faz é inserir no arquivo `ScreenSound.csproj` a seguinte marcação XML: 
```XML
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Resto do código -->
    <TargetFramework>net6.0</TargetFramework>
    <!-- Resto do código -->
  </PropertyGroup>

  <!-- Resto do código -->
  
  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
  </ItemGroup>

</Project>
```

Veja o código a seguir: 

```CSharp
// Banco/Connection.cs
using Microsoft.Data.SqlClient;
// Resto do código
namespace ScreenSound.Banco;

internal class Connection 
{
	private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ScreenSound;Integrated Security=True;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
	public SqlConnection ObterConexao() 
	{
		return new SqlConnection(connectionString);
	}
}
```
```CSharp
// Program.cs
using ScreenSound.Banco;

try
{
    using var connection = new Connection().ObterConexao();
    connection.Open();
    Console.WriteLine(connection.State);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
return;
```

> A variável `connection` vai ser descartada quando o bloco `try` for encerrado. Isso acontece por causa da palavra reservada `using`.

Para rodar o projeto a partir da linha de comando, vá até a raiz do diretório da solução (onde está o arquivo .sln) e use o comando:
```
dotnet run --project NomeDoProjeto
```
## Consulta de Artistas
```Csharp
// Banco/Connection.cs
using ScreenSound.Modelos;

// Resto do código
internal class Connection 
{
    // Resto do código
	public static IEnumerable<Artista> Listar()
	{
		var lista = new List<Artista>();
		using var connection = ObterConexao();
		connection.Open();

		string sql = "SELECT * FROM Artistas";

		SqlCommand command = new SqlCommand(sql, connection);
		using SqlDataReader dataReader = command.ExecuteReader();
		while (dataReader.Read()) {
			string nomeArtista = Convert.ToString(dataReader["Nome"]);
			string bioArtista = Convert.ToString(dataReader["Bio"]);
			int idArtista = Convert.ToInt32(dataReader["id"]);
			Artista artista = new Artista(nomeArtista, bioArtista) { Id = idArtista};

			lista.Add(artista);
		}

		return lista;
	}
}
```
> Os passos para ler do banco são: 
> 1. Criamos um `SqlCommand` com a string do código SQL e um `SqlConnection`;
> 2. Criamos um `SqlDataReader` a partir do método de objeto `SqlCommand.ExecuteReader()`;
> 3. Percorremos o `SqlDataReader` enquanto o método de objeto `SqlDataReader.Read()` for verdadeiro;
> 4. Para cada coluna do resultado do SQL, acessamos com o padrão `dataReader["nomeColuna"]`.
> 
> Todos as classes mencionadas fazem parte do ADO.NET (ActiveX Data Objects .NET).

Agora, a execução do código no programa principal: 
```Csharp
// Program.cs
using ScreenSound.Banco;
// using ScreenSound.Menus;
using ScreenSound.Modelos;


try
{
    var listaArtistas = Connection.Listar();
    foreach (Artista artista in listaArtistas)
    {
        Console.WriteLine(artista);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
return;
```

# Incluindo Artista
Vamos criar uma camada de acesso a dados (DAL) na classe `ArtistaDAL`:

```Csharp
// Banco/ArtistaDAL.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class ArtistaDAL
    {
        public static IEnumerable<Artista> Listar()
        {
            // Resto do código.
        }

        public static void Adicionar(Artista artista)
        {
            using var connection = Connection.ObterConexao();
            connection.Open();

            string sql = "INSERT INTO Artistas (Nome, FotoPerfil, Bio) VALUES (@nome, @perfilPadrao, @bio)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@nome", artista.Nome);
            command.Parameters.AddWithValue("@perfilPadrao", artista.FotoPerfil);
            command.Parameters.AddWithValue("@bio", artista.Bio);

            int retorno = command.ExecuteNonQuery();
            Console.WriteLine($"Linhas afetadas: {retorno}");
        }
    }
}
```

A inserção é realizada por meio do método de objeto `SqlCommand.ExecuteNonQuery()`, que retorno um número inteiro que representa o número de linhas afetadas no banco de dados.

Note também que é necessário acrescentar os parâmetros ao comando (`command.Parameters.AddWithValue("@parmComArroba", valorDoParm)`).

> Note que o método `Listar` foi migrado da classe `Connection` para a classe `ArtistaDAL`.
>
> Note também a sintaxe de string formatada do C# (é precedida por um cifrão, ao invés da letra f - como é usado no Python).

Testando o código no programa principal:
```Csharp
// Program.cs
// Resto do código.

try
{
    // Teste de inserção de um novo artista.
    ArtistaDAL.Adicionar(new Artista(
        "Foo Fighters",
        "Biografia do Foo Fighters"
    ));

    var listaArtistas = ArtistaDAL.Listar();
    
    foreach (Artista artista in listaArtistas)
    {
        Console.WriteLine(artista);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
return;
```

## Mão na massa: incluindo os métodos Atualizar e Deletar
```Csharp
// Banco/ArtistaDAL.cs

// Resto do código
using Microsoft.Data.SqlClient;
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class ArtistaDAL
    {
        // Resto do código

        public static void Atualizar(Artista artista)
        {
            string sql = "UPDATE Artistas SET Nome = @nome, Bio = @bio WHERE id = @id";
            
            using var connection = Connection.ObterConexao();
            connection.Open();

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", artista.Id);
            command.Parameters.AddWithValue("@nome", artista.Nome);
            command.Parameters.AddWithValue("@bio", artista.Bio);

            int retorno = command.ExecuteNonQuery();
            Console.WriteLine($"Linhas atualizadas: {retorno}");
        }

        public static void Deletar(Artista artista)
        {
            string sql = "DELETE FROM Artistas WHERE id = @id";

            using var connection = Connection.ObterConexao();
            connection.Open();

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", artista.Id);
            int retorno = command.ExecuteNonQuery();
            Console.WriteLine($"Linhas deletadas: {retorno}");
        }
    }
}
```
Na essência, o código .NET para atualização e remoação não é diferente da inserção.

> Lembre-se da palavra reservada `using`: isso facilita o descarte da conexão após a execução do bloco de código.
