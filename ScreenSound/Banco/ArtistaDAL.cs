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
