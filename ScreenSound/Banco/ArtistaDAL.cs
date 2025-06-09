using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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
