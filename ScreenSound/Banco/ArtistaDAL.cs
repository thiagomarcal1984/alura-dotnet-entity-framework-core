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
        public ArtistaDAL(ScreenSoundContext context): base(context) {}

        public Artista? ProcurarPeloNome(string nome)
        {
            return context.Artistas.FirstOrDefault(a => a.Nome.Equals(nome));
        }
    }
}
