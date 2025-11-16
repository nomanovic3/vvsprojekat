using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FilmMate.Models;
using FilmMate.Data;

namespace FilmMate.Services
{
    public class FilmService
    {
        private IFilmRepository repo;

        
        public FilmService(IFilmRepository filmRepo)
        {
            repo = filmRepo;
        }

        public void dodajFilm()
        {
            Console.Write("Naziv: ");
            string naziv = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(naziv) || naziv.Length < 2)
            {
                Console.WriteLine("Naziv mora imati bar 2 karaktera!");
                return;
            }

            bool postoji = false;
            foreach (var f in repo.GetAll())
            {
                if (f.getNazivFilma().ToLower() == naziv.ToLower())
                {
                    postoji = true;
                    break;
                }
            }

            if (postoji)
            {
                Console.WriteLine("Film već postoji!");
                return;
            }

            Console.Write("Kategorija: ");
            string kat = Console.ReadLine() ?? "";

            Console.Write("Ocjena (1-10): ");
            double ocjena = 0;
            bool validOcjena = double.TryParse(Console.ReadLine(), out ocjena);

            if (validOcjena)
            {
                if (ocjena < 1 || ocjena > 10)
                {
                    ocjena = 5;
                    Console.WriteLine("Neispravna ocjena, postavljena na 5.");
                }
            }
            else
            {
                ocjena = 5;
                Console.WriteLine("Neispravan format ocjene, postavljena na 5.");
            }

            Console.Write("Godina: ");
            int godina = int.TryParse(Console.ReadLine(), out godina) ? godina : 0;

            repo.GetAll().Add(new Film(naziv, kat, ocjena, godina));
            repo.Sacuvaj();
            Console.WriteLine("Film uspješno dodat!");
        }

        public void obrisiFilm()
        {
            Console.Write("Unesite naziv filma: ");
            string naziv = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(naziv))
            {
                Console.WriteLine("Naziv filma ne može biti prazan.");
                return;
            }

            Film filmZaBrisanje = null;
            bool pronadjen = false;
            foreach (var f in repo.GetAll())
            {
                if (f.getNazivFilma().ToLower() == naziv.ToLower())
                {
                    filmZaBrisanje = f;
                    pronadjen = true;
                    break;
                }
            }

            if (pronadjen)
            {
                bool imaOcjene = filmZaBrisanje.getOcjene().Count > 0;

                string upozorenje = imaOcjene
                                  ? $"PAŽNJA: Film '{filmZaBrisanje.getNazivFilma()}' ima {filmZaBrisanje.getOcjene().Count} ocjena. "
                                  : "";

                Console.WriteLine($"Jeste li sigurni da želite obrisati {filmZaBrisanje.getNazivFilma()}? {upozorenje}(D/N)");
                string potvrda = Console.ReadLine()?.ToLower() ?? "";

                if (potvrda == "d")
                {
                    if (imaOcjene)
                    {
                        Console.Write("Brisanjem se gube sve ocjene! Unesite 'OBRISI' za finalnu potvrdu: ");
                        string finalnaPotvrda = Console.ReadLine() ?? "";

                        if (finalnaPotvrda == "OBRISI")
                        {
                            repo.GetAll().Remove(filmZaBrisanje);
                            repo.Sacuvaj();
                            Console.WriteLine("Film obrisan!");
                        }
                        else
                        {
                            Console.WriteLine("Brisanje otkazano zbog neuspjele finalne potvrde.");
                        }
                    }
                    else
                    {
                        repo.GetAll().Remove(filmZaBrisanje);
                        repo.Sacuvaj();
                        Console.WriteLine("Film obrisan!");
                    }
                }
                else
                {
                    Console.WriteLine("Brisanje otkazano.");
                }
            }
            else
            {
                Console.WriteLine("Film nije pronađen!");
            }
        }

        public void azurirajFilm()
        {
            Console.Write("Unesite naziv filma: ");
            string naziv = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(naziv))
            {
                Console.WriteLine("Naziv filma ne može biti prazan.");
                return;
            }

            Film filmZaAzuriranje = null;
            foreach (var f in repo.GetAll())
            {
                if (f.getNazivFilma().ToLower() == naziv.ToLower())
                {
                    filmZaAzuriranje = f;
                    break;
                }
            }

            if (filmZaAzuriranje == null)
            {
                Console.WriteLine("Film nije pronađen!");
                return;
            }

            izvrsiAzuriranje(filmZaAzuriranje);

            if (repo != null)
            {
                repo.Sacuvaj();
                Console.WriteLine("Film ažuriran!");
            }
            else
            {
                Console.WriteLine("Greška: Repozitorij nedostupan za čuvanje.");
            }
        }

        private void izvrsiAzuriranje(Film film)
        {
            Console.WriteLine("1. Naziv 2. Kategorija 3. Godina");
            string izbor = Console.ReadLine() ?? "";

            switch (izbor)
            {
                case "1":
                    Console.Write("Novi naziv: ");
                    string noviNaziv = Console.ReadLine() ?? "";
                    if (!string.IsNullOrWhiteSpace(noviNaziv))
                        film.setNaziv(noviNaziv);
                    else
                        Console.WriteLine("Naziv ne može biti prazan.");
                    break;
                case "2":
                    Console.Write("Nova kategorija: ");
                    string novaKategorija = Console.ReadLine() ?? "";
                    if (!string.IsNullOrWhiteSpace(novaKategorija))
                        film.setKategorija(novaKategorija);
                    break;
                case "3":
                    Console.Write("Nova godina: ");
                    if (int.TryParse(Console.ReadLine(), out int god))
                    {
                        if (god > 1900 && god <= DateTime.Now.Year)
                            film.setGodina(god);
                        else
                            Console.WriteLine("Neispravan raspon godine.");
                    }
                    else
                        Console.WriteLine("Neispravan format.");
                    break;
                default:
                    Console.WriteLine("Pogrešan unos!");
                    break;
            }
        }

        public void OcijeniFilmGledaoca()
        {
            if (repo.GetAll().Count == 0)
            {
                Console.WriteLine("Nema filmova za ocjenjivanje.");
                return;
            }

            Console.Write("Unesite naziv filma koji želite ocijeniti: ");
            string naziv = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(naziv))
            {
                Console.WriteLine("Naziv filma ne može biti prazan.");
                return;
            }

            Film filmZaOcjenjivanje = null;
            foreach (var f in repo.GetAll())
            {
                if (f.getNazivFilma().ToLower() == naziv.ToLower())
                {
                    filmZaOcjenjivanje = f;
                    break;
                }
            }

            if (filmZaOcjenjivanje == null)
            {
                Console.WriteLine("Film nije pronađen!");
                return;
            }

            Console.Write($"Unesite ocjenu za '{filmZaOcjenjivanje.getNazivFilma()}' (1-10): ");
            int ocjena = 0;

            if (int.TryParse(Console.ReadLine(), out ocjena))
            {
                if (ocjena >= 1 && ocjena <= 10)
                {
                    filmZaOcjenjivanje.DodajOcjenu(ocjena);
                    repo.Sacuvaj();
                    Console.WriteLine($"Uspješno ste ocijenili film. Nova prosječna ocjena je {filmZaOcjenjivanje.getOcjena():F2}.");
                }
                else
                {
                    Console.WriteLine("Ocjena mora biti u rasponu od 1 do 10.");
                }
            }
            else
            {
                Console.WriteLine("Neispravan unos za ocjenu.");
            }
        }

        public virtual void FiltrirajPretraziFilmove()
        {
            if (repo.GetAll().Count == 0)
            {
                Console.WriteLine("Nema filmova za pretragu!");
                return;
            }

            Console.WriteLine("\n--- Filtriranje i Pretraga Filmova ---");
            Console.WriteLine("1. Pretraga po Nazivu");
            Console.WriteLine("2. Filtriranje po Kategoriji");
            Console.WriteLine("3. Filtriranje po Minimalnoj Ocjeni");
            Console.WriteLine("0. Povratak na Meni");
            Console.Write("Odabir: ");
            string izbor = Console.ReadLine() ?? "";

            List<Film> rezultati = new List<Film>();

            switch (izbor)
            {
                case "1": rezultati = filtrirajPoNazivu(); break;
                case "2": rezultati = filtrirajPoKategoriji(); break;
                case "3": rezultati = filtrirajPoMinimalnojOcjeni(); break;
                case "0": return;
                default:
                    Console.WriteLine("Pogrešan odabir.");
                    if (string.IsNullOrWhiteSpace(izbor)) Console.WriteLine("Unos ne smije biti prazan.");
                    return;
            }

            if (rezultati.Any())
            {
                if (rezultati.Count > 10)
                {
                    PrikaziListuFilmova(rezultati.Take(10).ToList(), $"Pronađeno 10 od {rezultati.Count}");
                }
                else
                {
                    PrikaziListuFilmova(rezultati, "Pronađeno");
                }
            }
            else
            {
                Console.WriteLine("Nema pronađenih filmova.");
            }
        }

        private List<Film> filtrirajPoNazivu()
        {
            Console.Write("Unesite dio naziva filma za pretragu: ");
            string dioNaziva = Console.ReadLine()?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(dioNaziva))
            {
                Console.WriteLine("Unos ne može biti prazan.");
                return new List<Film>();
            }

            return repo.GetAll()
                .Where(f => f.getNazivFilma().ToLower().Contains(dioNaziva))
                .ToList();
        }

        private List<Film> filtrirajPoKategoriji()
        {
            Console.Write("Unesite kategoriju za filtriranje: ");
            string kategorija = Console.ReadLine()?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(kategorija))
            {
                Console.WriteLine("Unos ne može biti prazan.");
                return new List<Film>();
            }

            return repo.GetAll()
                .Where(f => f.getKategorija().ToLower() == kategorija)
                .ToList();
        }

        private List<Film> filtrirajPoMinimalnojOcjeni()
        {
            Console.Write("Unesite minimalnu ocjenu: ");
            if (double.TryParse(Console.ReadLine(), out double minOcjena))
            {
                if (minOcjena < 1 || minOcjena > 10)
                {
                    Console.WriteLine("Ocjena mora biti u rasponu 1-10.");
                    return new List<Film>();
                }

                return repo.GetAll()
                    .Where(f => f.getOcjena() >= minOcjena)
                    .ToList();
            }
            else
            {
                Console.WriteLine("Neispravan format ocjene.");
                return new List<Film>();
            }
        }

        private List<Film> Merge(List<Film> left, List<Film> right, Func<Film, Film, bool> isLess)
        {
            if (left == null) left = new List<Film>();
            if (right == null) right = new List<Film>();

            var result = new List<Film>();
            int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
                // Ako je element sa lijeve strane manji (prema kriteriju isLess)
                if (isLess(left[i], right[j]))
                {
                    result.Add(left[i]);
                    i++;
                }
                else
                {
                    // U suprotnom, uzmi element sa desne strane
                    result.Add(right[j]);
                    j++;
                }
            }

            // Dodaj preostale elemente iz lijeve liste
            while (i < left.Count)
            {
                result.Add(left[i]);
                i++;
            }

            // Dodaj preostale elemente iz desne liste
            while (j < right.Count)
            {
                result.Add(right[j]);
                j++;
            }
            return result;
        }

        private List<Film> MergeSort(List<Film> list, Func<Film, Film, bool> isLess)
        {
            if (list == null || !list.Any())
                return new List<Film>();

            if (list.Count <= 1)
                return list;

            int mid = list.Count / 2;
            var left = new List<Film>(list.GetRange(0, mid));
            var right = new List<Film>(list.GetRange(mid, list.Count - mid));

            left = MergeSort(left, isLess);
            right = MergeSort(right, isLess);

            if (left.Count == 0 && right.Count == 0)
                return new List<Film>();

            return Merge(left, right, isLess);
        }

        public void SortirajPoOcjeni(bool smjerRastuci)
        {
            Func<Film, Film, bool> isLess;
            if (smjerRastuci)
                isLess = (f1, f2) => f1.getOcjena() < f2.getOcjena();
            else
                isLess = (f1, f2) => f1.getOcjena() > f2.getOcjena();

            var sortirano = MergeSort(repo.GetAll(), isLess);
            PrikaziListuFilmova(sortirano, $"Sortirana Lista (Ocjena - {(smjerRastuci ? "Rastuće" : "Opadajuće")}");
        }

        public void SortirajPoGodini(bool smjerRastuci)
        {
            Func<Film, Film, bool> isLess;
            if (smjerRastuci)
                isLess = (f1, f2) => f1.getGodina() < f2.getGodina();
            else
                isLess = (f1, f2) => f1.getGodina() > f2.getGodina();

            var sortirano = MergeSort(repo.GetAll(), isLess);
            PrikaziListuFilmova(sortirano, $"Sortirana Lista (Godina - {(smjerRastuci ? "Rastuće" : "Opadajuće")}");
        }

        public void SortirajPoNazivu(bool smjerRastuci)
        {
            Func<Film, Film, bool> isLess;
            if (smjerRastuci)
                isLess = (f1, f2) => string.Compare(f1.getNazivFilma(), f2.getNazivFilma()) < 0;
            else
                isLess = (f1, f2) => string.Compare(f1.getNazivFilma(), f2.getNazivFilma()) > 0;

            var sortirano = MergeSort(repo.GetAll(), isLess);
            PrikaziListuFilmova(sortirano, $"Sortirana Lista (Naziv - {(smjerRastuci ? "Rastuće" : "Opadajuće")}");
        }

        public void PrikaziJedinstveneKategorije()
        {
            if (repo.GetAll().Count == 0)
            {
                Console.WriteLine("Nema filmova, nema ni kategorija.");
                return;
            }

            var kategorije = repo.GetAll()
                                  .Select(f => f.getKategorija())
                                  .Distinct()
                                  .ToList();

            Console.WriteLine("\n--- Postojeće Kategorije ---");
            foreach (var kat in kategorije)
            {
                Console.WriteLine($"- {kat}");
            }
            Console.WriteLine("----------------------------");
        }

        private void PrikaziListuFilmova(List<Film> filmovi, string naslov)
        {
            if (filmovi == null || !filmovi.Any())
            {
                Console.WriteLine("Lista filmova je prazna ili nije pronađena.");
            }
            else
            {
                Console.WriteLine($"\n--- {naslov} ---");
                foreach (var f in filmovi)
                {
                    Console.WriteLine(f);
                }
            }
        }

        public virtual void prikaziFilmove()
        {
            PrikaziListuFilmova(repo.GetAll(), "Svi Filmovi u Bazi");
        }
    }
}
