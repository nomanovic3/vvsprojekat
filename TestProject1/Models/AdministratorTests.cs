using FilmMate.Data;
using FilmMate.Models;
using FilmMate.Services;
using Moq;
using System.Text;

[TestClass]
public class AdministratorTests
{
    private Mock<IFilmRepository> mockRepo;
    private FilmService filmService;
    private Administrator admin;
    private StringBuilder sb;

    [TestInitialize]
    public void SetUp()
    {
        mockRepo = new Mock<IFilmRepository>();

        mockRepo.Setup(r => r.GetAll()).Returns(new List<Film>());

        filmService = new FilmService(mockRepo.Object);
        admin = new Administrator();

        sb = new StringBuilder();
        Console.SetOut(new StringWriter(sb));
    }

    [TestCleanup]
    public void TearDown()
    {
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
    }

    [TestMethod]
    public void DodajFilm_PozivaRepoSacuvaj()
    {
        // pripremimo ulaz koji vodi do repo.Sacuvaj()
        string input =
            "NoviFilm" + Environment.NewLine +   // Naziv
            "Akcija" + Environment.NewLine +     // Kategorija
            "8" + Environment.NewLine +          // Ocjena
            "2020" + Environment.NewLine;        // Godina

        Console.SetIn(new StringReader(input));

        // Act
        admin.dodajFilm(filmService);

        // Assert
        mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
    }
    [TestMethod]
    public void ObrisiFilm_PozivaRepoSacuvaj_SaOcjenama()
    {
        // Pripremamo film sa ocjenama da forsiramo dvostruku potvrdu
        var filmSaOcjenom = new Film("Avatar", "Sci-Fi", 8, 2009);
        filmSaOcjenom.DodajOcjenu(10); // Dodajemo ocjenu, sada getOcjene().Count > 0
        var list = new List<Film> { filmSaOcjenom };
        mockRepo.Setup(r => r.GetAll()).Returns(list);

        // KORISTITE Environment.NewLine za ispravan rad Console.ReadLine()
        string input =
            "Avatar" + Environment.NewLine +  // 1. Naziv filma
            "d" + Environment.NewLine +        // 2. Opšta potvrda (d)
            "OBRISI" + Environment.NewLine;    // 3. Finalna potvrda (OBRISI)

        Console.SetIn(new StringReader(input));

        // Act
        admin.obrisiFilm(filmService);

        // Assert: Sada bi Sacuvaj trebao biti pozvan jednom
        mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
    }

    [TestMethod]
    public void AzurirajFilm_PozivaRepoSacuvaj()
    {
        var list = new List<Film> { new Film("Avatar", "Sci-Fi", 8, 2009) };
        mockRepo.Setup(r => r.GetAll()).Returns(list);

        string input =
            "Avatar\n" +   // trazi ga
            "1\n" +        // azurirati naziv
            "NoviAvatar\n"; // novi naziv

        Console.SetIn(new StringReader(input));

        admin.azurirajFilm(filmService);

        mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
    }
}
