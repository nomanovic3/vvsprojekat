using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FilmMate.Models;

namespace FilmMate.Data
{
    public class FilmRepository : IFilmRepository
    {
        private string filePath = "filmovi.txt";
        private List<Film> filmovi = new List<Film>();

        public FilmRepository() => UcitajFilmove();

        private void UcitajFilmove()
        {
            if (!File.Exists(filePath)) return;
            foreach (var line in File.ReadAllLines(filePath))
            {
                var p = line.Split(';');
                if (p.Length == 4)
                    filmovi.Add(new Film(p[0], p[1], double.Parse(p[2]), int.Parse(p[3])));
            }
        }

        public void Sacuvaj()
        {
            var lines = new List<string>();
            foreach (var f in filmovi)
                lines.Add($"{f.getNazivFilma()};{f.getKategorija()};{f.getOcjena()};{f.getGodina()}");
            File.WriteAllLines(filePath, lines);
        }

        public List<Film> GetAll() => filmovi;
    }
}

