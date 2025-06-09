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

        public static Artista? ProcurarPeloNome(string nome)
        {
            return context.Artistas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
