using ScreenSound.Banco;
using ScreenSound.Modelos;

namespace ScreenSound.Menus;

internal class MenuMostrarMusicasPorAno : Menu
{
    public override void Executar(DAL<Artista> artistaDAL)
    {
        base.Executar(artistaDAL);
        ExibirTituloDaOpcao("Músicas por ano");
        Console.Write("Digite o ano em que a música foi publicada: ");
        string ano = Console.ReadLine()!;
        DAL<Musica> musicaDAL = new DAL<Musica>(new ScreenSoundContext());
        var musicas = musicaDAL.RecuperarListaPor(m => m.AnoLancamento == Convert.ToInt32(ano)).ToList();
        if (musicas.Any())
        {
            Console.WriteLine($"\nMúsicas do ano de {ano}:");
            foreach (var musica in musicas)
            {
                musica.ExibirFichaTecnica();
            }
        }
        else
        {
            Console.WriteLine($"\nNão foram encontradas músicas publicadas no ano de {ano}.");
        }
        Console.WriteLine("Digite uma tecla para voltar ao menu principal");
        Console.ReadKey();
        Console.Clear();
    }
}
