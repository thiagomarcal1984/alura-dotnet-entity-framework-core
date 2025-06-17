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

## Incluindo Artista
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

## Mão na massa: criando o método recuperar pelo nome
Código da classe `ArtistaDAL.cs`:
```Csharp
// Resto do código
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class ArtistaDAL
    {
        // Resto do código 
        public static Artista? ProcurarPeloNome(string nome)
        {
            return context.Artistas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
```
Note a interrogação após a declaração do tipo de retorno na assinatura (`Artista?`). Isso significa que o retorno eventualmente pode ser nulo.

O método `FirstOrDefault` recebe como parâmetro uma função que retorna um booleano. Essa função é usada para percorrer o banco e recuperar o primeiro registro que atender à função.

## Ajustando o menu
Os submenus (que herdam de uma classe de menu) da aplicação recebiam um dicionário de artistas ao invés da DAL de artistas. Então é necessário fazer uma refatoração do código.

Mas o código que fiz até agora usou métodos estáticos. Precisei modificar a classe `ArtistaDAL` para forçar a sua criação e posterior repasse para as subclasses de `Menu`:
```Csharp
// Banco/ArtistaDAL.cs
namespace ScreenSound.Banco
{
    internal class ArtistaDAL
    {
        // private static readonly ScreenSoundContext context = new ScreenSoundContext();
        private readonly ScreenSoundContext context = new ScreenSoundContext();

        // public static IEnumerable<Artista> Listar()
        public IEnumerable<Artista> Listar() 
        { 
            // Resto do código
        }

        // public static void Adicionar (Artista artista)
        public void Adicionar (Artista artista) 
        { 
            // Resto do código
        }

        // public static void Atualizar(Artista artista)
        public void Atualizar(Artista artista) 
        { 
            // Resto do código
        }
        // public static void Deletar(Artista artista)
        public void Deletar(Artista artista) 
        { 
            // Resto do código
        }

        // public static Artista? ProcurarPeloNome(string nome)
        public Artista? ProcurarPeloNome(string nome) 
        { 
            // Resto do código
        }
    }
}
```

A refatoração do código de `Menu` para atualização dos submenus pode ser feita no Visual Studio escolhendo a opção `Alterar assinatura...`. Ao usar essa opção, podemos acrescentar o tipo do parâmetro, nome do parâmetro da assinatura e o nome do objeto que será inserido nas chamadas do método refatorado. 

Após a refatoração, será necessário referenciar os métodos corretos de `ArtistaDAL`:

```Csharp
// Menus.Menu.cs
using ScreenSound.Banco;
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

internal class Menu
{
    // Resto do código
    public virtual void Executar(ArtistaDAL artistaDAL)
    {
        Console.Clear();
    }
}
```

```Csharp
// Menus/MenuMostrarArtista.cs
using ScreenSound.Banco; // Inserção da dependência
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

internal class MenuMostrarArtistas : Menu
{
    public override void Executar(ArtistaDAL artistaDAL) // Inserção da DAL
    {
        base.Executar(artistaDAL); // Inserção da DAL
        ExibirTituloDaOpcao("Exibindo todos os artistas registradas na nossa aplicação");

        foreach (Artista artista in artistaDAL.Listar()) // Aqui está a mudança
        {
            Console.WriteLine($"Artista: {artista.Nome}");
        }
        // Resto do código
    }
}

```
```Csharp
// Menus/MenuMostrarMusicas.cs
using ScreenSound.Banco; // Inserção da dependência
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

internal class MenuMostrarMusicas : Menu
{
    public override void Executar(ArtistaDAL artistaDAL) // Inserção da DAL
    {
        base.Executar(artistaDAL); // Inserção da DAL
        ExibirTituloDaOpcao("Exibir detalhes do artista");
        Console.Write("Digite o nome do artista que deseja conhecer melhor: ");
        string nomeDoArtista = Console.ReadLine()!;
        var artistaRecuperado = artistaDAL.ProcurarPeloNome(nomeDoArtista); // Aqui mudou.
        if (artistaRecuperado is not null) // Aqui também.
        {
            // Resto do código
        }
    }
}
```

```Csharp
// Menus/MenuRegistrarArtista.cs
using ScreenSound.Banco; // Inserção da dependência
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

internal class MenuRegistrarArtista : Menu
{
    public override void Executar(ArtistaDAL artistaDAL) // Inserção da DAL
    {
        base.Executar(artistaDAL); // Inserção da DAL
        ExibirTituloDaOpcao("Registro dos Artistas");
        Console.Write("Digite o nome do artista que deseja registrar: ");
        string nomeDoArtista = Console.ReadLine()!;
        Console.Write("Digite a bio do artista que deseja registrar: ");
        string bioDoArtista = Console.ReadLine()!;
        Artista artista = new Artista(nomeDoArtista, bioDoArtista);
        artistaDAL.Adicionar(artista); // Aqui mudou.
        Console.WriteLine($"O artista {nomeDoArtista} foi registrado com sucesso!");
        Thread.Sleep(4000);
        Console.Clear();
    }
}
```

```Csharp
// Menus/MenuRegistrarMusica.cs
using ScreenSound.Banco; // Inserção da dependência
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

internal class MenuRegistrarMusica : Menu
{
    public override void Executar(ArtistaDAL artistaDAL) // Inserção da DAL
    {
        base.Executar(artistaDAL); // Inserção da DAL
        ExibirTituloDaOpcao("Registro de músicas");
        Console.Write("Digite o artista cuja música deseja registrar: ");
        string nomeDoArtista = Console.ReadLine()!;
        var artistaRecuperado = artistaDAL.ProcurarPeloNome(nomeDoArtista); // Aqui mudou
        if (artistaRecuperado is not null) // Aqui também.
        {
            // Resto do código
        }
    }
}
```

```Csharp
// Menus/MenuSair.cs
using ScreenSound.Banco; // Inserção da dependência.
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

internal class MenuSair : Menu
{
    public override void Executar(ArtistaDAL artistaDAL) // Inserção da DAL... mas não é usada.
    {
        Console.WriteLine("Tchau tchau :)");
    }
}
```

Agora, o programa principal vai repassar a `ArtistaDAL` para os menus:

```Csharp
// Program.cs
// Resto do código

var artistaDAL = new ArtistaDAL();

// Resto do código

void ExibirOpcoesDoMenu()
{
    // Resto do código
    if (opcoes.ContainsKey(opcaoEscolhidaNumerica))
    {
        Menu menuASerExibido = opcoes[opcaoEscolhidaNumerica];
        menuASerExibido.Executar(artistaDAL); // Aqui mudou.
        // Resto do código
    }
}

// Resto do código
```
## Mão na massa: persistindo músicas

Script para criação da tabela Musicas:
```SQL
create table Musicas(
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nome NVARCHAR(255) NOT NULL
);
```

Atualização do código do `ScreenSoundContext.cs`:
```Csharp
// Banco/ScreenSoundContext.cs
using Microsoft.EntityFrameworkCore;
using ScreenSound.Modelos;
// Resto do código

namespace ScreenSound.Banco;

internal class ScreenSoundContext : DbContext
{
    public DbSet<Artista> Artistas { get; set; }
    public DbSet<Musica> Musicas { get; set; }

    private static string connectionString = "...string_de_conexao...";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }
}
```
Criação da camada `MusicaDAL.cs`:
```Csharp
// MusicaDAL.cs
// Resto do código
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class MusicaDAL
    {
        private readonly ScreenSoundContext context = new ScreenSoundContext();

        public IEnumerable<Musica> Listar()
        {
            return context.Musicas.ToList<Musica>();
        }

        public void Adicionar (Musica Musica)
        {
            context.Musicas.Add(Musica);
            context.SaveChanges();
        }

        public void Atualizar(Musica Musica)
        {
            context.Musicas.Update(Musica);
            context.SaveChanges();
        }
        public void Deletar(Musica Musica)
        {
            context.Musicas.Remove(Musica);
            context.SaveChanges();
        }

        public Musica? ProcurarPeloNome(string nome)
        {
            return context.Musicas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
```
> Note que a lógica é a mesma de `ArtistaDAL`. O que muda é a referência à tabela (DBSet).

# Generics
## Utilizando Generics
Vamos criar uma classe abstrata chamada `DAL`. Ela vai impor os CRUDs sobre as demais DALs.

```Csharp
// Banco/DAL.cs
// Imports

namespace ScreenSound.Banco
{
    internal abstract class DAL<T>
    {
        public abstract IEnumerable<T> Listar();
        public abstract void Adicionar(T objeto);
        public abstract void Atualizar(T objeto);
        public abstract void Deletar(T objeto);
    }
}
```
> Note a letra `T` dentro do diamante na declaração da classe abstrata. Essa letra T (que na verdade pode ser qualquer outra) indica o tipo genérico que será substituído pelo tipo específico ao instanciar um objeto de uma subclasse de DAL.

Agora, a classe `ArtistaDAL` será modificada para herdar de `DAL`:
```Csharp
// Outros imports
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class ArtistaDAL: DAL<Artista>
    {
        private readonly ScreenSoundContext context = new ScreenSoundContext();

        public override IEnumerable<Artista> Listar()
        {
            return context.Artistas.ToList();
        }

        public override void Adicionar (Artista artista)
        {
            context.Artistas.Add(artista);
            context.SaveChanges();
        }

        public override void Atualizar(Artista artista)
        {
            context.Artistas.Update(artista);
            context.SaveChanges();
        }
        public override void Deletar(Artista artista)
        {
            context.Artistas.Remove(artista);
            context.SaveChanges();
        }

        public Artista? ProcurarPeloNome(string nome)
        {
            return context.Artistas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
```
Note que todos os métodos herdados de `DAL` usam a palavra chave `override` para sobrescrever de fato os métodos abstratos. Note também a sintaxe para herança no C# (`internal class ArtistaDAL: DAL<Artista>`).

## Mão na massa: MusicaDAL com genérico
```Csharp
// Banco/MusicaDAL.cs
// Resto dos imports
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class MusicaDAL: DAL<Musica>
    {
        private readonly ScreenSoundContext context = new ScreenSoundContext();

        public override IEnumerable<Musica> Listar()
        {
            return context.Musicas.ToList();
        }

        // O restante dos métodos segue o mesmo padrão de ArtistaDAL
    }
}
```
## Implementando métodos genéricos
A classe `DAL` vai concentrar os métodos genéricos das operações CRUD. Vamos criar nela um construtor com acesso `protected` e implementar os métodos:

```Csharp
// Banco/DAL.cs
// Os imports foram omitidos.

namespace ScreenSound.Banco
{
    internal abstract class DAL<T> where T : class
    {
        protected readonly ScreenSoundContext context;
        protected DAL(ScreenSoundContext context){
            this.context = context;
        }
        public IEnumerable<T> Listar()
        {
            return context.Set<T>().ToList();
        }
        public void Adicionar(T objeto)
        {
            context.Set<T>().Add(objeto);
            context.SaveChanges();
        }

        public void Atualizar(T objeto)
        {
            context.Set<T>().Update(objeto);
            context.SaveChanges();
        }

        public void Deletar(T objeto)
        {
            context.Set<T>().Remove(objeto);
            context.SaveChanges();
        }
    }
}
```
> Note o texto `where T : class` após `internal abstract class DAL<T>` na declaração da classe: `where` impõe uma restrição aos tipos de dados do tipo T. Uma possibilidade é limitar o tipo com `where` usando uma superclasse de modelo (no exemplo, estamos usando apenas `class` mesmo ao invés da superclasse de modelo).

Adaptação das subclasses de DAL (`ArtistaDAL` e `MusicaDAL`):
```Csharp
// Banco/ArtistaDAL.cs
// Resto dos imports
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class ArtistaDAL: DAL<Artista>
    {
        public ArtistaDAL(ScreenSoundContext context): base(context) {}

        public Artista? ProcurarPeloNome(string nome)
        {
            return context.Artistas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
```
> Note o código `: base(context) {}`: ele representa a invocação do construtor da superclasse `DAL`, e o parâmetro context é fornecido para esse construtor. O construtor local está vazio (note as chaves `{}`).
> O método `ProcurarPeloNome` não é herdado, portanto foi necessário implementá-lo aqui.

O mesmo para `MusicaDAL`:
```Csharp
// Banco/MusicaDAL.cs
// Resto dos imports
using ScreenSound.Modelos;

namespace ScreenSound.Banco
{
    internal class MusicaDAL: DAL<Musica>
    {
        public MusicaDAL(ScreenSoundContext context): base(context) {}

        public Musica? ProcurarPeloNome(string nome)
        {
            return context.Musicas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
```
## Utilizando Func
A classe `DAL` não será mais abstrata, uma vez que as demais classes DAL (`ArtistaDAL` e `MusicaDAL`) serão removidas.
```Csharp
// Banco/DAL.cs
// Imports omitidos

namespace ScreenSound.Banco
{
    internal class DAL<T> where T : class
    {
        protected readonly ScreenSoundContext context;
        public DAL(ScreenSoundContext context){
            this.context = context;
        }

        // Códigos de CRUD omitidos


        public T? RecuperarPor(Func<T, bool> condicao)
        {
            return context.Set<T>().FirstOrDefault(condicao);
        }
    }
}
```
> Note o parâmetro `Func<T, bool> condicao` do novo método `RecuperarPor`. É um jeito de generalizar a busca a partir de uma função. Neste exemplo, a função batizada de `condicao` usa um objeto do tipo `T` e retorna um `bool`.

## Mão na massa: atualizando os menus
O método `Executar` da classe `Menus/Menu` precisa ter seu parâmetro de execução mudado. Antes era `ArtistaDAL` (que não existe mais). Agora usaremos o a DAL genérica e usaremos a classe Artista como critério.

Código da superclasse `Menus/Menu`:
```Csharp
// Menus/Menu.cs
// Imports omitidos
internal class Menu
{
    // Resto do código
    public virtual void Executar(DAL<Artista> artistaDAL)
    {
        // Resto do código
    }
}
```
Vamos exemplificar o uso da nova assinatura do método `Executar` somente na classe `MenuMostrarMusica`:
```Csharp
// Menus/MenuMostrarMusica.cs
// Importos omitidos.
namespace ScreenSound.Menus;

internal class MenuMostrarMusicas : Menu
{
    public override void Executar(DAL<Artista> artistaDAL)
    {
        base.Executar(artistaDAL);
        ExibirTituloDaOpcao("Exibir detalhes do artista");
        Console.Write("Digite o nome do artista que deseja conhecer melhor: ");
        string nomeDoArtista = Console.ReadLine()!;
        var artistaRecuperado = artistaDAL.RecuperarPor(a => a.Nome == nomeDoArtista);
        // Resto do código
    }
}
```

> Note a sintaxe da condição fornecida para o método `RecuperarPor`:
> ```Csharp
>      // Código em Banco/DAL.cs
>      public T? RecuperarPor(Func<T, bool> condicao)
>      {
>          return context.Set<T>().FirstOrDefault(condicao);
>      }
> ```
>
> A função fornecida usa um objeto de tipo `T` e na chamada do método foi batizada de `a`. Em seguida, acrescentamos uma seta (`=>`). Finalmente colocamos o corpo/retorno da função (no caso, o retorno da função é do tipo `bool`, conforme assinatura do método `RecuperarPor`). O teste booleano aplicado é `a.Nome == nomeDoArtista`. Na primeira vez em que o teste for verdadeiro, o objeto testado `T` é retornado.

Finalmente, vamos modificar o programa principal para fornecer o `DAL` correto:
```Csharp
// Program.cs
// Resto do código
var artistaDAL = new DAL<Artista>(new ScreenSoundContext());

void ExibirOpcoesDoMenu()
{
    // Resto do código
    if (opcoes.ContainsKey(opcaoEscolhidaNumerica))
    {
        Menu menuASerExibido = opcoes[opcaoEscolhidaNumerica];
        menuASerExibido.Executar(artistaDAL); // Fornece o DAL para o menu selecionado.
        // Resto do código
    }
}
```
# Migrations
## Utilizando Migrations
Precisaremos instalar duas dependências com o NuGet  (neste projeto a versão de ambas é 7.0.14): 
1. Microsoft.EntityFrameworkCore.Design
2. Microsoft.EntityFrameworkCore.Tools

Conteúdo do arquivo `ScreenSound.csproj` após a instalação das dependências:
```XML
<Project Sdk="Microsoft.NET.Sdk">

  <!-- Resto do código -->

  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
    <!-- Nova dependência: Design -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="7.0.14">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.14" />
    <!-- Nova dependência: Tools -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.14">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

Agora, vamos usar a CLI do NuGet: `Ferramentas -> Gerenciador de pacotes do NuGet -> Console do Gerenciador de Pacotes.`

> De uma forma estranha, a CLI do NuGet não fica separada do Visual Studio. 

Na Console do Gerenciador de Pacotes, executamos o comando `Add-Migration {nome da migration}` para criarmos uma migration. Veja a saída do comando::
```bash
Cada pacote é licenciado para você por seu proprietário. A NuGet não é 
responsável por pacotes de terceiros nem concede licenças a eles. Alguns 
pacotes podem incluir dependências que são administradas por licenças 
adicionais. Siga a URL da origem (feed) do pacote para determinar todas 
as dependências.

Versão 6.14.0.116 do Host do Console do Gerenciador de Pacotes

Digite 'get-help NuGet' para ver todos os comandos disponíveis do NuGet.

PM> Add-Migration projetoInicial
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
PM> 
```
Depois deste comando, o diretório `Migrations` será criado e dentro dele os arquivos necessários para executar as migrations também serão criados.

## Update-Database
Vamos estudar os arquivos gerados ao adicionar a migration:

O arquivo`{timestamp}_{nome da migration}.cs` (e o seu complemento `{timestamp}_{nome da migration}.Designer.cs`) declaram uma classe que extende de Migration. Esta classe tem basicamente dois métodos: `Up` e `Down`. Eles servem para aplicar e desaplicar a migration.

Geralmente a aplicação das migrations é feita num banco de dados em branco. Então, vamos mudar a string de conexão para usar um banco de dados com o nome `ScreenSoundV0`. Ao mudar a string de conexão, esse banco de dados será criado e nele as migrations serão aplicadas.

```Csharp
// Imports

namespace ScreenSound.Banco;

internal class ScreenSoundContext : DbContext
{
    // Resto do código
    private static string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;" + 
        "Initial Catalog=ScreenSoundV0;Integrated Security=True;Encrypt=False;" + 
        "Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

    // Resto do código
}
```
Para aplicar as migrations, execute o comando `Update-Database {nome da migration}` no Console do Gerenciador de Pacotes:
```bash
PM> Update-Database projetoInicial
Build started...
Build succeeded.
Applying migration '20250616223713_projetoInicial'.
Done.
PM> 
```
Após a execução deste comando, o banco de dados `ScreenSoundV0` será criado e nele as tabelas preparadas na migration de nome `projetoInicial`. Um outra tabela também será criada: a tabela `dbo.__EFMigrationsHistory`. Ela vai armazenar a identificação dos arquivos das migrations (e a versão do Entity Framework usada).

## Inserindo dados
Primeiro, vamos acrescentar uma nova migration chamada `PopularTabela` usando o Console do Gerenciador de Pacotes:
```bash
PM> Add-Migration PopularTabela
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
PM> 
```
O(s) arquivo(s) da nova migration vai implementar os métodos `Up` e `Down`, mas em branco. O preenchimento deles será algo parecido com o código a seguir:
```Csharp
// {timestamp}_PopularTabela.cs
namespace ScreenSound.Migrations
{
    /// <inheritdoc />
    public partial class PopularTabela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O método InsertData recebe o nome da tabela, o nome das colunas e os valores para 
            // a linha que está sendo acrescentada.
            migrationBuilder.InsertData(
                "Artistas", 
                new string[] {
                    "Nome", "Bio", "FotoPerfil"
                }, 
                new object[] {
                    "Djavan", "Bio do Djavan", 
                    "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png"
                }
            );
            // Outras inserções usando o migrationBuilder.
            migrationBuilder.InsertData(nomeTabela, arrayColunas, arrayValores);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Execução de SQL cru para apagar os registros da tabela.
            migrationBuilder.Sql("DELETE FROM Artistas");
        }
    }
}
```
> O método `InsertData` do objeto `migrationBuilder` recebe o nome da tabela, o nome das colunas e os valores para a linha que está sendo acrescentada. Já o método `Sql` do objeto `migrationBuilder` executa um script SQL cru.

Vamos agora executar a atualização do banco aplicando a migration com o comando `Update-Table` do Console do Gerenciador de Pacotes
```bash
PM> Update-Database
Build started...
Build succeeded.
Applying migration '20250616230436_PopularTabela'.
Done.
PM> 
```
> Note que o comando `Update-Database` não precisa de parâmetros. Na primeira aplicação da migration, especificamos o nome da migration, mas isso não é obrigatório.

## Adicionando uma nova coluna
Vamos acrescentar a propriedade `AnoLancamento` na classe de modelo `Musica`:
```Csharp
// Modelos/Musica.cs
namespace ScreenSound.Modelos;

internal class Musica
{
    // Resto do código
    public int? AnoLancamento { get; set; }
    // Resto do código
}
```
Depois de acrescentarmos a coluna, vamos usar o Console do Gerenciador de Pacotes e criar uma nova migration chamada `AdicionarColunaAnoLancamento`:
```bash
PM> Add-Migration AdicionarColunaAnoLancamento
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
PM> 
```

O código resultante da migration será:

```Csharp
// {timestamp}_AdicionarColunaAnoLancamento.cs
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenSound.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarColunaAnoLancamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnoLancamento",
                table: "Musicas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnoLancamento",
                table: "Musicas");
        }
    }
}
```
O arquivo `{nome do contexto do banco}ModelSnapshot.cs` será modificado para refletir o estado atual das entidades do contexto.
```Csharp 
// Migrations/{nome do contexto do banco}ModelSnapshot.cs
// <auto-generated />
// Imports

#nullable disable

namespace ScreenSound.Migrations
{
    [DbContext(typeof(ScreenSoundContext))]
    partial class ScreenSoundContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ScreenSound.Modelos.Artista", b =>
                {
                    // Declaração das propriedades de Artista.
                    b.ToTable("Artistas");
                });

            modelBuilder.Entity("ScreenSound.Modelos.Musica", b =>
                {
                    // Declaração das propriedades anteriores de Musica.
                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    // Resto do código

                    b.ToTable("Musicas");
                });
#pragma warning restore 612, 618
        }
    }
}
``` 
Uma vez criada a migration, vamos aplicá-la com o comando `Update-Database` do Console do Gerenciador de Pacotes:
```bash
PM> Update-Database
Build started...
Build succeeded.
Applying migration '20250616232809_AdicionarColunaAnoLancamento'.
Done.
PM> 
```
## Mão na massa: adicionando músicas na tabela
Criação da migration `PopularMusicas` no Console do Gerenciador de Pacotes:
```bash
PM> Add-Migration PopularMusicas
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
PM> 
```

Edição da migration:
```Csharp
// Migrations/{timestamp}_PopularMusicas.cs
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenSound.Migrations
{
    /// <inheritdoc />
    public partial class PopularMusicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                "Musicas", 
                new string[] { "Nome", "AnoLancamento" }, 
                new object[] { "Oceano", 1989 }
            );
            migrationBuilder.InsertData(
                "Musicas", 
                new string[] { "Nome", "AnoLancamento" }, 
                new object[] { "Flor de Lis", 1976 }
            );
            migrationBuilder.InsertData(
                "Musicas", 
                new string[] { "Nome", "AnoLancamento" }, 
                new object[] { "Samurai", 1982 }
            );
            migrationBuilder.InsertData(
                "Musicas", 
                new string[] { "Nome", "AnoLancamento" }, 
                new object[] { "Se", 1992 }
            );
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Musicas");
        }
    }
}
```
Aplicação da migration no Console do Gerenciador de Pacotes:
```bash
PM> Update-Database
Build started...
Build succeeded.
Applying migration '20250616234008_PopularMusicas'.
Done.
PM> 
```
# Relacionamento
## Relacionando artista a música
Faremos alguns pequenos ajustes nas classes de modelo antes de criarmos a migration que faz o relacionamento entre Artista e Música: 

```Csharp
// Modelos/Musica.cs
namespace ScreenSound.Modelos;

internal class Musica
{
    // Resto do código.
    public Artista? Artista { get; set; } // Não havia a declaração do artista.
    // Resto do código.
}
```

```Csharp
// Modelos/Artista.cs
namespace ScreenSound.Modelos; 

internal class Artista 
{
    public ICollection<Musica> Musicas { get; set; }

    public Artista(string nome, string bio)
    {
        Nome = nome;
        Bio = bio;
        FotoPerfil = "https://cdn.pixabay.com/photo/2016/08/08/09/17/avatar-1577909_1280.png";
    }

    public string Nome { get; set; }
    public string FotoPerfil { get; set; }
    public string Bio { get; set; }
    public int Id { get; set; }

    public void AdicionarMusica(Musica musica)
    {
        Musicas.Add(musica);
    }

    public void ExibirDiscografia()
    {
        Console.WriteLine($"Discografia do artista {Nome}");
        foreach (var musica in Musicas)
        {
            Console.WriteLine($"Música: {musica.Nome}");
        }
    }

    public override string ToString()
    {
        return $@"Id: {Id}
            Nome: {Nome}
            Foto de Perfil: {FotoPerfil}
            Bio: {Bio}";
    }
}
```
Vamos criar a migration com o comando `Add-Migration` no Console do Gerenciador de Pacotes:
```bash
PM> Add-Migration RelacionarArtistaMusica
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
PM> 
```

Código resultante da criação da migration RelacionarArtistaMusica:
```Csharp
// Migrations/{timestamp}_RelacionarArtistaMusica.cs
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenSound.Migrations
{
    /// <inheritdoc />
    public partial class RelacionarArtistaMusica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArtistaId",
                table: "Musicas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Musicas_ArtistaId",
                table: "Musicas",
                column: "ArtistaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Musicas_Artistas_ArtistaId",
                table: "Musicas",
                column: "ArtistaId",
                principalTable: "Artistas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Musicas_Artistas_ArtistaId",
                table: "Musicas");

            migrationBuilder.DropIndex(
                name: "IX_Musicas_ArtistaId",
                table: "Musicas");

            migrationBuilder.DropColumn(
                name: "ArtistaId",
                table: "Musicas");
        }
    }
}
```

Finalmente, vamos aplicar a migration com o comando `Update-Database`:
```bash
PM> Update-Database
Build started...
Build succeeded.
Applying migration '20250617010721_RelacionarArtistaMusica'.
Done.
PM> 
```
