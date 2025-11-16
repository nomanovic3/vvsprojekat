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


[TestFixture]
public class FilmServiceRateTests
{
    private Mock<IFilmRepository> mockRepo;
    private FilmService service;
    private List<Film> lazniFilmovi;
    private StringWriter consoleOutput; // Za hvatanje Console.WriteLine

    [SetUp]
    public void Setup()
    {
        // Inicijalizacija lažnih podataka
        lazniFilmovi = new List<Film>
    {
            
            new Film("Avatar", "Sci-Fi", 8.0, 2009),
      new Film("Titanik", "Romansa", 7.8, 1997)
    };

        mockRepo = new Mock<IFilmRepository>();

       
        mockRepo.Setup(repo => repo.GetAll())
        .Returns(lazniFilmovi);

        service = new FilmService(mockRepo.Object);

        //StringWriter sluzii za hvatanje izlaza
        consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);
    }

    // ciscenje nakon svakog testa -- kao clear nesto
    [TearDown]
    public void TearDown()
    {
        // Vraćamo Console.Out i Console.In na originalni tok
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));
    }

    // Test: Uspješnost ocjenjivanja filma
    [Test]
    public void OcijeniFilmGledaoca_ValidRating_CallsSaveAndUpdatesRating()
    {
        // Simulacija unosa sa konzole
        string unos =
      "Avatar" + Environment.NewLine + 
            "10";

        var stringReader = new StringReader(unos);
        Console.SetIn(stringReader);

     
        service.OcijeniFilmGledaoca();

        // 1. Verifikacija Mocka: da li je Save() pozvan uopste
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once,
      "Repo.Sacuvaj() mora biti pozvan nakon uspješnog ocenjivanja.");

        // 2. Verifikacija logike: da li je ocena filma ažurirana
        var avatarFilm = lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Avatar");

        // Očekivano: (8 + 10) / 2 = 9.0
        Assert.AreEqual(9.0, avatarFilm.getOcjena(), 0.001,
      "Prosječna ocjena se nije ispravno izračunala.");

        StringAssert.Contains(
      consoleOutput.ToString(),
      "Uspješno ste ocijenili film."
    );
    }

    // Test: Sacuvaj() metoda se NE poziva za neispravnu ocenu
    [Test]
    public void OcijeniFilmGledaoca_InvalidRating_DoesNotCallSave()
    {
        string unos =
      "Titanik" + Environment.NewLine +
            "12";                            

        var stringReader = new StringReader(unos);
        Console.SetIn(stringReader);

       
        service.OcijeniFilmGledaoca();

      

       
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Never,
      "Sacuvaj() se ne smije pozvati ako je ocjena izvan raspona.");

       
        StringAssert.Contains(
      consoleOutput.ToString(),
      "Ocjena mora biti u rasponu od 1 do 10."
    );
    }

    [Test]
    public void DodajFilm_ValidInput_AddsFilmAndCallsSave()
    {
        // Simulacija unosa za novi film
        string unos =
            "Novi Film" + Environment.NewLine + 
            "Akcija" + Environment.NewLine +     
            "9" + Environment.NewLine +         
            "2023";                              

        Console.SetIn(new StringReader(unos));

        service.dodajFilm();

        // 1. Provjera: Da li je film dodan u listu (repo.GetAll())
        Assert.AreEqual(3, lazniFilmovi.Count, "Film treba biti dodat u listu.");

        var noviFilm = lazniFilmovi.Last();
        Assert.AreEqual("Novi Film", noviFilm.getNazivFilma(), "Naziv filma se ne podudara.");

        // 2. Verifikacija Mocka: Sacuvaj() mora biti pozvana
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once,
            "Sacuvaj() mora biti pozvana nakon uspješnog dodavanja filma.");

        StringAssert.Contains(consoleOutput.ToString(), "Film uspješno dodat!");
    }

    [Test]
    public void DodajFilm_ExistingName_DoesNotAddAndReturnsError()
    {
        // Simulacija unosa za film koji već postoji ("Avatar")
        string unos = "Avatar" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.dodajFilm();

        // ASSERT
        // 1. Broj filmova NE smije se promijeniti
        Assert.AreEqual(2, lazniFilmovi.Count, "Novi film ne smije biti dodat.");

        // 2. Verifikacija Mocka: Sacuvaj() NE smije biti pozvan
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Never,
            "Sacuvaj() se ne smije pozvati ako film već postoji.");

        StringAssert.Contains(consoleOutput.ToString(), "Film već postoji!");
    }


    [Test]
    public void ObrisiFilm_ValidNameWithConfirmation_RemovesFilmAndCallsSave()
    {
        string unos ="Titanik\n" +"d\n" +"OBRISI\n";


        Console.SetIn(new StringReader(unos));


        service.obrisiFilm();

        // 1. Broj filmova mora biti smanjen
        Assert.AreEqual(1, lazniFilmovi.Count, "Film 'Titanik' treba biti obrisan.");
        Assert.IsNull(lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Titanik"), "Titanik ne smije biti u listi.");

        // 2. Verifikacija Mocka: Sacuvaj() mora biti pozvan
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once,
            "Sacuvaj() mora biti pozvana nakon uspješnog brisanja.");

        StringAssert.Contains(consoleOutput.ToString(), "Film obrisan!");
    }

    [Test]
    public void ObrisiFilm_FilmNotFound_DoesNotCallSaveAndReturnsError()
    {
        // Simulacija unosa za nepostojeći film
        string unos = "Nepostojeći Film" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

       
        service.obrisiFilm();

        // 1. Broj filmova ostaje isti
        Assert.AreEqual(2, lazniFilmovi.Count, "Nijedan film ne sijme biti obrisan.");

        // 2. Verifikacija Mocka: Sacuvaj() metoda NE smije biti pozvana
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Never,
            "Sacuvaj() se ne smije pozvati ako film nije pronađen.");

        StringAssert.Contains(consoleOutput.ToString(), "Film nije pronađen!");
    }



    [Test]
    public void AzurirajFilm_UpdateNaziv_UpdatesFilmAndCallsSave()
    {
       
        string unos =
            "Avatar" + Environment.NewLine +
            "1" + Environment.NewLine +
            "Novi Avatar Naziv" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

       
        service.azurirajFilm();

       //provjera da li se desilo azuriranje
        var film = lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Novi Avatar Naziv");
        Assert.IsNotNull(film, "Film mora biti pronađen pod novim nazivom.");

        //Sacuvaj() mora biti pozvana
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once,
            "Sacuvaj() mora biti pozvan nakon uspješnog ažuriranja.");

        StringAssert.Contains(consoleOutput.ToString(), "Film ažuriran!");
    }

    [Test]
    public void AzurirajFilm_FilmNotFound_DoesNotCallSaveAndReturnsError()
    {
        //  Simulacija unos - nepostojeći film
        string unos = "Nepostojeći Film" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        
        service.azurirajFilm();

        // Sacuvaj() NE smije biti pozvan
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Never,
            "Sacuvaj() se ne smije pozvati ako film nije pronađen.");

        StringAssert.Contains(consoleOutput.ToString(), "Film nije pronađen!");
    }



    //--------Testiranje metode FiltrirajPretraziFilmove------

    [Test]
    public void FiltrirajPretraziFilmove_SearchByName_ReturnsCorrectFilms()
    {
        string unos = "1" + Environment.NewLine + "Titanik" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Titanik");
    }

    [Test]
    public void FiltrirajPretraziFilmove_FilterByCategory_ReturnsCorrectFilms()
    {
        string unos = "2" + Environment.NewLine + "Sci-Fi" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Avatar");
    }

    [Test]
    public void FiltrirajPretraziFilmove_FiterByMinimum_ReturnsCorrectFilms()
    {
        string unos = "3" + Environment.NewLine + "8" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Avatar");
    }



    //-------Testiranje metode SortirajPoOcjeni---------
    [Test]
    public void SortirajPoOcjeni_Ascending_ShowsSortedFilms()
    {
        service.SortirajPoOcjeni(true);
        string output = consoleOutput.ToString();
        int indexAvatar = output.IndexOf("Avatar");
        int indexTitanik = output.IndexOf("Titanik");
        Assert.IsTrue(indexTitanik < indexAvatar);
    }

    [Test]
    public void SortirajPoOcjeni_Descending_ShowsSortedFilms()
    {
        service.SortirajPoOcjeni(false);
        string output = consoleOutput.ToString();
        int indexAvatar = output.IndexOf("Avatar");
        int indexTitanik = output.IndexOf("Titanik");
        Assert.IsTrue(indexAvatar > indexTitanik);
    }


   //---------Testiranje metode SortirajPoGodini--------
    [Test]
    public void SortirajPoGodini_Ascending_ShowsSortedFilms()
    {
        service.SortirajPoGodini(true);
        string output = consoleOutput.ToString();
        int indexTitanik = output.IndexOf("Titanik");
        int indexAvatar = output.IndexOf("Avatar");
        Assert.IsTrue(indexTitanik < indexAvatar);
    }
    [Test]
    public void SortirajPoGodini_Descending_ShowsSortedFilms()
    {
        service.SortirajPoGodini(false);
        string output = consoleOutput.ToString();
        int indexTitanik = output.IndexOf("Titanik");
        int indexAvatar = output.IndexOf("Avatar");
        Assert.IsTrue(indexAvatar < indexTitanik);
    }

    //-------PrikaziJeidnstveneKategorije--------
    [Test]
    public void PrikaziJedinstveneKategorije_MultipleFilmsWithDuplicateCategories_PrintsOnlyUnique()
    {
        service.PrikaziJedinstveneKategorije();
        string output = consoleOutput.ToString();
        StringAssert.Contains(output, "Sci-Fi");
        StringAssert.Contains(output, "Romansa");
    }


    [Test]
    public void DodajFilm_InvalidNameLength_ReturnsError()
    {
        string unos =
            "a" + Environment.NewLine;

        Console.SetIn(new StringReader(unos));
        service.dodajFilm();

        Assert.AreEqual(2, lazniFilmovi.Count);
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Never);
        StringAssert.Contains(consoleOutput.ToString(), "Naziv mora imati bar 2 karaktera!");
    }

    [Test]
    public void DodajFilm_InvalidRatingFormat_DefaultsToFive()
    {
        string unos =
            "Novi Film 2" + Environment.NewLine +
            "Akcija" + Environment.NewLine +
            "devet" + Environment.NewLine + // Neispravan format
            "2024";

        Console.SetIn(new StringReader(unos));
        service.dodajFilm();

        Assert.AreEqual(3, lazniFilmovi.Count);
        Assert.AreEqual(5.0, lazniFilmovi.Last().getOcjena(), 0.001);
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once);
        StringAssert.Contains(consoleOutput.ToString(), "Neispravan format ocjene, postavljena na 5.");
    }

    [Test]
    public void DodajFilm_InvalidRatingRange_DefaultsToFive()
    {
        string unos =
            "Novi Film 3" + Environment.NewLine +
            "Horor" + Environment.NewLine +
            "15" + Environment.NewLine + // Izvan raspona 1-10
            "2024";

        Console.SetIn(new StringReader(unos));
        service.dodajFilm();

        Assert.AreEqual(3, lazniFilmovi.Count);
        Assert.AreEqual(5.0, lazniFilmovi.Last().getOcjena(), 0.001);
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once);
        StringAssert.Contains(consoleOutput.ToString(), "Neispravna ocjena, postavljena na 5.");
    }


    //---dodani moguci slucajevi kod obrisi film-----

    [Test]
    public void ObrisiFilm_EmptyName_ReturnsError()
    {
        string unos = "" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.obrisiFilm();

        Assert.AreEqual(2, lazniFilmovi.Count);
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Never);
        StringAssert.Contains(consoleOutput.ToString(), "Naziv filma ne može biti prazan.");
    }
    [Test]
    public void ObrisiFilm_CancelConfirmation_DoesNotDeleteAndDoesNotSave()
    {
        string unos =
            "Titanik" + Environment.NewLine +
            "n" + Environment.NewLine; // Otkazivanje opšte potvrde

        Console.SetIn(new StringReader(unos));
        service.obrisiFilm();

        Assert.AreEqual(2, lazniFilmovi.Count);
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Never);
        StringAssert.Contains(consoleOutput.ToString(), "Brisanje otkazano.");
    }


    //dodatni moguci slucajevi kod azuriraj film  
    [Test]
    public void AzurirajFilm_UpdateKategorija_UpdatesFilmAndCallsSave()
    {
        string unos =
            "Titanik" + Environment.NewLine +
            "2" + Environment.NewLine + // Kategorija
            "Drama" + Environment.NewLine;

        Console.SetIn(new StringReader(unos));
        service.azurirajFilm();

        var film = lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Titanik");
        Assert.AreEqual("Drama", film.getKategorija());
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once);
        StringAssert.Contains(consoleOutput.ToString(), "Film ažuriran!");
    }

    [Test]
    public void AzurirajFilm_UpdateGodina_UpdatesFilmAndCallsSave()
    {
        string unos =
            "Avatar" + Environment.NewLine +
            "3" + Environment.NewLine + // Godina
            "2010" + Environment.NewLine;

        Console.SetIn(new StringReader(unos));
        service.azurirajFilm();

        var film = lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Avatar");
        Assert.AreEqual(2010, film.getGodina());
        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Once);
    }




    //------FiltirirajPretraziFilmove dodatni testovi------
    [Test]
    public void FiltrirajPretraziFilmove_NoFilmsFound_PrintsNotFoundMessage()
    {
        string unos = "1" + Environment.NewLine + "Nepostojeci Naziv" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Nema pronađenih filmova.");
    }

    [Test]
    public void FiltrirajPretraziFilmove_InvalidMenuChoice_ReturnsError()
    {
        string unos = "5" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Pogrešan odabir.");
    }

    [Test]
    public void FiltrirajPretraziFilmove_FilterByMinRating_InvalidFormat_ReturnsError()
    {
        string unos = "3" + Environment.NewLine + "neki text" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Neispravan format ocjene.");
    }

    [Test]
    public void FiltrirajPretraziFilmove_FilterByMinRating_OutOfRange_ReturnsError()
    {
        string unos = "3" + Environment.NewLine + "11" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Ocjena mora biti u rasponu 1-10.");
    }


    [Test]
    public void SortirajPoNazivu_Ascending_ShowsSortedFilms()
    {
        service.SortirajPoNazivu(true);
        string output = consoleOutput.ToString();

        // Avatar (A) treba biti pre Titanika (T)
        int indexAvatar = output.IndexOf("Avatar");
        int indexTitanik = output.IndexOf("Titanik");

        Assert.IsTrue(indexAvatar < indexTitanik, "Sortiranje po nazivu (rastuće) je neispravno.");
    }

    [Test]
    public void SortirajPoNazivu_Descending_ShowsSortedFilms()
    {
        service.SortirajPoNazivu(false);
        string output = consoleOutput.ToString();

        int indexAvatar = output.IndexOf("Avatar");
        int indexTitanik = output.IndexOf("Titanik");

        //  assert
        Assert.IsTrue(indexTitanik < indexAvatar, "Sortiranje po nazivu (opadajuće) je neispravno.");
    }


    [Test]
    public void SortirajPoOcjeni_EmptyList_PrintsNoFilmsMessage()
    {
        
        mockRepo.Setup(repo => repo.GetAll()).Returns(new List<Film>());

        service.SortirajPoOcjeni(true);

        StringAssert.Contains(consoleOutput.ToString(), "Lista filmova je prazna ili nije pronađena."); 
    }

    [Test]
    public void ObrisiFilm_WithRatings_SuccessfulFinalConfirmation()
    {
        string ocjenaUnos = "Avatar" + Environment.NewLine + "10";
        Console.SetIn(new StringReader(ocjenaUnos));
        service.OcijeniFilmGledaoca();

        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        string unos =
            "Avatar" + Environment.NewLine +
            "d" + Environment.NewLine +
            "OBRISI" + Environment.NewLine;

        Console.SetIn(new StringReader(unos));
        service.obrisiFilm();

        
        Assert.AreEqual(1, lazniFilmovi.Count); // 2 originalna - 1 obrisan = 1 ostao
        Assert.IsNull(lazniFilmovi.FirstOrDefault(f => f.getNazivFilma() == "Avatar"));

        mockRepo.Verify(repo => repo.Sacuvaj(), Times.Exactly(2), "Sacuvaj mora biti pozvan za brisanje.");
        StringAssert.Contains(consoleOutput.ToString(), "Film obrisan!");
    }



    [Test]
    public void FiltrirajPretraziFilmove_InvalidMenuChoice_ReturnsErrorAndExits()
    {
        string unos = "X" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();
        StringAssert.Contains(consoleOutput.ToString(), "Pogrešan odabir.");
    }

    [Test]
    public void FiltrirajPoKategoriji_EmptyInput_ReturnsError()
    {
        // Simuliramo unos: "2" (za Kategoriju), a zatim prazan unos
        string unos = "2" + Environment.NewLine + "" + Environment.NewLine;
        Console.SetIn(new StringReader(unos));

        service.FiltrirajPretraziFilmove();

        StringAssert.Contains(consoleOutput.ToString(), "Unos ne može biti prazan.");
    }

}