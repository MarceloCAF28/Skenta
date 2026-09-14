using NUnit.Framework;

namespace Game.Health.Tests
{
    /// <summary>
    /// Testes do nucleo. Rodam no Test Runner em modo Edit, sem entrar em play e sem cena.
    /// Isso so e possivel porque Health nao depende de UnityEngine.
    /// </summary>
    public class HealthTests
    {
        [Test]
        public void NasceComVidaCheia()
        {
            var h = new Health(100);
            Assert.AreEqual(100, h.Current);
            Assert.IsFalse(h.IsDead);
        }

        [Test]
        public void DanoReduzVida()
        {
            var h = new Health(100);
            h.TakeDamage(30);
            Assert.AreEqual(70, h.Current);
        }

        [Test]
        public void VidaNaoFicaNegativa()
        {
            var h = new Health(50);
            h.TakeDamage(999);
            Assert.AreEqual(0, h.Current);
            Assert.IsTrue(h.IsDead);
        }

        [Test]
        public void CuraNaoPassaDoMaximo()
        {
            var h = new Health(100, 90);
            h.Heal(50);
            Assert.AreEqual(100, h.Current);
        }

        [Test]
        public void MorteDisparaUmaVezSo()
        {
            var h = new Health(10);
            int mortes = 0;
            h.Died += () => mortes++;

            h.TakeDamage(10);
            h.TakeDamage(10);

            Assert.AreEqual(1, mortes);
        }

        [Test]
        public void MortoNaoRecebeCura()
        {
            var h = new Health(10);
            h.TakeDamage(10);
            h.Heal(5);
            Assert.AreEqual(0, h.Current);
        }

        [Test]
        public void ReviveVoltaComVidaCheia()
        {
            var h = new Health(10);
            h.TakeDamage(10);
            h.Revive();
            Assert.AreEqual(10, h.Current);
            Assert.IsFalse(h.IsDead);
        }

        [Test]
        public void DanoZeroNaoDisparaEvento()
        {
            var h = new Health(100);
            int chamadas = 0;
            h.Changed += _ => chamadas++;

            h.TakeDamage(0);
            h.Heal(0);

            Assert.AreEqual(0, chamadas);
        }

        [Test]
        public void EventoTrazDeltaCorreto()
        {
            var h = new Health(100);
            HealthChange capturado = default;
            h.Changed += c => capturado = c;

            h.TakeDamage(25);

            Assert.AreEqual(-25, capturado.Delta);
            Assert.AreEqual(0.75f, capturado.Normalized, 0.001f);
            Assert.IsTrue(capturado.IsDamage);
        }

        [Test]
        public void CaptureERestoreMantemEstado()
        {
            var h = new Health(100);
            h.TakeDamage(40);
            var estado = h.Capture();

            var outra = new Health(100);
            outra.Restore(estado);

            Assert.AreEqual(60, outra.Current);
            Assert.AreEqual(100, outra.Max);
        }

        [Test]
        public void RestoreAvisaAUiViaChanged()
        {
            var h = new Health(100);
            int chamadas = 0;
            h.Changed += _ => chamadas++;

            h.Restore(new HealthState(20, 100));

            Assert.AreEqual(1, chamadas);
        }

        [Test]
        public void AumentarMaximoComRatioMantemProporcao()
        {
            var h = new Health(100, 50);
            h.SetMax(200, keepRatio: true);
            Assert.AreEqual(100, h.Current);
        }
    }
}
