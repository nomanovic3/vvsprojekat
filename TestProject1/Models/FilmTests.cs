using FilmMate.Models; 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq; 
using TestProject1.TestData;

namespace FilmMate.Tests.Models
{
    [TestClass]
    public class FilmTests
    {

        // Data Driven Test: Testiranje izračuna prosječne ocjene
        [DataRow(new int[] { 8, 9, 10 }, 9.0, "triOcjene_8_9_10")]
        [DataRow(new int[] { 5, 5, 5, 5, 5 }, 5.0, "petIstihOcjena")]
        [DataRow(new int[] { 1, 10 }, 5.5, "minIMaxVrijednosti")]
        [DataRow(new int[] { }, 0.0, "praznaListaOcjena")]
        [DataTestMethod]
        public void GetOcjena_VariousRatingInputs_ShouldReturnCorrectAverage(int[] nizOcjena, double ocekivaniProsjek, string opis)
        {

            var film = new Film("Test Film", "Action", 5.0, 2020);

            // ocisti postojece ocjene prije dodavanja novih za test
            film.getOcjene().Clear();

            foreach (var rating in nizOcjena)
            {
                film.DodajOcjenu(rating);
            }

            double stvarniAverage = film.getOcjena();

            Assert.AreEqual(ocekivaniProsjek, stvarniAverage, 0.001, $"Razlog pada testa: {opis}");
        }


        // Unit Test: Testiranje graničnih vrijednosti za DodajOcjenu
        [TestMethod]
        public void DodajOcjenu_InvalidInputs_ShouldIgnoreRating()
        {

            var film = new Film("Validation Test", "Sci-Fi", 8.0, 2024);

            // Konstruktor postvlja 1 ocjenu   
            int initialRatingCount = film.getOcjene().Count;

            // pokušavamo ubaciti nevalidne ocjene
            film.DodajOcjenu(0);
            film.DodajOcjenu(11);

            // Assert: count i average ne smiju se pormijeniti
            Assert.AreEqual(initialRatingCount, film.getOcjene().Count, "Unešena je nevalidna ocjena");
            Assert.AreEqual(8.0, film.getOcjena(), 0.001, "Prosječna ocjena ne smije biti izmijenjena unosom nevalidnih ocjena");
        }


        [TestMethod]
        [DynamicData(nameof(GetCsvTestData), DynamicDataSourceType.Method)]
        public void SetteriIToString_PromjenaAtributa_TrebaVratitiIspravanToString(FilmTestData testData)
        {
            //  Kreiranje filma sa početnim podacima
            // Konstruktor postavlja jednu ocjenu (5.0)
            var film = new Film(testData.PocetniNaziv, "Drama", 5.0, 2020);

            //  Korištenje settera
            film.setNaziv(testData.NoviNaziv);
            film.setKategorija(testData.NovaKategorija);
            film.setGodina(testData.NovaGodina);

            // za testiranje ToString()  metode
            string stvarniToString = film.ToString();

            // je li formatirani string ispravan
            Assert.AreEqual(testData.OcekivaniToString, stvarniToString,
                             $"ToString() nije ispravan nakon settera.");

            // Dodatna provjera: Da li je setter za Godinu zaista promijenio vrijednost
            Assert.AreEqual(testData.NovaGodina, film.getGodina(), "Setter za Godinu nije radio.");
        }
        public static IEnumerable<object[]> GetCsvTestData()
        {
           
            return CsvDataLoader.GetTestData<FilmTestData>("TestData/TestData.csv");
        }

    }
}