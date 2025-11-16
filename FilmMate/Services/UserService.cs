using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using FilmMate.Models;
using FilmMate.Data;

namespace FilmMate.Services
{
    public class UserService
    {
        private UserRepository userRepo;

        public UserService(UserRepository repo)
        {
            userRepo = repo;
        }

        public void Registracija()
        {
            Console.Write("Korisničko ime: ");
            string ime = Console.ReadLine() ?? "";

            bool postoji = false;
            foreach (var k in userRepo.GetAll())
            {
                if (k.getKorisnickoIme().ToLower() == ime.ToLower())
                {
                    postoji = true;
                    break;
                }
            }

            if (postoji)
            {
                Console.WriteLine("Korisnik već postoji!");
                return;
            }

            Console.Write("Lozinka: ");
            string pass = Console.ReadLine() ?? "";
            string hash = GenerisiHash(pass);

            Gledalac novi = new Gledalac();
            novi.setKorisnickoIme(ime);
            novi.setLozinka(hash);

            userRepo.GetAll().Add(novi);
            userRepo.Sacuvaj();
            Console.WriteLine("Registracija uspješna!");
        }

        public Korisnik? Prijava()
        {
            Console.Write("Korisničko ime: ");
            string ime = Console.ReadLine() ?? "";
            Console.Write("Lozinka: ");
            string pass = Console.ReadLine() ?? "";

            string hash = GenerisiHash(pass);
            Korisnik? korisnik = null;

            foreach (var k in userRepo.GetAll())
            {
                if (k.getKorisnickoIme() == ime && k.getLozinka() == hash)
                {
                    korisnik = k;
                    break;
                }
            }

            if (korisnik == null)
                Console.WriteLine("Pogrešno korisničko ime ili lozinka!");
            else
                Console.WriteLine($"Dobrodošao {korisnik.getKorisnickoIme()}");

            return korisnik;
        }

        private string GenerisiHash(string lozinka)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(lozinka));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}

