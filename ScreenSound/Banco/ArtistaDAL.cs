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
            var lista = new List<Artista>();
            using var connection = Connection.ObterConexao();
            connection.Open();

            string sql = "SELECT * FROM Artistas";

            SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                string nomeArtista = Convert.ToString(dataReader["Nome"]);
                string bioArtista = Convert.ToString(dataReader["Bio"]);
                int idArtista = Convert.ToInt32(dataReader["id"]);
                Artista artista = new Artista(nomeArtista, bioArtista) { Id = idArtista };

                lista.Add(artista);
            }

            return lista;
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
