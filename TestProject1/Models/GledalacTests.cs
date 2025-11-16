using Microsoft.VisualStudio.TestTools.UnitTesting;
using FilmMate.Models;
using FilmMate.Services;

namespace TestProject1.Models
{
    [TestClass]
    public class GledalacTests
    {
        private class DummyFilmService : FilmService
        {
            public bool PrikaziPozvano { get; private set; }
            public bool FiltrirajPozvano { get; private set; }

            public DummyFilmService() : base(null) { }

            public override void prikaziFilmove()
            {
                PrikaziPozvano = true;
            }

            public override void FiltrirajPretraziFilmove()
            {
                FiltrirajPozvano = true;
            }
        }

        [TestMethod]
        public void PregledajFilmove_PozivaMetodu()
        {
            var gledalac = new Gledalac();
            var fs = new DummyFilmService();

            gledalac.pregledajFilmove(fs);

            Assert.IsTrue(fs.PrikaziPozvano);
        }

        [TestMethod]
        public void PretragaFilmova_PozivaMetodu()
        {
            var gledalac = new Gledalac();
            var fs = new DummyFilmService();

            gledalac.pretragaFilmova(fs);

            Assert.IsTrue(fs.FiltrirajPozvano);
        }
    }
}
