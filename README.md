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
