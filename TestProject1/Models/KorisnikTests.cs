using FilmMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Models
{
    [TestClass]
    public class KorisnikTests
    {
        // Koristićemo Administratora za testiranje logike nasljedjivana
        private Administrator korisnik;

        [TestInitialize]
        public void Initialize()
        {
            korisnik = new Administrator();
        }

        [TestMethod]
        public void SetAndGetKorisnickoIme_ShouldReturnCorrectValue()
        {
            string novoIme = "testUser123";
            korisnik.setKorisnickoIme(novoIme);

            // ASSERT
            Assert.AreEqual(novoIme, korisnik.getKorisnickoIme(), "Korisničko ime nije uspešno postavljeno ili pročitano.");
        }

        [TestMethod]
        public void SetAndGetLozinka_ShouldReturnCorrectValue()
        {
            string novaLozinka = "tajnaLozinka456";
            korisnik.setLozinka(novaLozinka);

            Assert.AreEqual(novaLozinka, korisnik.getLozinka(), "Lozinka nije uspešno postavljena ili pročitana.");
        }
    }
}
