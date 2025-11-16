using FilmMate.Data;
using FilmMate.Services;
using FilmMate.Models;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.WriteLine("--- FilmMate Aplikacija Pokrenuta ---");

        var userRepo = new UserRepository();
        var filmRepo = new FilmRepository();
        var userService = new UserService(userRepo);
        var filmService = new FilmService(filmRepo);


        while (true)
        {
            Console.WriteLine("\n1. Registracija\n2. Prijava\n3. Izlaz");
            Console.Write("Odabir: ");
            string izbor = Console.ReadLine() ?? "";

            if (izbor == "1") userService.Registracija();
            else if (izbor == "2")
            {
                var user = userService.Prijava();
                if (user is Administrator admin)
                    AdminMeni(admin, filmService);
                else if (user is Gledalac gledalac)
                    GledalacMeni(gledalac, filmService);
            }
            else if (izbor == "3") break;
            else Console.WriteLine("Pogrešan unos!");
        }
    }

    static void AdminMeni(Administrator admin, FilmService fs)
    {
        bool nazad = false;
        while (!nazad)
        {
            Console.WriteLine("\n--- Admin Meni ---");
            Console.WriteLine("1. Dodaj film\n2. Ažuriraj film\n3. Obriši film\n4. Prikaz filmova\n5. Pretraga/Filtriranje filmova\n6. Prikaz svih kategorija\n7. Sortiraj Filmove\n0. Nazad");
            string odabir = Console.ReadLine() ?? "";

            switch (odabir)
            {
                case "1": admin.dodajFilm(fs); break;
                case "2": admin.azurirajFilm(fs); break;
                case "3": admin.obrisiFilm(fs); break;
                case "4": fs.prikaziFilmove(); break;
                case "5": fs.FiltrirajPretraziFilmove(); break;
                case "6": fs.PrikaziJedinstveneKategorije(); break;
                case "7": SortirajMeni.PrikaziSortirajMeni(fs); break;
                case "0": nazad = true; break;
                default: Console.WriteLine("Pogrešan unos!"); break;
            }
        }
    }

    static void GledalacMeni(Gledalac gledalac, FilmService fs)
    {
        bool nazad = false;
        while (!nazad)
        {
            Console.WriteLine("\n--- Gledalac Meni ---");
            Console.WriteLine("1. Prikaz svih filmova");
            Console.WriteLine("2. Pretraga, Filtriranje i Sortiranje");
            Console.WriteLine("0. Nazad (Odjava)");
            Console.Write("Odabir: ");
            string odabir = Console.ReadLine() ?? "";

            switch (odabir)
            {
                case "1": fs.prikaziFilmove(); break;
                case "2": gledalac.pretragaFilmova(fs); break; 
                case "0": nazad = true; break;
                default: Console.WriteLine("Pogrešan unos!"); break;
            }
        }
    }
}

public static class SortirajMeni
{
    public static void PrikaziSortirajMeni(FilmService fs)
    {
        bool nazad = false;
        while (!nazad)
        {
            Console.WriteLine("\n--- Sortiranje Filmova ---");
            Console.WriteLine("1. Po Ocjeni\n2. Po Godini Izlaska\n3. Po Nazivu\n0. Nazad");
            Console.Write("Odaberite kriterij: ");
            string odabirKriterija = Console.ReadLine() ?? "";

            if (odabirKriterija == "0") { nazad = true; continue; }
            
            if (odabirKriterija == "1" || odabirKriterija == "2" || odabirKriterija == "3")
            {
                 Console.WriteLine("Odaberite smjer:\n  A. Rastuće (Ascending)\n  D. Opadajuće (Descending)");
                 Console.Write("Smjer (A/D): ");
                 string smjerOdabir = Console.ReadLine()?.ToUpper() ?? "";

                 bool smjerRastuci;
                 
                 if (smjerOdabir == "A")
                 {
                      smjerRastuci = true;
                 }
                 else if (smjerOdabir == "D")
                 {
                      smjerRastuci = false;
                 }
                 else
                 {
                      Console.WriteLine("Neispravan odabir smjera.");
                      continue;
                 }

                 switch (odabirKriterija)
                 {
                     case "1": fs.SortirajPoOcjeni(smjerRastuci); break;
                     case "2": fs.SortirajPoGodini(smjerRastuci); break;
                     case "3": fs.SortirajPoNazivu(smjerRastuci); break;
                 }
            }
            else
            {
                Console.WriteLine("Pogrešan unos!");
            }
        }
    }
}