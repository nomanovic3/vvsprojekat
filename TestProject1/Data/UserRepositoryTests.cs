using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmMate.Data;
using FilmMate.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading; // Dodato za Thread.Sleep

namespace TestProject1.Data
{
    [TestClass]
    public class UserRepositoryTests
    {
        private string originalFilePath = "korisnici.txt";
        private string backupFilePath = "korisnici_backup.txt";

        // ============================================
        // ROBUSTNA METODA ZA BRISANJE FAJLA
        // ============================================
        // Ova metoda je ključna za rješavanje nepredvidivih IOException
        private void ForceDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        File.Delete(filePath);
                        return; // Uspješno obrisano
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(50); // Kratka pauza
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
            // 1. Osiguraj da originalni fajl ne ostane zaključan.
            ForceDeleteFile(originalFilePath);

            // 2. Backup originalnog fajla ako je postojao prije testova.
            if (File.Exists(originalFilePath))
            {
                File.Copy(originalFilePath, backupFilePath, true);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            // 1. **AGRESIVNO BRISANJE:** Obriši fajl nakon testa.
            ForceDeleteFile(originalFilePath);

            // 2. Restore originalnog fajla iz backupa
            if (File.Exists(backupFilePath))
            {
                try
                {
                    File.Copy(backupFilePath, originalFilePath, true);
                    ForceDeleteFile(backupFilePath);
                }
                catch { }
            }

            // 3. Očisti sve privremene fajlove
            var testFiles = Directory.GetFiles(".", "test_korisnici_*.txt");
            foreach (var file in testFiles)
            {
                ForceDeleteFile(file);
            }
        }

        private void SetPrivateFilePath(UserRepository repo, string newPath)
        {
            var field = typeof(UserRepository).GetField("filePath",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(repo, newPath);
        }

        private void InvokePrivateUcitaj(UserRepository repo)
        {
            var method = typeof(UserRepository).GetMethod("Ucitaj",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(repo, null);
        }

        // ============================================
        // TEST 1: Konstruktor - Fajl ne postoji
        // ============================================
        [TestMethod]
        public void Constructor_FileDoesNotExist_CreatesEmptyList()
        {
            ForceDeleteFile(originalFilePath);

            var repo = new UserRepository();

            Assert.IsNotNull(repo);
            Assert.AreEqual(0, repo.GetAll().Count, "Lista korisnika treba biti prazna kad fajl ne postoji.");
        }

        // ============================================
        // TEST 2: Konstruktor - Prazan fajl
        // ============================================
        [TestMethod]
        public void Constructor_EmptyFile_CreatesEmptyList()
        {
            File.WriteAllText(originalFilePath, "");

            var repo = new UserRepository();

            Assert.AreEqual(0, repo.GetAll().Count, "Prazan fajl treba rezultovati praznom listom.");
        }

        // ============================================
        // TEST 4: Ucitaj - Linija sa manje od 3 polja (preskače)
        // ============================================
        [TestMethod]
        public void Ucitaj_InvalidLineWithLessThan3Fields_SkipsLine()
        {
            var lines = new List<string>
            {
                "validuser;validpass;user",
                "invalidline;onlytwovalues",
                "admin2;adminpass2;admin"
            };
            File.WriteAllLines(originalFilePath, lines);

            var repo = new UserRepository();

            Assert.AreEqual(2, repo.GetAll().Count, "Trebalo bi učitati samo 2 validna korisnika.");
            Assert.AreEqual("validuser", repo.GetAll()[0].getKorisnickoIme());
            Assert.AreEqual("admin2", repo.GetAll()[1].getKorisnickoIme());
        }

        // ============================================
        // TEST 5: Ucitaj - Linija sa tačno 3 polja
        // ============================================
        [TestMethod]
        public void Ucitaj_LineWithExactly3Fields_Loads()
        {
            File.WriteAllText(originalFilePath, "user1;pass1;user");

            var repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("user1", repo.GetAll()[0].getKorisnickoIme());
            Assert.AreEqual("pass1", repo.GetAll()[0].getLozinka());
        }

        // ============================================
        // TEST 11: GetAll - Prazan repozitorij
        // ============================================
        [TestMethod]
        public void GetAll_EmptyRepository_ReturnsEmptyList()
        {
            ForceDeleteFile(originalFilePath);

            var repo = new UserRepository();

            Assert.AreEqual(0, repo.GetAll().Count);
            Assert.IsNotNull(repo.GetAll());
        }

        // ============================================
        // TEST 12: Sacuvaj - Čuva Administrator kao "admin"
        // ============================================
        [TestMethod]
        public void Sacuvaj_Administrator_SavesAsAdmin()
        {
            // OVDJE JE KRITIČNO: ForceDeleteFile se poziva preko Setup, 
            // ali pozivom ovdje osiguravamo da je fajl slobodan neposredno prije
            // File.WriteAllLines u Sacuvaj() metodi.
            ForceDeleteFile(originalFilePath);

            var repo = new UserRepository();

            var admin = new Administrator();
            admin.setKorisnickoIme("admintest");
            admin.setLozinka("adminpass");
            repo.GetAll().Add(admin);

            repo.Sacuvaj(); // Ova linija baca izuzetak

            var lines = File.ReadAllLines(originalFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("admintest;adminpass;admin", lines[0]);
        }

        // ============================================
        // TEST 13: Sacuvaj - Čuva Gledalac kao "user"
        // ============================================
        [TestMethod]
        public void Sacuvaj_Gledalac_SavesAsUser()
        {
            ForceDeleteFile(originalFilePath);

            var repo = new UserRepository();

            var gledalac = new Gledalac();
            gledalac.setKorisnickoIme("viewertest");
            gledalac.setLozinka("viewerpass");
            repo.GetAll().Add(gledalac);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(originalFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("viewertest;viewerpass;user", lines[0]);
        }

        // ============================================
        // TEST 16: Sacuvaj pa Ucitaj - Persistence
        // ============================================
        [TestMethod]
        public void Sacuvaj_ThenUcitaj_DataPersists()
        {
            ForceDeleteFile(originalFilePath);

            var repo1 = new UserRepository();
            var admin = new Administrator();
            admin.setKorisnickoIme("persistadmin");
            admin.setLozinka("persistpass");
            repo1.GetAll().Add(admin);
            repo1.Sacuvaj();

            var repo2 = new UserRepository();

            Assert.AreEqual(1, repo2.GetAll().Count);
            Assert.IsInstanceOfType(repo2.GetAll()[0], typeof(Administrator));
            Assert.AreEqual("persistadmin", repo2.GetAll()[0].getKorisnickoIme());
            Assert.AreEqual("persistpass", repo2.GetAll()[0].getLozinka());
        }

        // ============================================
        // TEST 17: Sacuvaj - Prepisuje postojeći fajl
        // ============================================
        [TestMethod]
        public void Sacuvaj_ExistingFile_Overwrites()
        {
            File.WriteAllText(originalFilePath, "olduser;oldpass;user");

            var repo = new UserRepository();
            repo.GetAll().Clear();

            var newUser = new Gledalac();
            newUser.setKorisnickoIme("newuser");
            newUser.setLozinka("newpass");
            repo.GetAll().Add(newUser);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(originalFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("newuser;newpass;user", lines[0]);
        }

        // ============================================
        // TEST 19: GetAll - Vraća istu referencu
        // ============================================
        [TestMethod]
        public void GetAll_ReturnsSameListReference()
        {
            ForceDeleteFile(originalFilePath);

            var repo = new UserRepository();

            var list1 = repo.GetAll();
            var list2 = repo.GetAll();

            Assert.AreSame(list1, list2);
        }

        // ============================================
        // TEST 22: Sacuvaj - Veliki broj korisnika
        // ============================================
        [TestMethod]
        public void Sacuvaj_MultipleUsers_SavesAll()
        {
            ForceDeleteFile(originalFilePath);

            var repo = new UserRepository();

            for (int i = 1; i <= 10; i++)
            {
                var user = new Gledalac();
                user.setKorisnickoIme($"user{i}");
                user.setLozinka($"pass{i}");
                repo.GetAll().Add(user);
            }

            repo.Sacuvaj();

            var lines = File.ReadAllLines(originalFilePath);
            Assert.AreEqual(10, lines.Length);
        }
    }
}