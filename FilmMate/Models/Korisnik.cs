using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmMate.Models
{
    public abstract class Korisnik
    {
        protected string korisnickoIme;
        protected string lozinka;

        public string getKorisnickoIme() => korisnickoIme;
        public void setKorisnickoIme(string ime) => korisnickoIme = ime;

        public string getLozinka() => lozinka;
        public void setLozinka(string pass) => lozinka = pass;
    }
}

