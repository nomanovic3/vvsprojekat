using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmMate.Models
{
    public class Film
    {
        private string nazivFilma;
        private string kategorija;
        private List<int> ocjene; // Lista ocjena za izračun prosjeka
        private int godinaIzlaska;

        public Film(string naziv, string kat, double pocetnaOcjena, int god)
        {
            nazivFilma = naziv;
            kategorija = kat;
            godinaIzlaska = god;
            ocjene = new List<int>();

            
            if (pocetnaOcjena >= 1 && pocetnaOcjena <= 10)
            {
                ocjene.Add((int)Math.Round(pocetnaOcjena));
            }
        }

        // GET METODE
        public string getNazivFilma() => nazivFilma;
        public string getKategorija() => kategorija;
        public int getGodina() => godinaIzlaska;

       
        public List<int> getOcjene() => ocjene;

        
        public double getOcjena()
        {
            if (ocjene.Any())
            {
                return ocjene.Average();
            }
            return 0.0;
        }

      
        public void setNaziv(string n) => nazivFilma = n;
        public void setKategorija(string k) => kategorija = k;
       

        public void setGodina(int g) => godinaIzlaska = g;

        
        public void DodajOcjenu(int ocjena)
        {
            if (ocjena >= 1 && ocjena <= 10)
            {
                ocjene.Add(ocjena);
            }
        }

        public override string ToString()
        {
            
            return $"{nazivFilma} | {kategorija} | {godinaIzlaska} | Prosjek: {getOcjena():F2} ({ocjene.Count} ocjena)";
        }
     
    }
}