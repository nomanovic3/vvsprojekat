using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FilmMate.Services;

namespace FilmMate.Models
{
    public class Administrator : Korisnik
    {
        public void dodajFilm(FilmService filmService) => filmService.dodajFilm();
        public void obrisiFilm(FilmService filmService) => filmService.obrisiFilm();
        public void azurirajFilm(FilmService filmService) => filmService.azurirajFilm();
    }
}

