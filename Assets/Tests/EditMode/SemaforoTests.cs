using System;
using NUnit.Framework;
using SimulacionSemaforo.Logica;

namespace SimulacionSemaforo.Tests
{
    public class RepartoTests
    {
        private static readonly Parametros P = new Parametros();

        // Verde total del ciclo: 60 - 2 * (3 + 2) = 50 s.
        [TestCase(7, 3, 35f, 15f)]    // proporcional: 50 * 7/10
        [TestCase(14, 3, 38f, 12f)]   // 50 * 14/17 = 41.2, pero B no baja de 12
        [TestCase(0, 0, 25f, 25f)]    // sin autos: mitad y mitad
        [TestCase(20, 0, 38f, 12f)]   // un carril vacío igual recibe el mínimo
        [TestCase(1, 30, 12f, 38f)]
        public void RepartePorDemanda(int a, int b, float verdeA, float verdeB)
        {
            var plan = Reparto.Planificar(a, b, P);
            Assert.AreEqual(verdeA, plan.VerdeA);
            Assert.AreEqual(verdeB, plan.VerdeB);
            Assert.AreEqual(P.VerdeTotal, plan.VerdeA + plan.VerdeB);
        }

        [Test]
        public void RechazaColasNegativasYCiclosImposibles()
        {
            Assert.Throws<ArgumentException>(() => Reparto.Planificar(-1, 2, P));
            Assert.Throws<ArgumentException>(() => new Parametros(ciclo: 30));
            Assert.Throws<ArgumentException>(() => new Parametros(amarillo: 0));
        }
    }

    public class SemaforoTests
    {
        [Test]
        public void RecorreLasSeisFasesYSumaElCiclo()
        {
            var s = new Semaforo(new Parametros(), () => (5, 5));
            Assert.AreEqual(Fase.VerdeA, s.Fase);
            Assert.AreEqual(25f, s.Restante);

            var esperado = new[] { Fase.AmarilloA, Fase.TodoRojoTrasA, Fase.VerdeB, Fase.AmarilloB, Fase.TodoRojoTrasB, Fase.VerdeA };
            var duraciones = new[] { 25f, 3f, 2f, 25f, 3f, 2f };
            for (int i = 0; i < esperado.Length; i++)
            {
                s.Avanzar(duraciones[i]);
                Assert.AreEqual(esperado[i], s.Fase, $"paso {i}");
            }
            Assert.AreEqual(1, s.Ciclos);
        }

        [Test]
        public void NuncaHayDosVerdesNiUnVerdeContraAmarillo()
        {
            var s = new Semaforo(new Parametros(), () => (7, 2));
            for (int i = 0; i < 2000; i++)
            {
                s.Avanzar(0.1f);
                var a = s.LuzDe(Carril.A);
                var b = s.LuzDe(Carril.B);
                Assert.IsTrue(a == Luz.Roja || b == Luz.Roja, $"{s.Fase}: A={a} B={b}");
            }
        }

        [Test]
        public void CadaCicloLeeLasColasDeEseMomento()
        {
            var colas = (a: 20, b: 0);
            var s = new Semaforo(new Parametros(), () => colas);
            Assert.AreEqual(38f, s.Plan.VerdeA);

            colas = (0, 20);
            s.Avanzar(60f);
            Assert.AreEqual(Fase.VerdeA, s.Fase);
            Assert.AreEqual(12f, s.Plan.VerdeA);
            Assert.AreEqual(38f, s.Plan.VerdeB);
        }

        [Test]
        public void UnPasoGrandeCruzaVariasFases()
        {
            var s = new Semaforo(new Parametros(), () => (5, 5));
            s.Avanzar(25f + 3f + 2f + 1f);
            Assert.AreEqual(Fase.VerdeB, s.Fase);
            Assert.AreEqual(24f, s.Restante, 1e-4);
        }
    }
}
