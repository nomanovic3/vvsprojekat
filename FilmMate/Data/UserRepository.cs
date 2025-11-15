using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FilmMate.Models;

namespace FilmMate.Data
{
    public class UserRepository
    {
        private string filePath = "korisnici.txt";
        private List<Korisnik> korisnici = new List<Korisnik>();

        public UserRepository() => Ucitaj();

        private void Ucitaj()
        {
            if (!File.Exists(filePath)) return;
            foreach (var line in File.ReadAllLines(filePath))
            {
                var p = line.Split(';');
                if (p.Length >= 3)
                {
                    string tip = p[2];
                    Korisnik k = (tip == "admin") ? new Administrator() : new Gledalac();
                    k.setKorisnickoIme(p[0]);
                    k.setLozinka(p[1]);
                    korisnici.Add(k);
                }
            }
        }

        public void Sacuvaj()
        {
            var lines = new List<string>();
            foreach (var k in korisnici)
            {
                string tip = (k is Administrator) ? "admin" : "user";
                lines.Add($"{k.getKorisnickoIme()};{k.getLozinka()};{tip}");
            }
            File.WriteAllLines(filePath, lines);
        }

        public List<Korisnik> GetAll() => korisnici;
    }
}

