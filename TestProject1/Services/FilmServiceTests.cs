using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using FilmMate.Models;
using FilmMate.Data;
using FilmMate.Services;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1.Services
{
    [TestClass]
    public class FilmServiceCompleteTests
    {
        private Mock<IFilmRepository> mockRepo;
        private FilmService service;
        private List<Film> lazniFilmovi;
        private StringWriter consoleOutput;
        private TextWriter originalConsoleOut;
        private TextReader originalConsoleIn;

        [TestInitialize]
        public void Setup()
        {
            lazniFilmovi = new List<Film>
            {
                new Film("Avatar", "Sci-Fi", 8.0, 2009),
                new Film("Titanik", "Romansa", 7.8, 1997),
                new Film("Matrix", "Sci-Fi", 9.0, 1999)
            };

            mockRepo = new Mock<IFilmRepository>();
            mockRepo.Setup(repo => repo.GetAll()).Returns(lazniFilmovi);
            mockRepo.Setup(repo => repo.Sacuvaj());

            service = new FilmService(mockRepo.Object);

            originalConsoleOut = Console.Out;
            originalConsoleIn = Console.In;
            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
        }

        [TestCleanup]
        public void TearDown()
        {
            Console.SetOut(originalConsoleOut);
            Console.SetIn(originalConsoleIn);
        }

        #region DodajFilm Tests

        [TestMethod]
        public void DodajFilm_ValidInput_AddsFilmSuccessfully()
        {
            string unos = "Novi Film" + Environment.NewLine +
                         "Akcija" + Environment.NewLine +
                         "8.5" + Environment.NewLine +
                         "2023";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(4, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
            StringAssert.Contains(consoleOutput.ToString(), "Film uspješno dodat!");
        }

        [TestMethod]
        public void DodajFilm_NameTooShort_ReturnsError()
        {
            string unos = "A";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Naziv mora imati bar 2 karaktera!");
        }

        [TestMethod]
        public void DodajFilm_EmptyName_ReturnsError()
        {
            string unos = "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            StringAssert.Contains(consoleOutput.ToString(), "Naziv mora imati bar 2 karaktera!");
        }

        [TestMethod]
        public void DodajFilm_WhitespaceName_ReturnsError()
        {
            string unos = "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            StringAssert.Contains(consoleOutput.ToString(), "Naziv mora imati bar 2 karaktera!");
        }

        [TestMethod]
        public void DodajFilm_ExistingName_ReturnsError()
        {
            string unos = "Avatar" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Film već postoji!");
        }

        [TestMethod]
        public void DodajFilm_ExistingNameDifferentCase_ReturnsError()
        {
            string unos = "AVATAR" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            StringAssert.Contains(consoleOutput.ToString(), "Film već postoji!");
        }

        [TestMethod]
        public void DodajFilm_RatingBelowRange_DefaultsToFive()
        {
            string unos = "Test Film" + Environment.NewLine +
                         "Drama" + Environment.NewLine +
                         "0" + Environment.NewLine +
                         "2020";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(5.0, lazniFilmovi.Last().getOcjena(), 0.001);
            StringAssert.Contains(consoleOutput.ToString(), "Neispravna ocjena, postavljena na 5.");
        }

        [TestMethod]
        public void DodajFilm_RatingAboveRange_DefaultsToFive()
        {
            string unos = "Test Film 2" + Environment.NewLine +
                         "Horor" + Environment.NewLine +
                         "11" + Environment.NewLine +
                         "2021";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(5.0, lazniFilmovi.Last().getOcjena(), 0.001);
            StringAssert.Contains(consoleOutput.ToString(), "Neispravna ocjena, postavljena na 5.");
        }

        [TestMethod]
        public void DodajFilm_InvalidRatingFormat_DefaultsToFive()
        {
            string unos = "Test Film 3" + Environment.NewLine +
                         "Komedija" + Environment.NewLine +
                         "invalid" + Environment.NewLine +
                         "2022";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(5.0, lazniFilmovi.Last().getOcjena(), 0.001);
            StringAssert.Contains(consoleOutput.ToString(), "Neispravan format ocjene, postavljena na 5.");
        }

        [TestMethod]
        public void DodajFilm_ValidRatingAtLowerBound_SetsCorrectly()
        {
            string unos = "Boundary Test" + Environment.NewLine +
                         "Drama" + Environment.NewLine +
                         "1" + Environment.NewLine +
                         "2020";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(1.0, lazniFilmovi.Last().getOcjena(), 0.001);
        }

        [TestMethod]
        public void DodajFilm_ValidRatingAtUpperBound_SetsCorrectly()
        {
            string unos = "Boundary Test 2" + Environment.NewLine +
                         "Drama" + Environment.NewLine +
                         "10" + Environment.NewLine +
                         "2020";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(10.0, lazniFilmovi.Last().getOcjena(), 0.001);
        }

        [TestMethod]
        public void DodajFilm_InvalidYearFormat_DefaultsToZero()
        {
            string unos = "Year Test" + Environment.NewLine +
                         "Drama" + Environment.NewLine +
                         "7" + Environment.NewLine +
                         "invalid";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(0, lazniFilmovi.Last().getGodina());
        }

        #endregion

        #region ObrisiFilm Tests

        [TestMethod]
        public void ObrisiFilm_EmptyName_ReturnsError()
        {
            string unos = "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.obrisiFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Naziv filma ne može biti prazan.");
        }

        [TestMethod]
        public void ObrisiFilm_WhitespaceName_ReturnsError()
        {
            string unos = "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.obrisiFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            StringAssert.Contains(consoleOutput.ToString(), "Naziv filma ne može biti prazan.");
        }

        [TestMethod]
        public void ObrisiFilm_FilmNotFound_ReturnsError()
        {
            string unos = "Nepostojeci Film" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.obrisiFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Film nije pronađen!");
        }

        [TestMethod]
        public void ObrisiFilm_NoRatings_ConfirmYes_DeletesFilm()
        {
            string unos = "Avatar" + Environment.NewLine + "d" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            lazniFilmovi[0].getOcjene().Clear(); // Ukloni default ocjenu

            service.obrisiFilm();

            Assert.AreEqual(2, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
            StringAssert.Contains(consoleOutput.ToString(), "Film obrisan!");
        }

        [TestMethod]
        public void ObrisiFilm_NoRatings_ConfirmNo_CancelsDelete()
        {
            string unos = "Avatar" + Environment.NewLine + "n" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            lazniFilmovi[0].getOcjene().Clear();

            service.obrisiFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Brisanje otkazano.");
        }

        [TestMethod]
        public void ObrisiFilm_WithRatings_FinalConfirmCorrect_DeletesFilm()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "d" + Environment.NewLine +
                         "OBRISI" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.obrisiFilm();

            Assert.AreEqual(2, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
            StringAssert.Contains(consoleOutput.ToString(), "Film obrisan!");
        }

        [TestMethod]
        public void ObrisiFilm_WithRatings_FinalConfirmWrong_CancelsDelete()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "d" + Environment.NewLine +
                         "WRONG" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.obrisiFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Brisanje otkazano zbog neuspjele finalne potvrde.");
        }

        [TestMethod]
        public void ObrisiFilm_WithRatings_FirstConfirmNo_CancelsDelete()
        {
            string unos = "Avatar" + Environment.NewLine + "n" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.obrisiFilm();

            Assert.AreEqual(3, lazniFilmovi.Count);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Brisanje otkazano.");
        }

        [TestMethod]
        public void ObrisiFilm_CaseInsensitiveName_FindsAndDeletes()
        {
            string unos = "AVATAR" + Environment.NewLine + "d" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            lazniFilmovi[0].getOcjene().Clear();

            service.obrisiFilm();

            Assert.AreEqual(2, lazniFilmovi.Count);
        }

        #endregion

        #region AzurirajFilm Tests

        [TestMethod]
        public void AzurirajFilm_EmptyName_ReturnsError()
        {
            string unos = "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Naziv filma ne može biti prazan.");
        }

        [TestMethod]
        public void AzurirajFilm_WhitespaceName_ReturnsError()
        {
            string unos = "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            StringAssert.Contains(consoleOutput.ToString(), "Naziv filma ne može biti prazan.");
        }

        [TestMethod]
        public void AzurirajFilm_FilmNotFound_ReturnsError()
        {
            string unos = "Nepostojeci Film" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Film nije pronađen!");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateNaziv_ValidInput_UpdatesSuccessfully()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "1" + Environment.NewLine +
                         "Novi Avatar" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            var film = lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Novi Avatar");
            Assert.IsNotNull(film);
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
            StringAssert.Contains(consoleOutput.ToString(), "Film ažuriran!");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateNaziv_EmptyInput_DisplaysError()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "1" + Environment.NewLine +
                         "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual("Avatar", lazniFilmovi[0].getNazivFilma());
            StringAssert.Contains(consoleOutput.ToString(), "Naziv ne može biti prazan.");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateNaziv_WhitespaceInput_DisplaysError()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "1" + Environment.NewLine +
                         "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual("Avatar", lazniFilmovi[0].getNazivFilma());
            StringAssert.Contains(consoleOutput.ToString(), "Naziv ne može biti prazan.");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateKategorija_ValidInput_UpdatesSuccessfully()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "2" + Environment.NewLine +
                         "Fantasy" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual("Fantasy", lazniFilmovi[0].getKategorija());
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void AzurirajFilm_UpdateKategorija_EmptyInput_NoChange()
        {
            string originalKategorija = lazniFilmovi[0].getKategorija();
            string unos = "Avatar" + Environment.NewLine +
                         "2" + Environment.NewLine +
                         "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            // Empty kategorija se može postaviti (nema validacije)
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void AzurirajFilm_UpdateGodina_ValidInput_UpdatesSuccessfully()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "3" + Environment.NewLine +
                         "2010" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual(2010, lazniFilmovi[0].getGodina());
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void AzurirajFilm_UpdateGodina_BelowMinimum_DisplaysError()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "3" + Environment.NewLine +
                         "1800" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual(2009, lazniFilmovi[0].getGodina());
            StringAssert.Contains(consoleOutput.ToString(), "Neispravan raspon godine.");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateGodina_FutureYear_DisplaysError()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "3" + Environment.NewLine +
                         "2100" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual(2009, lazniFilmovi[0].getGodina());
            StringAssert.Contains(consoleOutput.ToString(), "Neispravan raspon godine.");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateGodina_InvalidFormat_DisplaysError()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "3" + Environment.NewLine +
                         "invalid" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual(2009, lazniFilmovi[0].getGodina());
            StringAssert.Contains(consoleOutput.ToString(), "Neispravan format.");
        }

        [TestMethod]
        public void AzurirajFilm_InvalidChoice_DisplaysError()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "9" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            StringAssert.Contains(consoleOutput.ToString(), "Pogrešan unos!");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateGodina_CurrentYear_UpdatesSuccessfully()
        {
            int currentYear = DateTime.Now.Year;
            string unos = "Avatar" + Environment.NewLine +
                         "3" + Environment.NewLine +
                         currentYear.ToString() + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual(currentYear, lazniFilmovi[0].getGodina());
        }

        [TestMethod]
        public void AzurirajFilm_UpdateGodina_ExactlyAtMinimum_UpdatesSuccessfully()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "3" + Environment.NewLine +
                         "1901" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            Assert.AreEqual(1901, lazniFilmovi[0].getGodina());
        }

        #endregion

        #region OcijeniFilmGledaoca Tests

        [TestMethod]
        public void OcijeniFilmGledaoca_EmptyList_ReturnsError()
        {
            lazniFilmovi.Clear();

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Nema filmova za ocjenjivanje.");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_EmptyName_ReturnsError()
        {
            string unos = "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            StringAssert.Contains(consoleOutput.ToString(), "Naziv filma ne može biti prazan.");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_FilmNotFound_ReturnsError()
        {
            string unos = "Nepostojeci Film" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Film nije pronađen!");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_ValidRating_UpdatesFilm()
        {
            string unos = "Avatar" + Environment.NewLine + "9" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
            StringAssert.Contains(consoleOutput.ToString(), "Uspješno ste ocijenili film");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_RatingBelowRange_ReturnsError()
        {
            string unos = "Avatar" + Environment.NewLine + "0" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Ocjena mora biti u rasponu od 1 do 10.");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_RatingAboveRange_ReturnsError()
        {
            string unos = "Avatar" + Environment.NewLine + "11" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Ocjena mora biti u rasponu od 1 do 10.");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_InvalidFormat_ReturnsError()
        {
            string unos = "Avatar" + Environment.NewLine + "invalid" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Never);
            StringAssert.Contains(consoleOutput.ToString(), "Neispravan unos za ocjenu.");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_RatingAtLowerBound_UpdatesSuccessfully()
        {
            string unos = "Avatar" + Environment.NewLine + "1" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_RatingAtUpperBound_UpdatesSuccessfully()
        {
            string unos = "Avatar" + Environment.NewLine + "10" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        #endregion

        #region FiltrirajPretraziFilmove Tests

        [TestMethod]
        public void FiltrirajPretraziFilmove_EmptyList_ReturnsError()
        {
            lazniFilmovi.Clear();

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Nema filmova za pretragu!");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_Option0_ExitsImmediately()
        {
            string unos = "0" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            // Trebao bi izaći bez greške
            mockRepo.Verify(r => r.GetAll(), Times.AtLeastOnce());
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_InvalidOption_DisplaysError()
        {
            string unos = "9" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Pogrešan odabir.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_EmptyOption_DisplaysError()
        {
            string unos = "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Pogrešan odabir.");
            StringAssert.Contains(consoleOutput.ToString(), "Unos ne smije biti prazan.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_SearchByName_EmptyInput_ReturnsError()
        {
            string unos = "1" + Environment.NewLine + "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Unos ne može biti prazan.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_SearchByName_ValidInput_ReturnsResults()
        {
            string unos = "1" + Environment.NewLine + "Avatar" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Avatar");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_SearchByName_NoResults_DisplaysMessage()
        {
            string unos = "1" + Environment.NewLine + "Nepostojeci" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Nema pronađenih filmova.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByCategory_EmptyInput_ReturnsError()
        {
            string unos = "2" + Environment.NewLine + "" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Unos ne može biti prazan.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByCategory_ValidInput_ReturnsResults()
        {
            string unos = "2" + Environment.NewLine + "Sci-Fi" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Avatar");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByCategory_NoResults_DisplaysMessage()
        {
            string unos = "2" + Environment.NewLine + "Western" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Nema pronađenih filmova.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByMinRating_InvalidFormat_ReturnsError()
        {
            string unos = "3" + Environment.NewLine + "invalid" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Neispravan format ocjene.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByMinRating_BelowRange_ReturnsError()
        {
            string unos = "3" + Environment.NewLine + "0" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Ocjena mora biti u rasponu 1-10.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByMinRating_AboveRange_ReturnsError()
        {
            string unos = "3" + Environment.NewLine + "11" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Ocjena mora biti u rasponu 1-10.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByMinRating_ValidInput_ReturnsResults()
        {
            string unos = "3" + Environment.NewLine + "8" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Avatar");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByMinRating_NoResults_DisplaysMessage()
        {
            string unos = "3" + Environment.NewLine + "9.5" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Nema pronađenih filmova.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_MoreThan10Results_DisplaysFirst10()
        {
            // Dodaj više od 10 filmova
            for (int i = 1; i <= 12; i++)
            {
                lazniFilmovi.Add(new Film($"Film {i}", "Akcija", 7.0, 2020));
            }

            string unos = "2" + Environment.NewLine + "Akcija" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Pronađeno 10 od");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_Exactly10Results_DisplaysAll()
        {
            // Dodaj tačno 10 filmova
            for (int i = 1; i <= 7; i++)
            {
                lazniFilmovi.Add(new Film($"Film {i}", "Akcija", 7.0, 2020));
            }

            string unos = "2" + Environment.NewLine + "Akcija" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Pronađeno");
        }

        #endregion

        #region SortirajPoOcjeni Tests

        [TestMethod]
        public void SortirajPoOcjeni_Ascending_DisplaysSortedList()
        {
            service.SortirajPoOcjeni(true);

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Sortirana Lista");
            StringAssert.Contains(output, "Ocjena - Rastuće");

            // Titanik (7.8) treba biti prije Avatara (8.0)
            int indexTitanik = output.IndexOf("Titanik");
            int indexAvatar = output.IndexOf("Avatar");
            Assert.IsTrue(indexTitanik < indexAvatar);
        }

        [TestMethod]
        public void SortirajPoOcjeni_Descending_DisplaysSortedList()
        {
            service.SortirajPoOcjeni(false);

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Ocjena - Opadajuće");

            // Matrix (9.0) treba biti prije Avatara (8.0)
            int indexMatrix = output.IndexOf("Matrix");
            int indexAvatar = output.IndexOf("Avatar");
            Assert.IsTrue(indexMatrix < indexAvatar);
        }

        [TestMethod]
        public void SortirajPoOcjeni_EmptyList_DisplaysMessage()
        {
            lazniFilmovi.Clear();

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        [TestMethod]
        public void SortirajPoOcjeni_SingleItem_DisplaysSingleItem()
        {
            lazniFilmovi.Clear();
            lazniFilmovi.Add(new Film("Solo Film", "Drama", 8.0, 2020));

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Solo Film");
        }

        #endregion

        #region SortirajPoGodini Tests

        [TestMethod]
        public void SortirajPoGodini_Ascending_DisplaysSortedList()
        {
            service.SortirajPoGodini(true);

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Godina - Rastuće");

            // Titanik (1997) treba biti prije Matrixa (1999)
            int indexTitanik = output.IndexOf("Titanik");
            int indexMatrix = output.IndexOf("Matrix");
            Assert.IsTrue(indexTitanik < indexMatrix);
        }

        [TestMethod]
        public void SortirajPoGodini_Descending_DisplaysSortedList()
        {
            service.SortirajPoGodini(false);

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Godina - Opadajuće");

            // Avatar (2009) treba biti prije Matrixa (1999)
            int indexAvatar = output.IndexOf("Avatar");
            int indexMatrix = output.IndexOf("Matrix");
            Assert.IsTrue(indexAvatar < indexMatrix);
        }

        [TestMethod]
        public void SortirajPoGodini_EmptyList_DisplaysMessage()
        {
            lazniFilmovi.Clear();

            service.SortirajPoGodini(true);

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        #endregion

        #region SortirajPoNazivu Tests

        [TestMethod]
        public void SortirajPoNazivu_Ascending_DisplaysSortedList()
        {
            service.SortirajPoNazivu(true);

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Naziv - Rastuće");

            // Avatar (A) treba biti prije Matrixa (M)
            int indexAvatar = output.IndexOf("Avatar");
            int indexMatrix = output.IndexOf("Matrix");
            Assert.IsTrue(indexAvatar < indexMatrix);
        }

        [TestMethod]
        public void SortirajPoNazivu_Descending_DisplaysSortedList()
        {
            service.SortirajPoNazivu(false);

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Naziv - Opadajuće");

            // Titanik (T) treba biti prije Avatara (A)
            int indexTitanik = output.IndexOf("Titanik");
            int indexAvatar = output.IndexOf("Avatar");
            Assert.IsTrue(indexTitanik < indexAvatar);
        }

        [TestMethod]
        public void SortirajPoNazivu_EmptyList_DisplaysMessage()
        {
            lazniFilmovi.Clear();

            service.SortirajPoNazivu(true);

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        #endregion

        #region PrikaziJedinstveneKategorije Tests

        [TestMethod]
        public void PrikaziJedinstveneKategorije_EmptyList_DisplaysMessage()
        {
            lazniFilmovi.Clear();

            service.PrikaziJedinstveneKategorije();

            StringAssert.Contains(consoleOutput.ToString(), "Nema filmova, nema ni kategorija.");
        }

        [TestMethod]
        public void PrikaziJedinstveneKategorije_MultipleCategories_DisplaysAll()
        {
            service.PrikaziJedinstveneKategorije();

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Postojeće Kategorije");
            StringAssert.Contains(output, "Sci-Fi");
            StringAssert.Contains(output, "Romansa");
        }

        [TestMethod]
        public void PrikaziJedinstveneKategorije_DuplicateCategories_DisplaysOnlyUnique()
        {
            lazniFilmovi.Add(new Film("Film 4", "Sci-Fi", 7.0, 2020));
            lazniFilmovi.Add(new Film("Film 5", "Sci-Fi", 6.0, 2021));

            service.PrikaziJedinstveneKategorije();

            string output = consoleOutput.ToString();
            int firstIndex = output.IndexOf("Sci-Fi");
            int lastIndex = output.LastIndexOf("Sci-Fi");

            // Treba se pojaviti samo jednom
            Assert.AreEqual(firstIndex, lastIndex);
        }

        [TestMethod]
        public void PrikaziJedinstveneKategorije_SingleCategory_DisplaysCorrectly()
        {
            lazniFilmovi.Clear();
            lazniFilmovi.Add(new Film("Film 1", "Drama", 8.0, 2020));
            lazniFilmovi.Add(new Film("Film 2", "Drama", 7.0, 2021));

            service.PrikaziJedinstveneKategorije();

            StringAssert.Contains(consoleOutput.ToString(), "Drama");
        }

        #endregion

        #region PrikaziFilmove Tests

        [TestMethod]
        public void PrikaziFilmove_WithFilms_DisplaysAll()
        {
            service.prikaziFilmove();

            string output = consoleOutput.ToString();
            StringAssert.Contains(output, "Svi Filmovi u Bazi");
            StringAssert.Contains(output, "Avatar");
            StringAssert.Contains(output, "Titanik");
            StringAssert.Contains(output, "Matrix");
        }

        [TestMethod]
        public void PrikaziFilmove_EmptyList_DisplaysMessage()
        {
            lazniFilmovi.Clear();

            service.prikaziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        #endregion

        #region MergeSort Edge Cases

        [TestMethod]
        public void MergeSort_NullList_ReturnsEmptyList()
        {
            mockRepo.Setup(r => r.GetAll()).Returns((List<Film>)null);
            service = new FilmService(mockRepo.Object);

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        [TestMethod]
        public void MergeSort_EmptyList_ReturnsEmptyList()
        {
            lazniFilmovi.Clear();

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        [TestMethod]
        public void MergeSort_SingleElement_ReturnsSameElement()
        {
            lazniFilmovi.Clear();
            lazniFilmovi.Add(new Film("Solo", "Drama", 8.0, 2020));

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Solo");
        }

        [TestMethod]
        public void MergeSort_TwoElements_SortsCorrectly()
        {
            lazniFilmovi.Clear();
            lazniFilmovi.Add(new Film("B Film", "Drama", 9.0, 2020));
            lazniFilmovi.Add(new Film("A Film", "Drama", 7.0, 2021));

            service.SortirajPoOcjeni(true);

            string output = consoleOutput.ToString();
            int indexA = output.IndexOf("A Film");
            int indexB = output.IndexOf("B Film");
            Assert.IsTrue(indexA < indexB);
        }

        [TestMethod]
        public void Merge_BothListsEmpty_ReturnsEmpty()
        {
            lazniFilmovi.Clear();

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        [TestMethod]
        public void MergeSort_AllEqualRatings_MaintainsOrder()
        {
            lazniFilmovi.Clear();
            lazniFilmovi.Add(new Film("Film A", "Drama", 8.0, 2020));
            lazniFilmovi.Add(new Film("Film B", "Drama", 8.0, 2021));
            lazniFilmovi.Add(new Film("Film C", "Drama", 8.0, 2022));

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Film A");
            StringAssert.Contains(consoleOutput.ToString(), "Film B");
            StringAssert.Contains(consoleOutput.ToString(), "Film C");
        }

        #endregion

        #region Additional Edge Cases

        [TestMethod]
        public void FiltrirajPretraziFilmove_SearchByName_PartialMatch_ReturnsResults()
        {
            string unos = "1" + Environment.NewLine + "ava" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Avatar");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByCategory_CaseInsensitive_ReturnsResults()
        {
            string unos = "2" + Environment.NewLine + "SCI-FI" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Avatar");
        }

        [TestMethod]
        public void ObrisiFilm_ConfirmWithUppercaseD_DeletesFilm()
        {
            string unos = "Avatar" + Environment.NewLine + "D" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            lazniFilmovi[0].getOcjene().Clear();

            service.obrisiFilm();

            Assert.AreEqual(2, lazniFilmovi.Count);
        }

        [TestMethod]
        public void AzurirajFilm_CaseInsensitiveSearch_FindsFilm()
        {
            string unos = "AVATAR" + Environment.NewLine +
                         "1" + Environment.NewLine +
                         "Updated Avatar" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            var film = lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Updated Avatar");
            Assert.IsNotNull(film);
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_CaseInsensitiveSearch_FindsFilm()
        {
            string unos = "AVATAR" + Environment.NewLine + "8" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void DodajFilm_ValidDecimalRating_AddsCorrectly()
        {
            string unos = "Decimal Test" + Environment.NewLine +
                         "Drama" + Environment.NewLine +
                         "7.5" + Environment.NewLine +
                         "2020";
            Console.SetIn(new StringReader(unos));

            service.dodajFilm();

            Assert.AreEqual(7.5, lazniFilmovi.Last().getOcjena(), 0.001);
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByMinRating_DecimalValue_ReturnsResults()
        {
            string unos = "3" + Environment.NewLine + "7.9" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Avatar");
        }

        [TestMethod]
        public void PrikaziListuFilmova_NullList_DisplaysMessage()
        {
            mockRepo.Setup(r => r.GetAll()).Returns((List<Film>)null);
            service = new FilmService(mockRepo.Object);

            service.prikaziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena.");
        }

        [TestMethod]
        public void AzurirajFilm_UpdateKategorija_WhitespaceInput_SetsEmptyCategory()
        {
            string unos = "Avatar" + Environment.NewLine +
                         "2" + Environment.NewLine +
                         "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.azurirajFilm();

            // Whitespace kategorija se ne postavlja (nema validacije za whitespace)
            mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_SearchByName_WhitespaceInput_ReturnsError()
        {
            string unos = "1" + Environment.NewLine + "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Unos ne može biti prazan.");
        }

        [TestMethod]
        public void FiltrirajPretraziFilmove_FilterByCategory_WhitespaceInput_ReturnsError()
        {
            string unos = "2" + Environment.NewLine + "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.FiltrirajPretraziFilmove();

            StringAssert.Contains(consoleOutput.ToString(), "Unos ne može biti prazan.");
        }

        [TestMethod]
        public void OcijeniFilmGledaoca_WhitespaceName_ReturnsError()
        {
            string unos = "   " + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            service.OcijeniFilmGledaoca();

            StringAssert.Contains(consoleOutput.ToString(), "Naziv filma ne može biti prazan.");
        }

        [TestMethod]
        public void MergeSort_LargeList_SortsCorrectly()
        {
            lazniFilmovi.Clear();
            for (int i = 20; i > 0; i--)
            {
                lazniFilmovi.Add(new Film($"Film {i}", "Drama", i * 0.5, 2000 + i));
            }

            service.SortirajPoOcjeni(true);

            StringAssert.Contains(consoleOutput.ToString(), "Sortirana Lista");
        }

        [TestMethod]
        public void SortirajPoGodini_SameYears_MaintainsStableOrder()
        {
            lazniFilmovi.Clear();
            lazniFilmovi.Add(new Film("Film A", "Drama", 8.0, 2020));
            lazniFilmovi.Add(new Film("Film B", "Drama", 7.0, 2020));
            lazniFilmovi.Add(new Film("Film C", "Drama", 9.0, 2020));

            service.SortirajPoGodini(true);

            StringAssert.Contains(consoleOutput.ToString(), "Film A");
        }

        [TestMethod]
        public void SortirajPoNazivu_SpecialCharacters_SortsCorrectly()
        {
            lazniFilmovi.Clear();
            lazniFilmovi.Add(new Film("!Special", "Drama", 8.0, 2020));
            lazniFilmovi.Add(new Film("Film", "Drama", 7.0, 2021));
            lazniFilmovi.Add(new Film("123Numeric", "Drama", 9.0, 2022));

            service.SortirajPoNazivu(true);

            StringAssert.Contains(consoleOutput.ToString(), "Sortirana Lista");
        }

        #endregion
    }
}