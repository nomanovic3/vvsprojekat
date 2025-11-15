using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FilmMate.Services;

namespace FilmMate.Models
{
    public class Gledalac : Korisnik
    {
        public void pregledajFilmove(FilmService fs) => fs.prikaziFilmove();
        public void pretragaFilmova(FilmService fs) => fs.FiltrirajPretraziFilmove();
    }
}