using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmMate.Services;
using FilmMate.Data;
using FilmMate.Models;
using System;
using System.IO;

namespace TestProject1.Services
{
    [TestClass]
    public class UserServiceTests
    {
        private UserRepository userRepo;
        private UserService userService;
        private StringWriter consoleOutput;
        private TextWriter originalConsoleOut;
        private TextReader originalConsoleIn;
        private string testFilePath = "korisnici.txt";

        [TestInitialize]
        public void SetUp()
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);

            userRepo = new UserRepository();
            userService = new UserService(userRepo);

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

            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        [TestMethod]
        public void Registracija_ValidInput_AddsUserSuccessfully()
        {
            string unos = "noviKorisnik" + Environment.NewLine + "lozinka123" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            userService.Registracija();

            Assert.AreEqual(1, userRepo.GetAll().Count);
            StringAssert.Contains(consoleOutput.ToString(), "Registracija uspješna!");
        }

        [TestMethod]
        public void Registracija_ExistingUsername_ReturnsError()
        {
            string unos1 = "postojeciKorisnik" + Environment.NewLine + "lozinka1" + Environment.NewLine;
            Console.SetIn(new StringReader(unos1));
            userService.Registracija();

            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            string unos2 = "postojeciKorisnik" + Environment.NewLine + "lozinka2" + Environment.NewLine;
            Console.SetIn(new StringReader(unos2));
            userService.Registracija();

            Assert.AreEqual(1, userRepo.GetAll().Count);
            StringAssert.Contains(consoleOutput.ToString(), "Korisnik već postoji!");
        }

        [TestMethod]
        public void Registracija_CaseInsensitiveUsername_ReturnsError()
        {
            string unos1 = "TestUser" + Environment.NewLine + "pass1" + Environment.NewLine;
            Console.SetIn(new StringReader(unos1));
            userService.Registracija();

            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            string unos2 = "testuser" + Environment.NewLine + "pass2" + Environment.NewLine;
            Console.SetIn(new StringReader(unos2));
            userService.Registracija();

            Assert.AreEqual(1, userRepo.GetAll().Count);
            StringAssert.Contains(consoleOutput.ToString(), "Korisnik već postoji!");
        }

        [TestMethod]
        public void Prijava_ValidCredentials_ReturnsUser()
        {
            string registracija = "testUser" + Environment.NewLine + "testPass" + Environment.NewLine;
            Console.SetIn(new StringReader(registracija));
            userService.Registracija();

            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            string prijava = "testUser" + Environment.NewLine + "testPass" + Environment.NewLine;
            Console.SetIn(new StringReader(prijava));

            var korisnik = userService.Prijava();

            Assert.IsNotNull(korisnik);
            Assert.AreEqual("testUser", korisnik.getKorisnickoIme());
            StringAssert.Contains(consoleOutput.ToString(), "Dobrodošao testUser");
        }

        [TestMethod]
        public void Prijava_InvalidCredentials_ReturnsNull()
        {
            string unos = "nepostojeciKorisnik" + Environment.NewLine + "pogresnaLozinka" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            var korisnik = userService.Prijava();

            Assert.IsNull(korisnik);
            StringAssert.Contains(consoleOutput.ToString(), "Pogrešno korisničko ime ili lozinka!");
        }

        [TestMethod]
        public void Prijava_WrongPassword_ReturnsNull()
        {
            string registracija = "user1" + Environment.NewLine + "correctPass" + Environment.NewLine;
            Console.SetIn(new StringReader(registracija));
            userService.Registracija();

            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            string prijava = "user1" + Environment.NewLine + "wrongPass" + Environment.NewLine;
            Console.SetIn(new StringReader(prijava));

            var korisnik = userService.Prijava();

            Assert.IsNull(korisnik);
            StringAssert.Contains(consoleOutput.ToString(), "Pogrešno korisničko ime ili lozinka!");
        }

        [TestMethod]
        public void Prijava_CorrectUsernameWrongPassword_ReturnsNull()
        {
            string registracija = "validUser" + Environment.NewLine + "validPass123" + Environment.NewLine;
            Console.SetIn(new StringReader(registracija));
            userService.Registracija();

            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            string prijava = "validUser" + Environment.NewLine + "invalidPass" + Environment.NewLine;
            Console.SetIn(new StringReader(prijava));

            var korisnik = userService.Prijava();

            Assert.IsNull(korisnik);
        }

        [TestMethod]
        public void Registracija_EmptyUsername_StillAddsUser()
        {
            string unos = "" + Environment.NewLine + "lozinka" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            userService.Registracija();

            // UserService ne provjerava prazan username, dodaje korisnika
            Assert.AreEqual(1, userRepo.GetAll().Count);
        }

        [TestMethod]
        public void Prijava_EmptyUsername_ReturnsNull()
        {
            string unos = "" + Environment.NewLine + "lozinka" + Environment.NewLine;
            Console.SetIn(new StringReader(unos));

            var korisnik = userService.Prijava();

            Assert.IsNull(korisnik);
        }

        [TestMethod]
        public void GenerisiHash_SamePassword_ReturnsSameHash()
        {
            // Registruj dva korisnika sa istom lozinkom
            string unos1 = "user1" + Environment.NewLine + "ista123" + Environment.NewLine;
            Console.SetIn(new StringReader(unos1));
            userService.Registracija();

            string unos2 = "user2" + Environment.NewLine + "ista123" + Environment.NewLine;
            Console.SetIn(new StringReader(unos2));
            userService.Registracija();

            // Oba korisnika trebaju imati isti hash
            var korisnici = userRepo.GetAll();
            Assert.AreEqual(korisnici[0].getLozinka(), korisnici[1].getLozinka());
        }
    }
}