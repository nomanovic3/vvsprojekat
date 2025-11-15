using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmMate.Data;
using FilmMate.Models;
using System.IO;
using System.Linq;

namespace TestProject1.Data
{
    [TestClass]
    public class FilmRepositoryTests
    {
        private string testFilePath = "filmovi.txt";

        [TestCleanup]
        public void TearDown()
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        [TestMethod]
        public void Constructor_WithValidFile_LoadsFilmsCorrectly()
        {
            // Kreiranje test fajla
            File.WriteAllLines(testFilePath, new[]
            {
                "Inception;Sci-Fi;8.5;2010",
                "Gladiator;Action;8.0;2000"
            });

            var repo = new FilmRepository();
            var filmovi = repo.GetAll();

            Assert.AreEqual(2, filmovi.Count);
            Assert.AreEqual("Inception", filmovi[0].getNazivFilma());
            Assert.AreEqual("Sci-Fi", filmovi[0].getKategorija());
            Assert.AreEqual(2010, filmovi[0].getGodina());
        }

        [TestMethod]
        public void Constructor_NonExistentFile_CreatesEmptyList()
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);

            var repo = new FilmRepository();

            Assert.AreEqual(0, repo.GetAll().Count);
        }

        [TestMethod]
        public void Constructor_FileWithInvalidData_SkipsInvalidLines()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "Inception;Sci-Fi;8.5;2010",
                "Invalid;Line",  // Neispravan format - samo 2 polja
                "Matrix;Sci-Fi;9.0;1999"
            });

            var repo = new FilmRepository();

            // Treba da učita samo 2 validna filma
            Assert.AreEqual(2, repo.GetAll().Count);
        }

        [TestMethod]
        public void Sacuvaj_WithMultipleFilms_SavesAllToFile()
        {
            var repo = new FilmRepository();
            repo.GetAll().Add(new Film("Matrix", "Sci-Fi", 9.0, 1999));
            repo.GetAll().Add(new Film("Titanic", "Drama", 7.8, 1997));

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(2, lines.Length);
            StringAssert.Contains(lines[0], "Matrix");
            StringAssert.Contains(lines[1], "Titanic");
        }

        [TestMethod]
        public void Sacuvaj_EmptyList_CreatesEmptyFile()
        {
            var repo = new FilmRepository();
            repo.GetAll().Clear();

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(0, lines.Length);
        }

        [TestMethod]
        public void GetAll_ReturnsLiveReference_ModificationsReflected()
        {
            var repo = new FilmRepository();
            var listaFilmova = repo.GetAll();

            listaFilmova.Add(new Film("Test Film", "Akcija", 7.0, 2020));

            // GetAll() vraća istu referencu
            Assert.AreEqual(1, repo.GetAll().Count);
        }

        [TestMethod]
        public void Sacuvaj_WithSpecialCharacters_SavesCorrectly()
        {
            var repo = new FilmRepository();
            repo.GetAll().Add(new Film("Film: Test & More", "Komedija", 6.5, 2021));

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            StringAssert.Contains(lines[0], "Film: Test & More");
        }
    }
}