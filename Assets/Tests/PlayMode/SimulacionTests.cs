using System.Collections;
using NUnit.Framework;
using SimulacionSemaforo.Logica;
using SimulacionSemaforo.Simulacion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Sim = SimulacionSemaforo.Simulacion.Simulacion;

namespace SimulacionSemaforo.Tests
{
    /// <summary>Corre la escena real con el tráfico controlado por la prueba.</summary>
    public class SimulacionTests
    {
        private Sim _sim;

        [UnitySetUp]
        public IEnumerator Cargar()
        {
            yield return SceneManager.LoadSceneAsync("Interseccion");
            _sim = Sim.Instancia;
            _sim.ViaA.SinLlegadas();
            _sim.ViaB.SinLlegadas();
            yield return null;
        }

        /// <summary>Semáforo en verde para B (A en rojo) durante 38 s.</summary>
        private void VerdeParaB()
        {
            var s = new Semaforo(new Parametros(), () => (0, 20));
            s.Avanzar(12f + 3f + 2f + 0.1f);
            _sim.UsarSemaforo(s);
        }

        private static IEnumerator Esperar(float segundos)
        {
            float fin = Time.time + segundos;
            while (Time.time < fin) yield return null;
        }

        [UnityTest]
        public IEnumerator ConRojoElAutoSeDetieneAntesDeLaLinea()
        {
            VerdeParaB();
            var auto = _sim.ViaA.Agregar(_sim.ViaA.LineaDePare - 35f, Vehiculo.VelocidadMaxima);
            yield return Esperar(6f);

            float frente = auto.Recorrido + auto.Largo / 2;
            Assert.IsTrue(auto.Detenido, $"velocidad {auto.Velocidad}");
            Assert.LessOrEqual(frente, _sim.ViaA.LineaDePare + 0.2f);
            Assert.Greater(frente, _sim.ViaA.LineaDePare - 1.5f, "frena demasiado lejos de la línea");
            Assert.AreEqual(1, _sim.ViaA.Cola());
        }

        [UnityTest]
        public IEnumerator ConVerdeElAutoCruzaSinFrenar()
        {
            VerdeParaB();
            var auto = _sim.ViaB.Agregar(_sim.ViaB.LineaDePare - 30f, Vehiculo.VelocidadMaxima);
            yield return Esperar(4f);
            Assert.Greater(auto.Recorrido, _sim.ViaB.LineaDePare + 5f);
            Assert.AreEqual(0, _sim.ViaB.Cola());
        }

        [UnityTest]
        public IEnumerator LosAutosSeDetienenEnFilaSinChocar()
        {
            VerdeParaB();
            var primero = _sim.ViaA.Agregar(_sim.ViaA.LineaDePare - 20f, 8f);
            var segundo = _sim.ViaA.Agregar(_sim.ViaA.LineaDePare - 35f, Vehiculo.VelocidadMaxima);
            var tercero = _sim.ViaA.Agregar(_sim.ViaA.LineaDePare - 50f, Vehiculo.VelocidadMaxima);
            yield return Esperar(8f);

            foreach (var (atras, adelante) in new[] { (segundo, primero), (tercero, segundo) })
            {
                float hueco = adelante.Recorrido - adelante.Largo / 2 - (atras.Recorrido + atras.Largo / 2);
                Assert.That(hueco, Is.InRange(1.5f, 4f), "separación entre parachoques");
            }
            Assert.AreEqual(3, _sim.ViaA.Cola());
        }

        [UnityTest]
        public IEnumerator LosPostesMuestranLaLuzDelSemaforo()
        {
            VerdeParaB();
            yield return null;
            Assert.AreEqual(Luz.Roja, _sim.LuzDe(Carril.A));
            Assert.AreEqual(Luz.Verde, _sim.LuzDe(Carril.B));
            Assert.IsNotNull(GameObject.Find("Semáforo A"));
            Assert.IsNotNull(GameObject.Find("Semáforo B"));
        }

        [UnityTest]
        public IEnumerator ElCicloSiguienteUsaLasColasContadas()
        {
            // A en rojo con tres autos esperando y B vacío: el ciclo siguiente da más verde a A.
            var s = new Semaforo(new Parametros(), () => (_sim.ViaA.Cola(), _sim.ViaB.Cola()));
            s.Avanzar(25f + 3f + 2f + 0.1f);
            _sim.UsarSemaforo(s);
            for (int i = 0; i < 3; i++) _sim.ViaA.Agregar(_sim.ViaA.LineaDePare - 10f - i * 7f, 0f);
            yield return null;

            s.Avanzar(s.Restante + 3f + 2f + 0.01f);
            Assert.AreEqual(Fase.VerdeA, s.Fase);
            Assert.AreEqual(38f, s.Plan.VerdeA);
        }
    }
}
