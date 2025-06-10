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
        public MusicaDAL(ScreenSoundContext context): base(context) {}

        public Musica? ProcurarPeloNome(string nome)
        {
            return context.Musicas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
