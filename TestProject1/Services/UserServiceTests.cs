using FilmMate.Data;
using FilmMate.Models;
using FilmMate.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace TestProject1.Services
{
    [TestClass]
    public class UserServiceTests
    {
        // Koristimo isti pristup za fajl operacije kao u UserRepositoryTests
        private string originalFilePath = "korisnici.txt";
        private UserRepository userRepo;

        // ============================================
        // POMOĆNE METODE ZA I/O MOCKING I HEŠIRANJE
        // ============================================

        // Privatna metoda za heširanje, identična onoj u UserService
        private string GenerisiHash(string lozinka)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(lozinka));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        // Metoda za prisilno brisanje (preuzeto iz UserRepositoryTests.cs)
        private void ForceDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        File.Delete(filePath);
                        return;
                    }
                    catch (IOException)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return;
                    }
                }
            }
        }

        [TestInitialize]
        public void Setup()
        {
            // Osiguraj čisto stanje prije svakog testa
            ForceDeleteFile(originalFilePath);

            // Kreiraj svjež repozitorij
            userRepo = new UserRepository();
        }

        // ============================================
        // TESTOVI ZA REGISTRACIJU
        // ============================================

        [TestMethod]
        public void Registracija_NewUser_SavesUserAndReturnsSuccess()
        {
            const string testUsername = "novikorisnik";
            const string testPassword = "lozinka123";
            string expectedHash = GenerisiHash(testPassword);

            // 1. Priprema (Simulacija konzole)
            // Postavljamo ulazne podatke
            var input = new StringReader($"{testUsername}{Environment.NewLine}{testPassword}");
            Console.SetIn(input);

            // Postavljamo izlaz za hvatanje poruka
            var output = new StringWriter();
            Console.SetOut(output);

            // 2. Izvršenje
            var service = new UserService(userRepo);
            service.Registracija();

            // 3. Verifikacija
            var allUsers = userRepo.GetAll();

            Assert.AreEqual(1, allUsers.Count, "Treba biti tačno 1 korisnik u repozitoriju.");

            var savedUser = allUsers.First();
            Assert.AreEqual(testUsername, savedUser.getKorisnickoIme(), "Korisničko ime se ne podudara.");
            Assert.AreEqual(expectedHash, savedUser.getLozinka(), "Lozinka nije ispravno heširana.");
            Assert.IsInstanceOfType(savedUser, typeof(Gledalac), "Novi korisnik mora biti Gledalac.");

            string consoleOutput = output.ToString();
            Assert.IsTrue(consoleOutput.Contains("Registracija uspješna!"), "Trebalo bi ispisati poruku o uspješnoj registraciji.");
        }

        [TestMethod]
        public void Registracija_ExistingUser_DoesNotSaveAndReturnsError()
        {
            const string existingUsername = "admin_test";

            // 1. Priprema (Inicijalno stanje)
            var admin = new Administrator();
            admin.setKorisnickoIme(existingUsername);
            admin.setLozinka(GenerisiHash("pass"));
            userRepo.GetAll().Add(admin);
            userRepo.Sacuvaj();

            // 2. Simulacija konzole
            var input = new StringReader($"{existingUsername}{Environment.NewLine}bilo_koja_lozinka");
            Console.SetIn(input);

            var output = new StringWriter();
            Console.SetOut(output);

            // 3. Izvršenje
            var service = new UserService(userRepo);
            service.Registracija();

            // 4. Verifikacija
            Assert.AreEqual(1, userRepo.GetAll().Count, "Broj korisnika se ne smije promijeniti.");

            string consoleOutput = output.ToString();
            Assert.IsTrue(consoleOutput.Contains("Korisnik već postoji!"), "Trebalo bi ispisati poruku da korisnik već postoji.");
        }

        [TestMethod]
        public void Registracija_CaseInsensitiveCheck_ExistingUserIsFound()
        {
            const string existingUsername = "User1";

            // 1. Priprema
            var user = new Gledalac();
            user.setKorisnickoIme(existingUsername);
            user.setLozinka(GenerisiHash("pass"));
            userRepo.GetAll().Add(user);
            userRepo.Sacuvaj();

            // 2. Simulacija konzole: Korisnik unosi 'user1' (sva mala slova)
            var input = new StringReader($"user1{Environment.NewLine}bilo_koja_lozinka");
            Console.SetIn(input);

            var output = new StringWriter();
            Console.SetOut(output);

            // 3. Izvršenje
            var service = new UserService(userRepo);
            service.Registracija();

            // 4. Verifikacija
            Assert.AreEqual(1, userRepo.GetAll().Count, "Broj korisnika se ne smije promijeniti zbog provjere koja ne razlikuje velika/mala slova.");
            string consoleOutput = output.ToString();
            Assert.IsTrue(consoleOutput.Contains("Korisnik već postoji!"), "Provjera bi trebala biti neosjetljiva na veličinu slova.");
        }

        // ============================================
        // TESTOVI ZA PRIJAVU
        // ============================================

        [TestMethod]
        public void Prijava_ValidCredentials_ReturnsKorisnikObject()
        {
            const string validUsername = "ValidUser";
            const string validPassword = "Password1";
            string validHash = GenerisiHash(validPassword);

            // 1. Priprema (Inicijalno stanje)
            var gledalac = new Gledalac();
            gledalac.setKorisnickoIme(validUsername);
            gledalac.setLozinka(validHash);
            userRepo.GetAll().Add(gledalac);
            userRepo.Sacuvaj();

            // 2. Simulacija konzole
            var input = new StringReader($"{validUsername}{Environment.NewLine}{validPassword}");
            Console.SetIn(input);

            var output = new StringWriter();
            Console.SetOut(output);

            // 3. Izvršenje
            var service = new UserService(userRepo);
            Korisnik? result = service.Prijava();

            // 4. Verifikacija
            Assert.IsNotNull(result, "Prijava treba da vrati korisnički objekat.");
            Assert.AreEqual(validUsername, result.getKorisnickoIme());
            Assert.IsInstanceOfType(result, typeof(Gledalac));
            Assert.IsTrue(output.ToString().Contains($"Dobrodošao {validUsername}"), "Trebalo bi ispisati poruku dobrodošlice.");
        }

        [TestMethod]
        public void Prijava_InvalidUsername_ReturnsNull()
        {
            const string validUsername = "ValidUser";

            // 1. Priprema
            var gledalac = new Gledalac();
            gledalac.setKorisnickoIme(validUsername);
            gledalac.setLozinka(GenerisiHash("pass"));
            userRepo.GetAll().Add(gledalac);
            userRepo.Sacuvaj();

            // 2. Simulacija konzole: Pogrešno korisničko ime
            var input = new StringReader($"WrongUser{Environment.NewLine}pass");
            Console.SetIn(input);

            var output = new StringWriter();
            Console.SetOut(output);

            // 3. Izvršenje
            var service = new UserService(userRepo);
            Korisnik? result = service.Prijava();

            // 4. Verifikacija
            Assert.IsNull(result, "Prijava bi trebala biti neuspješna i vratiti null.");
            Assert.IsTrue(output.ToString().Contains("Pogrešno korisničko ime ili lozinka!"), "Trebalo bi ispisati poruku o neuspješnoj prijavi.");
        }

        [TestMethod]
        public void Prijava_InvalidPassword_ReturnsNull()
        {
            const string validUsername = "ValidUser";

            // 1. Priprema
            var gledalac = new Gledalac();
            gledalac.setKorisnickoIme(validUsername);
            gledalac.setLozinka(GenerisiHash("pass"));
            userRepo.GetAll().Add(gledalac);
            userRepo.Sacuvaj();

            // 2. Simulacija konzole: Pogrešna lozinka
            var input = new StringReader($"{validUsername}{Environment.NewLine}wrongpass");
            Console.SetIn(input);

            var output = new StringWriter();
            Console.SetOut(output);

            // 3. Izvršenje
            var service = new UserService(userRepo);
            Korisnik? result = service.Prijava();

            // 4. Verifikacija
            Assert.IsNull(result, "Prijava bi trebala biti neuspješna zbog pogrešne lozinke.");
            Assert.IsTrue(output.ToString().Contains("Pogrešno korisničko ime ili lozinka!"), "Trebalo bi ispisati poruku o neuspješnoj prijavi.");
        }

        [TestMethod]
        public void Prijava_AdminUser_ReturnsAdministratorObject()
        {
            const string adminUsername = "SuperAdmin";
            const string adminPassword = "AdminPass";
            string adminHash = GenerisiHash(adminPassword);

            // 1. Priprema
            var admin = new Administrator();
            admin.setKorisnickoIme(adminUsername);
            admin.setLozinka(adminHash);
            userRepo.GetAll().Add(admin);
            userRepo.Sacuvaj();

            // 2. Simulacija konzole
            var input = new StringReader($"{adminUsername}{Environment.NewLine}{adminPassword}");
            Console.SetIn(input);

            var output = new StringWriter();
            Console.SetOut(output);

            // 3. Izvršenje
            var service = new UserService(userRepo);
            Korisnik? result = service.Prijava();

            // 4. Verifikacija
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Administrator), "Prijava admina treba vratiti Administrator objekat.");
            Assert.IsTrue(output.ToString().Contains($"Dobrodošao {adminUsername}"));
        }

        // ============================================
        // TESTOVI ZA HEŠIRANJE (GenerisiHash)
        // ============================================

        [TestMethod]
        public void GenerisiHash_SamePassword_ReturnsSameHash()
        {
            // Koristimo Reflection da dobijemo pristup privatnoj metodi
            var method = typeof(UserService).GetMethod("GenerisiHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var service = new UserService(userRepo);

            string pass1 = "secretpassword123";
            string pass2 = "secretpassword123";

            string hash1 = (string)method.Invoke(service, new object[] { pass1 })!;
            string hash2 = (string)method.Invoke(service, new object[] { pass2 })!;

            Assert.AreEqual(hash1, hash2, "Ista lozinka mora generisati isti heš.");
            Assert.AreEqual(64, hash1.Length, "SHA256 heš bi trebao imati dužinu 64 znaka.");
        }

        [TestMethod]
        public void GenerisiHash_DifferentPassword_ReturnsDifferentHash()
        {
            var method = typeof(UserService).GetMethod("GenerisiHash", BindingFlags.NonPublic | BindingFlags.Instance);
            var service = new UserService(userRepo);

            string pass1 = "passwordA";
            string pass2 = "passwordB";

            string hash1 = (string)method.Invoke(service, new object[] { pass1 })!;
            string hash2 = (string)method.Invoke(service, new object[] { pass2 })!;

            Assert.AreNotEqual(hash1, hash2, "Različite lozinke moraju generisati različite heševe.");
        }

        // ============================================
        // CLEANUP
        // ============================================

        [TestCleanup]
        public void TestCleanup()
        {
            // Vrati standardni konzolni ulaz i izlaz (neophodno!)
            var standardInput = new StreamReader(Console.OpenStandardInput());
            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;

            Console.SetIn(standardInput);
            Console.SetOut(standardOutput);

            // Cleanup fajla (ako se nije dogodio u Setup/Cleanup Repository testa)
            ForceDeleteFile(originalFilePath);
        }
    }
}