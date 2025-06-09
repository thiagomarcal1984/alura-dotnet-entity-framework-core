using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
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

        public override void Adicionar (Musica Musica)
        {
            context.Musicas.Add(Musica);
            context.SaveChanges();
        }

        public override void Atualizar(Musica Musica)
        {
            context.Musicas.Update(Musica);
            context.SaveChanges();
        }
        public override void Deletar(Musica Musica)
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
