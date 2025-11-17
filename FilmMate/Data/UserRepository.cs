using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // <-- DODATO ZBOG FILE OPERACIJA

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
            // Očistiti listu prije ponovnog učitavanja, iako je nova instanca
            korisnici.Clear();

            if (!File.Exists(filePath)) return;

            // File.ReadAllLines se automatski zatvara i oslobađa handle
            foreach (var line in File.ReadAllLines(filePath))
            {
                var p = line.Split(';');
                if (p.Length >= 3)
                {
                    string tip = p[2].Trim().ToLower(); // Dodato Trim/ToLower za sigurnost
                    Korisnik k = (tip == "admin") ? new Administrator() : new Gledalac();
                    k.setKorisnickoIme(p[0].Trim());
                    k.setLozinka(p[1].Trim());
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
            // File.WriteAllLines se automatski zatvara i oslobađa handle
            File.WriteAllLines(filePath, lines);
        }

        public List<Korisnik> GetAll() => korisnici;
    }
}