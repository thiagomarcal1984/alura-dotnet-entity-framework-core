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

# Bibliotecas ORM
## Instalando o Entity
Vamos instalar o pacote NuGet para permitir a conexão com o SQL Server. Acesse o menu `Ferramentas -> Gerenciador de Pacotes do NuGet -> Gerenciar Pacotes do NuGet para a Solução...`.
Em seguida procure pelo pacote `Microsoft.EntityFrameworkCore.SqlServer`.

> Alternativamente, escreva esta linha de código no arquivo do projeto (no caso, `ScreenSound.csproj`):
> ```XML
> <Project Sdk="Microsoft.NET.Sdk">
> 
>   <PropertyGroup>
>     <!-- Resto do código -->
>   </PropertyGroup>
> 
>   <!-- Resto do código -->
> 
>   <ItemGroup>
>     <!-- Resto do código -->
>     <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.14" />
>   </ItemGroup>
> 
> </Project>
> ```

A classe `Banco/Connection` será renomeada para `Banco/ScreenSoundContext` e vai ter o seguinte conteúdo:
```Csharp
using Microsoft.EntityFrameworkCore;
// Resto do código

namespace ScreenSound.Banco;

internal class ScreenSoundContext : DbContext
{
    public DbSet<Artista> Artistas { get; set; }

	private static string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ScreenSound;Integrated Security=True;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }
}
```
Note que a classe extende da classe `DbContext`, e também sobrescrevemos o método `OnConfiguring`. Dentro do método sobrescrito, definimos no objeto `optionsBuilder` a string de conexão com o SQL Server.

## Mapeando Artistas

Note também que a tabela do banco de dados chamada `Artistas` foi mapeada por meio da declaração da propriedade (com getter e setter) do tipo `DbSet<Artista>` (sendo que `Artista` é a classe de modelo criada anteriormente).

A classe `ArtistaDAL.cs` por ora vai ser reescrita para usar o Entity Framework e as operações CRUD no programa principal também serão removidas (com exceção de listar):

```Csharp
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
            using var context = new ScreenSoundContext();
            return context.Artistas.ToList<Artista>();
        }
    }
}
```
Note que primeiro é necessário instanciar o contexto, referenciar o `DbSet` desejado (no caso, `Artistas`) e em seguida usar o método `ToList<Entidade>` do DbSet Artistas para retornar a lista.

## Refatorando ArtistaDAL
O resultado da refatoração da classe ArtistaDAL será:

```Csharp
// Banco/ArtistaDAL
// Resto do código
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class ArtistaDAL
    {
        private static readonly ScreenSoundContext context = new ScreenSoundContext();

        public static IEnumerable<Artista> Listar()
        {
            return context.Artistas.ToList<Artista>();
        }

        public static void Adicionar (Artista artista)
        {
            context.Artistas.Add(artista);
            context.SaveChanges();
        }

        public static void Atualizar(Artista artista)
        {
            context.Artistas.Update(artista);
            context.SaveChanges();
        }
        public static void Deletar(Artista artista)
        {
            context.Artistas.Remove(artista);
            context.SaveChanges();
        }
    }
}
```
> Note que as operações que modificam o banco de dados (métodos `Add`, `Update` e `Remove` do DbSet`Artistas`) são seguidos pela confirmação da alteração (método `context.SaveChanges`). Note também que o objeto de contexto `ScreenSoundContext` pode ser reusado pelos métodos várias vezes, sem necessidade de destrui-lo e recriá-lo.

Seguem as modificações do programa principal:
```Csharp
// Program.cs

using ScreenSound.Banco;
using ScreenSound.Menus;
using ScreenSound.Modelos;


try
{
    // Teste da inserção
    string n1 = "U2";
    Artista novoArtista = new Artista("U2", $"Bio do {n1}");
    ArtistaDAL.Adicionar(novoArtista);

    // Teste da atualização    
    string n2 = "Pearl Jam";
    Artista artistaParaEditar = new Artista(n1, $"Bio de {n1}") { Id = 1002 };
    artistaParaEditar.Nome = n2;
    artistaParaEditar.Bio = $"Bio de {n2}";
    ArtistaDAL.Atualizar(artistaParaEditar);

    foreach (Artista artista in ArtistaDAL.Listar())
    {
        Console.WriteLine(artista);
    }

    // Teste da remoção
    ArtistaDAL.Deletar(artistaParaEditar);
    foreach (Artista artista in ArtistaDAL.Listar())
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
> Importante: o contexto NÃO permite operações sobres dois objetos diferentes com o mesmo número de ID.
