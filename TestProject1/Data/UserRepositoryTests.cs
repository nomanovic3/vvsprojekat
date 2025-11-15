using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmMate.Data;
using FilmMate.Models;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TestProject1.Data
{
    [TestClass]
    public class UserRepositoryCompleteTests
    {
        private string testFilePath = "korisnici.txt";
        private UserRepository repo;

        [TestInitialize]
        public void SetUp()
        {
            // Cleanup prije svakog testa
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        [TestCleanup]
        public void TearDown()
        {
            // Cleanup nakon svakog testa
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);
        }

        #region Constructor and Ucitaj Tests

        [TestMethod]
        public void Constructor_FileDoesNotExist_CreatesEmptyList()
        {
            // Ensure file doesn't exist
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);

            repo = new UserRepository();

            Assert.IsNotNull(repo.GetAll());
            Assert.AreEqual(0, repo.GetAll().Count, "Lista korisnika treba biti prazna kada fajl ne postoji.");
        }

        [TestMethod]
        public void Constructor_FileExists_LoadsUsers()
        {
            // Kreiraj test fajl sa podacima
            File.WriteAllLines(testFilePath, new[]
            {
                "admin1;pass1;admin",
                "user1;pass2;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(2, repo.GetAll().Count, "Trebaju biti učitana 2 korisnika.");
        }

        [TestMethod]
        public void Ucitaj_ValidAdminLine_LoadsAdministrator()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "adminUser;hashedPassword;admin"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.IsInstanceOfType(repo.GetAll()[0], typeof(Administrator), "Korisnik treba biti Administrator tip.");
            Assert.AreEqual("adminUser", repo.GetAll()[0].getKorisnickoIme());
            Assert.AreEqual("hashedPassword", repo.GetAll()[0].getLozinka());
        }

        [TestMethod]
        public void Ucitaj_ValidUserLine_LoadsGledalac()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "regularUser;userPass;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.IsInstanceOfType(repo.GetAll()[0], typeof(Gledalac), "Korisnik treba biti Gledalac tip.");
            Assert.AreEqual("regularUser", repo.GetAll()[0].getKorisnickoIme());
            Assert.AreEqual("userPass", repo.GetAll()[0].getLozinka());
        }

        [TestMethod]
        public void Ucitaj_UnknownType_LoadsAsGledalac()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "unknownUser;pass123;unknown"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.IsInstanceOfType(repo.GetAll()[0], typeof(Gledalac), "Nepoznat tip treba biti učitan kao Gledalac.");
        }

        [TestMethod]
        public void Ucitaj_LineWithLessThan3Parts_SkipsLine()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "validUser;validPass;admin",
                "invalidLine;onlyTwoParts",  // Samo 2 dela - trebalo bi biti preskočeno
                "anotherUser;anotherPass;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(2, repo.GetAll().Count, "Linija sa manje od 3 dela treba biti preskočena.");
        }

        [TestMethod]
        public void Ucitaj_LineWithExactly3Parts_LoadsCorrectly()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;admin"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
        }

        [TestMethod]
        public void Ucitaj_LineWithMoreThan3Parts_LoadsFirst3()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;admin;extraData;moreData"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("user1", repo.GetAll()[0].getKorisnickoIme());
            Assert.AreEqual("pass1", repo.GetAll()[0].getLozinka());
            Assert.IsInstanceOfType(repo.GetAll()[0], typeof(Administrator));
        }

        [TestMethod]
        public void Ucitaj_EmptyFile_CreatesEmptyList()
        {
            File.WriteAllText(testFilePath, string.Empty);

            repo = new UserRepository();

            Assert.AreEqual(0, repo.GetAll().Count, "Prazan fajl treba rezultirati praznom listom.");
        }

        [TestMethod]
        public void Ucitaj_FileWithEmptyLines_SkipsEmptyLines()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;admin",
                "",
                "user2;pass2;user",
                "   ",
                "user3;pass3;admin"
            });

            repo = new UserRepository();

            // Prazne linije imaju p.Length < 3, pa se preskaču
            Assert.AreEqual(3, repo.GetAll().Count);
        }

        [TestMethod]
        public void Ucitaj_MultipleAdmins_LoadsAllCorrectly()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "admin1;pass1;admin",
                "admin2;pass2;admin",
                "admin3;pass3;admin"
            });

            repo = new UserRepository();

            Assert.AreEqual(3, repo.GetAll().Count);
            Assert.IsTrue(repo.GetAll().All(k => k is Administrator), "Svi korisnici trebaju biti Administratori.");
        }

        [TestMethod]
        public void Ucitaj_MultipleUsers_LoadsAllCorrectly()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;user",
                "user2;pass2;user",
                "user3;pass3;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(3, repo.GetAll().Count);
            Assert.IsTrue(repo.GetAll().All(k => k is Gledalac), "Svi korisnici trebaju biti Gledaoci.");
        }

        [TestMethod]
        public void Ucitaj_MixedTypes_LoadsAllCorrectly()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "admin1;pass1;admin",
                "user1;pass2;user",
                "admin2;pass3;admin",
                "user2;pass4;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(4, repo.GetAll().Count);
            Assert.AreEqual(2, repo.GetAll().Count(k => k is Administrator), "Trebaju biti 2 administratora.");
            Assert.AreEqual(2, repo.GetAll().Count(k => k is Gledalac), "Trebaju biti 2 gledaoca.");
        }

        [TestMethod]
        public void Ucitaj_SpecialCharactersInUsername_LoadsCorrectly()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user@123;pass123;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("user@123", repo.GetAll()[0].getKorisnickoIme());
        }

        [TestMethod]
        public void Ucitaj_SpecialCharactersInPassword_LoadsCorrectly()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;p@ss!w0rd#123;admin"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("p@ss!w0rd#123", repo.GetAll()[0].getLozinka());
        }

        [TestMethod]
        public void Ucitaj_EmptyUsername_LoadsWithEmptyUsername()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                ";password123;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("", repo.GetAll()[0].getKorisnickoIme());
        }

        [TestMethod]
        public void Ucitaj_EmptyPassword_LoadsWithEmptyPassword()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "username;;admin"
            });

            repo = new UserRepository();

            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("", repo.GetAll()[0].getLozinka());
        }

        [TestMethod]
        public void Ucitaj_CaseSensitiveType_AdminLowercase_LoadsAsAdmin()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;admin"
            });

            repo = new UserRepository();

            Assert.IsInstanceOfType(repo.GetAll()[0], typeof(Administrator));
        }

        [TestMethod]
        public void Ucitaj_CaseSensitiveType_AdminUppercase_LoadsAsGledalac()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;ADMIN"  // Uppercase, neće biti jednako "admin"
            });

            repo = new UserRepository();

            Assert.IsInstanceOfType(repo.GetAll()[0], typeof(Gledalac), "ADMIN uppercase treba biti učitan kao Gledalac.");
        }

        #endregion

        #region Sacuvaj Tests

        [TestMethod]
        public void Sacuvaj_EmptyList_CreatesEmptyFile()
        {
            repo = new UserRepository();
            repo.GetAll().Clear();

            repo.Sacuvaj();

            Assert.IsTrue(File.Exists(testFilePath), "Fajl treba biti kreiran.");
            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(0, lines.Length, "Fajl treba biti prazan.");
        }

        [TestMethod]
        public void Sacuvaj_SingleAdministrator_SavesCorrectly()
        {
            repo = new UserRepository();
            var admin = new Administrator();
            admin.setKorisnickoIme("adminUser");
            admin.setLozinka("adminPass");
            repo.GetAll().Add(admin);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("adminUser;adminPass;admin", lines[0]);
        }

        [TestMethod]
        public void Sacuvaj_SingleGledalac_SavesCorrectly()
        {
            repo = new UserRepository();
            var gledalac = new Gledalac();
            gledalac.setKorisnickoIme("userTest");
            gledalac.setLozinka("userPass");
            repo.GetAll().Add(gledalac);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("userTest;userPass;user", lines[0]);
        }

        [TestMethod]
        public void Sacuvaj_MultipleUsers_SavesAllCorrectly()
        {
            repo = new UserRepository();

            var admin = new Administrator();
            admin.setKorisnickoIme("admin1");
            admin.setLozinka("pass1");

            var user = new Gledalac();
            user.setKorisnickoIme("user1");
            user.setLozinka("pass2");

            repo.GetAll().Add(admin);
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(2, lines.Length);
            Assert.AreEqual("admin1;pass1;admin", lines[0]);
            Assert.AreEqual("user1;pass2;user", lines[1]);
        }

        [TestMethod]
        public void Sacuvaj_OverwritesExistingFile()
        {
            // Kreiraj inicijalni fajl
            File.WriteAllLines(testFilePath, new[] { "oldUser;oldPass;user" });

            repo = new UserRepository();
            repo.GetAll().Clear();

            var newUser = new Gledalac();
            newUser.setKorisnickoIme("newUser");
            newUser.setLozinka("newPass");
            repo.GetAll().Add(newUser);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(1, lines.Length);
            Assert.AreEqual("newUser;newPass;user", lines[0]);
        }

        [TestMethod]
        public void Sacuvaj_SpecialCharactersInUsername_SavesCorrectly()
        {
            repo = new UserRepository();
            var user = new Gledalac();
            user.setKorisnickoIme("user@test#123");
            user.setLozinka("pass");
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual("user@test#123;pass;user", lines[0]);
        }

        [TestMethod]
        public void Sacuvaj_SpecialCharactersInPassword_SavesCorrectly()
        {
            repo = new UserRepository();
            var user = new Gledalac();
            user.setKorisnickoIme("user");
            user.setLozinka("p@ss!w0rd#");
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual("user;p@ss!w0rd#;user", lines[0]);
        }

        [TestMethod]
        public void Sacuvaj_EmptyUsername_SavesCorrectly()
        {
            repo = new UserRepository();
            var user = new Gledalac();
            user.setKorisnickoIme("");
            user.setLozinka("password");
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(";password;user", lines[0]);
        }

        [TestMethod]
        public void Sacuvaj_EmptyPassword_SavesCorrectly()
        {
            repo = new UserRepository();
            var user = new Gledalac();
            user.setKorisnickoIme("username");
            user.setLozinka("");
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual("username;;user", lines[0]);
        }

        [TestMethod]
        public void Sacuvaj_MixedAdminsAndUsers_SavesCorrectTypes()
        {
            repo = new UserRepository();

            var admin1 = new Administrator();
            admin1.setKorisnickoIme("admin1");
            admin1.setLozinka("adminPass1");

            var user1 = new Gledalac();
            user1.setKorisnickoIme("user1");
            user1.setLozinka("userPass1");

            var admin2 = new Administrator();
            admin2.setKorisnickoIme("admin2");
            admin2.setLozinka("adminPass2");

            repo.GetAll().Add(admin1);
            repo.GetAll().Add(user1);
            repo.GetAll().Add(admin2);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(3, lines.Length);
            StringAssert.Contains(lines[0], ";admin");
            StringAssert.Contains(lines[1], ";user");
            StringAssert.Contains(lines[2], ";admin");
        }

        [TestMethod]
        public void Sacuvaj_LargeNumberOfUsers_SavesAllCorrectly()
        {
            repo = new UserRepository();

            for (int i = 0; i < 100; i++)
            {
                var user = new Gledalac();
                user.setKorisnickoIme($"user{i}");
                user.setLozinka($"pass{i}");
                repo.GetAll().Add(user);
            }

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.AreEqual(100, lines.Length);
        }

        #endregion

        #region GetAll Tests

        [TestMethod]
        public void GetAll_ReturnsCorrectList()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;admin",
                "user2;pass2;user"
            });

            repo = new UserRepository();
            var korisnici = repo.GetAll();

            Assert.IsNotNull(korisnici);
            Assert.AreEqual(2, korisnici.Count);
        }

        [TestMethod]
        public void GetAll_ReturnsLiveReference()
        {
            repo = new UserRepository();
            var lista = repo.GetAll();

            var newUser = new Gledalac();
            newUser.setKorisnickoIme("testUser");
            newUser.setLozinka("testPass");
            lista.Add(newUser);

            // GetAll() treba vratiti istu referencu
            Assert.AreEqual(1, repo.GetAll().Count);
        }

        [TestMethod]
        public void GetAll_EmptyRepository_ReturnsEmptyList()
        {
            if (File.Exists(testFilePath))
                File.Delete(testFilePath);

            repo = new UserRepository();
            var lista = repo.GetAll();

            Assert.IsNotNull(lista);
            Assert.AreEqual(0, lista.Count);
        }

        #endregion

        #region Integration Tests (Load -> Modify -> Save -> Load)

        [TestMethod]
        public void Integration_LoadModifySaveLoad_WorksCorrectly()
        {
            // 1. Kreiraj inicijalni fajl
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;user"
            });

            // 2. Učitaj
            repo = new UserRepository();
            Assert.AreEqual(1, repo.GetAll().Count);

            // 3. Dodaj novog korisnika
            var newAdmin = new Administrator();
            newAdmin.setKorisnickoIme("admin1");
            newAdmin.setLozinka("adminPass");
            repo.GetAll().Add(newAdmin);

            // 4. Sačuvaj
            repo.Sacuvaj();

            // 5. Ponovo učitaj iz fajla
            var repo2 = new UserRepository();
            Assert.AreEqual(2, repo2.GetAll().Count);
            Assert.AreEqual(1, repo2.GetAll().Count(k => k is Administrator));
            Assert.AreEqual(1, repo2.GetAll().Count(k => k is Gledalac));
        }

        [TestMethod]
        public void Integration_SaveAndLoadPreservesUserTypes()
        {
            repo = new UserRepository();

            var admin = new Administrator();
            admin.setKorisnickoIme("superAdmin");
            admin.setLozinka("superPass");

            var user = new Gledalac();
            user.setKorisnickoIme("regularUser");
            user.setLozinka("regularPass");

            repo.GetAll().Add(admin);
            repo.GetAll().Add(user);
            repo.Sacuvaj();

            // Učitaj ponovo
            var newRepo = new UserRepository();
            Assert.AreEqual(2, newRepo.GetAll().Count);
            Assert.IsInstanceOfType(newRepo.GetAll()[0], typeof(Administrator));
            Assert.IsInstanceOfType(newRepo.GetAll()[1], typeof(Gledalac));
        }

        [TestMethod]
        public void Integration_SaveAndLoadPreservesCredentials()
        {
            repo = new UserRepository();

            var user = new Gledalac();
            user.setKorisnickoIme("testUser123");
            user.setLozinka("hashedPassword456");
            repo.GetAll().Add(user);
            repo.Sacuvaj();

            // Učitaj ponovo
            var newRepo = new UserRepository();
            var loadedUser = newRepo.GetAll()[0];

            Assert.AreEqual("testUser123", loadedUser.getKorisnickoIme());
            Assert.AreEqual("hashedPassword456", loadedUser.getLozinka());
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Ucitaj_LineWithOnlyDelimiters_SkipsLine()
        {
            File.WriteAllLines(testFilePath, new[]
            {
                "user1;pass1;admin",
                ";;",  // Samo delimiteri
                "user2;pass2;user"
            });

            repo = new UserRepository();

            Assert.AreEqual(2, repo.GetAll().Count);
        }

        [TestMethod]
        public void Ucitaj_LineWithSemicolonInUsername_HandlesCorrectly()
        {
            // Ovo će biti split na više delova zbog semicolona u username
            File.WriteAllLines(testFilePath, new[]
            {
                "user;name;password;admin"  // 4 dela umesto 3
            });

            repo = new UserRepository();

            // p.Length >= 3, pa će se učitati
            Assert.AreEqual(1, repo.GetAll().Count);
            Assert.AreEqual("user", repo.GetAll()[0].getKorisnickoIme());
        }

        [TestMethod]
        public void Sacuvaj_NullUsername_SavesAsEmpty()
        {
            repo = new UserRepository();
            var user = new Gledalac();
            user.setKorisnickoIme(null);
            user.setLozinka("pass");
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            Assert.IsTrue(lines[0].StartsWith(";"));
        }

        [TestMethod]
        public void Sacuvaj_NullPassword_SavesAsEmpty()
        {
            repo = new UserRepository();
            var user = new Gledalac();
            user.setKorisnickoIme("user");
            user.setLozinka(null);
            repo.GetAll().Add(user);

            repo.Sacuvaj();

            var lines = File.ReadAllLines(testFilePath);
            StringAssert.Contains(lines[0], "user;");
        }

        #endregion
    }
}