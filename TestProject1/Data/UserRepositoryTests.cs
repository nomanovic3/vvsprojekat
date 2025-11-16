using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmMate.Data;
using FilmMate.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestProject1.Data
{
    [TestClass]
    public class UserRepositoryTests
    {
        private string tempFile;

        [TestInitialize]
        public void Setup()
        {
            tempFile = Path.GetTempFileName();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        // ------------------ TEST 1: Učitavanje kada fajl postoji ------------------
        [TestMethod]
        public void Konstruktor_TrebaUcitatiKorisnike_IzFajla()
        {
            File.WriteAllLines(tempFile, new[]
            {
                "admin1;pass1;admin",
                "user1;lozinka;user"
            });

            var repo = new TestableUserRepository(tempFile);

            var korisnici = repo.GetAll();

            Assert.AreEqual(2, korisnici.Count, "Očekivana su 2 korisnika.");
            Assert.IsTrue(korisnici[0] is Administrator, "Prvi korisnik treba biti Admin.");
            Assert.IsTrue(korisnici[1] is Gledalac, "Drugi korisnik treba biti obični user.");
            Assert.AreEqual("admin1", korisnici[0].getKorisnickoIme());
        }

        // ------------------ TEST 2: Učitavanje kada fajl ne postoji ------------------
        [TestMethod]
        public void Ucitaj_FajlNePostoji_NeBacaException()
        {
            string nonExist = tempFile + "_missing";

            var repo = new TestableUserRepository(nonExist);

            Assert.AreEqual(0, repo.GetAll().Count, "Lista treba biti prazna ako fajl ne postoji.");
        }

        // ------------------ TEST 3: Sacuvaj() generiše ispravan format ------------------
        [TestMethod]
        public void Sacuvaj_TrebaUpisatiIspravnePodatkeUFajl()
        {
            var repo = new TestableUserRepository(tempFile);

            var admin = new Administrator();
            admin.setKorisnickoIme("adminTest");
            admin.setLozinka("123");

            var user = new Gledalac();
            user.setKorisnickoIme("gledalac");
            user.setLozinka("abc");

            repo.GetAll().Add(admin);
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(tempFile);

            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual("adminTest;123;admin", lines[0]);
            Assert.AreEqual("gledalac;abc;user", lines[1]);
        }

        // ------------------ TEST 4: GetAll vraća pravu listu ------------------
        [TestMethod]
        public void GetAll_TrebaVratitiInternuListu()
        {
            var repo = new TestableUserRepository(tempFile);

            var u = new Gledalac();
            u.setKorisnickoIme("ime1");
            u.setLozinka("loz");

            repo.GetAll().Add(u);

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("ime1", repo.GetAll()[0].getKorisnickoIme());
        }

        // --------- Interna testabilna verzija koja koristi proširene metode ---------
        private class TestableUserRepository : UserRepository
        {
            public TestableUserRepository(string path)
            {
                this.SetFilePath(path);
                this.InvokeLoad();
            }
        }
    }

    // =================== EKSTENZIJE ZA TESTIRANJE PRIVATE POLJA =====================
    public static class UserRepositoryExtensions
    {
        public static void SetFilePath(this UserRepository repo, string val)
        {
            typeof(UserRepository)
                .GetField("filePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(repo, val);
        }

        public static void InvokeLoad(this UserRepository repo)
        {
            typeof(UserRepository)
                .GetMethod("Ucitaj", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(repo, null);
        }
    }
}
