using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FilmMate.Services;

namespace FilmMate.Models
{
    public class Gost
    {
        public void pregledajFilmove(FilmService fs) => fs.prikaziFilmove();
    }
}

