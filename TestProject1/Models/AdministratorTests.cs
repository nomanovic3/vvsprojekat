using FilmMate.Data;
using FilmMate.Models;
using FilmMate.Services;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System;

namespace TestProject1.Models
{
    [TestClass]
    public class AdministratorTests
    {
        private Mock<IFilmRepository> mockRepo;
        private FilmService filmService;
        private Administrator admin;
        private StringWriter consoleOutput;

        [TestInitialize]
        public void SetUp()
        {
            mockRepo = new Mock<IFilmRepository>();
            var filmovi = new List<Film>
            {
                new Film("Test Film", "Drama", 7.0, 2020)
            };
            mockRepo.Setup(r => r.GetAll()).Returns(filmovi);
            mockRepo.Setup(r => r.Sacuvaj());

            filmService = new FilmService(mockRepo.Object);
            admin = new Administrator();

            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
        }

        [TestCleanup]
        public void TearDown()
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        [TestMethod]
        public void DodajFilm_CallsFilmServiceDodajFilm()
        {
            string unos = "Novi Film Admin" + Environment.NewLine +
                         "Akcija" + Environment.NewLine +
                         "8" + Environment.NewLine +
                         "2023";
            Console.SetIn(new StringReader(unos));

            admin.dodajFilm(filmService);

            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void ObrisiFilm_CallsFilmServiceObrisiFilm()
        {
            string unos = "Test Film" + Environment.NewLine + "d" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            admin.obrisiFilm(filmService);

            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void AzurirajFilm_CallsFilmServiceAzurirajFilm()
        {
            string unos = "Test Film" + Environment.NewLine +
                         "1" + Environment.NewLine +
                         "Novi Naziv";
            Console.SetIn(new StringReader(unos));

            admin.azurirajFilm(filmService);

            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }
    }
}