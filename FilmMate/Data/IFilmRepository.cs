using FilmMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmMate.Data
{
    // NOVI FAJL ZA MOCKING
    public interface IFilmRepository
    {
        List<Film> GetAll();
        void Sacuvaj();
    }
}
