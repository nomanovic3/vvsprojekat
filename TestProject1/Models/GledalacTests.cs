using FilmMate.Data;
using FilmMate.Models;
using FilmMate.Services;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TestProject1.Models
{
    [TestClass]
    public class GledalacTests
    {
        private Mock<IFilmRepository> mockRepo;
        private FilmService filmService;
        private Gledalac gledalac;
        private StringBuilder sb;
        private TextWriter originalConsoleOut;
        private TextReader originalConsoleIn;

        [TestInitialize]
        public void SetUp()
        {
            mockRepo = new Mock<IFilmRepository>();
            filmService = new FilmService(mockRepo.Object);
            gledalac = new Gledalac();

            originalConsoleOut = Console.Out;
            originalConsoleIn = Console.In;

            sb = new StringBuilder();
            Console.SetOut(new StringWriter(sb));

            var mockFilmovi = new List<Film> {
                new Film("Avatar", "Sci-Fi", 8.0, 2009),
                new Film("Titanik", "Drama", 7.5, 1997)
            };
            mockRepo.Setup(r => r.GetAll()).Returns(mockFilmovi);
            mockRepo.Setup(r => r.Sacuvaj());
        }

        [TestCleanup]
        public void TearDown()
        {
            Console.SetOut(originalConsoleOut);
            Console.SetIn(originalConsoleIn);
        }

        [TestMethod]
        public void PregledajFilmove_Poziva_prikaziFilmove_I_Ispisuje_Listu()
        {
            gledalac.pregledajFilmove(filmService);

            mockRepo.Verify(r => r.GetAll(), Times.AtLeastOnce());

            string consoleOutput = sb.ToString();
            Assert.IsTrue(consoleOutput.Contains("--- Svi Filmovi u Bazi ---"));
            Assert.IsTrue(consoleOutput.Contains("Avatar"));
        }

        [TestMethod]
        public void PretragaFilmova_SearchByName_DisplaysResults()
        {
            string unos = "1" + Environment.NewLine + "Avatar" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            gledalac.pretragaFilmova(filmService);

            mockRepo.Verify(r => r.GetAll(), Times.AtLeastOnce());
            string consoleOutput = sb.ToString();
            Assert.IsTrue(consoleOutput.Contains("Avatar"));
        }

        [TestMethod]
        public void PretragaFilmova_FilterByCategory_DisplaysResults()
        {
            string unos = "2" + Environment.NewLine + "Sci-Fi" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            gledalac.pretragaFilmova(filmService);

            mockRepo.Verify(r => r.GetAll(), Times.AtLeastOnce());
            Assert.IsTrue(sb.ToString().Contains("Avatar"));
        }

        [TestMethod]
        public void PretragaFilmova_FilterByMinRating_DisplaysResults()
        {
            string unos = "3" + Environment.NewLine + "7.5" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            gledalac.pretragaFilmova(filmService);

            mockRepo.Verify(r => r.GetAll(), Times.AtLeastOnce());
        }

        [TestMethod]
        public void PretragaFilmova_ExitOption_ReturnsWithoutError()
        {
            string unos = "0" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            gledalac.pretragaFilmova(filmService);

            Assert.IsFalse(sb.ToString().Contains("Greška"));
        }

        [TestMethod]
        public void PretragaFilmova_InvalidChoice_DisplaysError()
        {
            string unos = "9" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            gledalac.pretragaFilmova(filmService);

            Assert.IsTrue(sb.ToString().Contains("Pogrešan odabir"));
        }

        [TestMethod]
        public void PretragaFilmova_EmptyInput_DisplaysError()
        {
            string unos = "1" + Environment.NewLine + "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            gledalac.pretragaFilmova(filmService);

            Assert.IsTrue(sb.ToString().Contains("Unos ne može biti prazan"));
        }
    }
}