﻿using Microsoft.Data.SqlClient;
using ScreenSound.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenSound.Banco;

internal class Connection 
{
	private static string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ScreenSound;Integrated Security=True;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
	public static SqlConnection ObterConexao() 
	{
		return new SqlConnection(connectionString);
	}

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
