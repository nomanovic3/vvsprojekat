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
            "NoviFilm\n" +   // Naziv
            "Akcija\n" +     // Kategorija
            "8\n" +          // Ocjena
            "2020\n";        // Godina

        Console.SetIn(new StringReader(input));

        // Act
        admin.dodajFilm(filmService);

        // Assert
        mockRepo.Verify(r => r.Sacuvaj(), Times.Once);
    }

    [TestMethod]
    public void ObrisiFilm_PozivaRepoSacuvaj()
    {
        // Repozitorij sa jednim filmom
        var list = new List<Film> { new Film("Avatar", "Sci-Fi", 8, 2009) };
        mockRepo.Setup(r => r.GetAll()).Returns(list);

        string input =
            "Avatar\n" +   // naziv
            "d\n" +        // potvrda
            "OBRISI\n";    // finalna potvrda

        Console.SetIn(new StringReader(input));

        admin.obrisiFilm(filmService);

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
