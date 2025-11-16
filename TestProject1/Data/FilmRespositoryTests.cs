using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmMate.Data;
using FilmMate.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestProject1.Data
{
    [TestClass]
    public class FilmRepositoryTests
    {
        private string tempFile;

        [TestInitialize]
        public void Setup()
        {
            // kreiramo privremeni fajl za test
            tempFile = Path.GetTempFileName();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        // --------------------- TEST: Učitavanje iz fajla ---------------------
        [TestMethod]
        public void Konstruktor_TrebaUcitatiFilmove_IzPostojecegFajla()
        {
            File.WriteAllLines(tempFile, new[]
            {
                "Film1;Akcija;8.2;2020",
                "Film2;Drama;7.5;2018"
            });

            // zamjena default putanje
            var repo = new TestableFilmRepository(tempFile);

            var result = repo.GetAll();

            Assert.AreEqual(2, result.Count, "Očekivana su 2 učitana filma.");
            Assert.AreEqual("Film1", result[0].getNazivFilma());
        }

        // --------------------- TEST: Fajl ne postoji ---------------------
        [TestMethod]
        public void UcitajFilmove_FajlNePostoji_NeSmijeBacitiException()
        {
            string nonExisting = tempFile + "_missing";

            var repo = new TestableFilmRepository(nonExisting);

            Assert.AreEqual(0, repo.GetAll().Count, "Lista filmova treba biti prazna ako fajl ne postoji.");
        }

        // --------------------- TEST: Sacuvaj() -----------------------
        [TestMethod]
        public void Sacuvaj_TrebaUpisatiIspravanSadrzajUFajl()
        {
            var repo = new TestableFilmRepository(tempFile);

            var f1 = new Film("Test1", "Akcija", 9.1, 2019);
            var f2 = new Film("Test2", "Komedija", 7.0, 2021);

            repo.GetAll().Add(f1);
            repo.GetAll().Add(f2);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(tempFile);

            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual("Test1;Akcija;9;2019", lines[0]);
            Assert.AreEqual("Test2;Komedija;7;2021", lines[1]);
        }

        // --------------------- TEST: GetAll -----------------------
        [TestMethod]
        public void GetAll_TrebaVratitiInternuListu()
        {
            var repo = new TestableFilmRepository(tempFile);

            var f = new Film("FilmTest", "SciFi", 8.8, 2024);
            repo.GetAll().Add(f);

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("FilmTest", repo.GetAll()[0].getNazivFilma());
        }


        // --- Pomoćna klasa koja dopušta setovanje putanje fajla ---
        private class TestableFilmRepository : FilmRepository
        {
            public TestableFilmRepository(string path)
            {
                this.SetFilePath(path);
                this.InvokeLoad();
            }
        }
    }

    // =================== EXTENZIJE ZA TESTIRANJE =====================
    public static class FilmRepositoryExtensions
    {
        public static void SetFilePath(this FilmRepository repo, string path)
        {
            typeof(FilmRepository)
                .GetField("filePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(repo, path);
        }

        public static void InvokeLoad(this FilmRepository repo)
        {
            typeof(FilmRepository)
                .GetMethod("UcitajFilmove", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(repo, null);
        }
    }
}
