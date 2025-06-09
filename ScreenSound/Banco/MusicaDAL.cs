using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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
