using FilmMate.Data;
using FilmMate.Models;
using FilmMate.Services;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject1.Models
{
    [TestClass]
    public class GostTests
    {
        [TestMethod]
        public void PregledajFilmove_Poziva_prikaziFilmove_Na_FilmService()
        {
            var mockRepo = new Mock<IFilmRepository>();
            var filmService = new FilmService(mockRepo.Object);
            var gost = new Gost();

            var mockFilmovi = new List<Film> {
                new Film("Test Film 1", "Akcija", 8.5, 2020),
                new Film("Test Film 2", "Komedija", 7.0, 2022)
            };

            mockRepo.Setup(r => r.GetAll()).Returns(mockFilmovi);

            var sb = new StringBuilder();
            var sw = new System.IO.StringWriter(sb);
            Console.SetOut(sw);

            gost.pregledajFilmove(filmService);

            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);

            mockRepo.Verify(r => r.GetAll(), Times.Once(), "Očekivalo se da će FilmRepository.GetAll() biti pozvan.");

            string consoleOutput = sb.ToString();
            Assert.IsTrue(consoleOutput.Contains("Svi Filmovi u Bazi"), "Očekivani naslov ispisa nije pronađen.");
            Assert.IsTrue(consoleOutput.Contains("Test Film 1"), "Očekivani film 'Test Film 1' nije pronađen u ispisu.");
        }
    }
}