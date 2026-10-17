using SimulacionSemaforo.Logica;
using UnityEngine;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>Poste con tres luces que muestra el estado del semáforo para un carril.</summary>
    public class PosteSemaforo : MonoBehaviour
    {
        private static readonly Color Rojo = new Color(0.9f, 0.1f, 0.08f);
        private static readonly Color Amarillo = new Color(1f, 0.7f, 0.05f);
        private static readonly Color Verde = new Color(0.1f, 0.85f, 0.25f);
        private static readonly Color Apagada = new Color(0.12f, 0.12f, 0.12f);

        private Renderer _roja, _amarilla, _verde;
        public Carril Carril { get; private set; }

        public static PosteSemaforo Crear(Transform padre, Carril carril, Vector3 posicion, Vector3 mirarHacia)
        {
            var raiz = new GameObject($"Semáforo {carril}");
            raiz.transform.SetParent(padre, false);
            raiz.transform.position = posicion;
            raiz.transform.rotation = Quaternion.LookRotation(mirarHacia);
            var poste = raiz.AddComponent<PosteSemaforo>();
            poste.Carril = carril;

            var gris = new Color(0.25f, 0.27f, 0.3f);
            Formas.Caja("Poste", raiz.transform, new Vector3(0, 2f, 0), new Vector3(0.18f, 2f, 0.18f), gris, PrimitiveType.Cylinder);
            Formas.Caja("Caja", raiz.transform, new Vector3(0, 4.6f, 0), new Vector3(0.5f, 1.5f, 0.4f), new Color(0.1f, 0.1f, 0.1f));
            poste._roja = Luz(raiz.transform, 5.05f);
            poste._amarilla = Luz(raiz.transform, 4.6f);
            poste._verde = Luz(raiz.transform, 4.15f);
            return poste;
        }

        private static Renderer Luz(Transform padre, float altura) =>
            Formas.Caja("Luz", padre, new Vector3(0, altura, 0.21f), Vector3.one * 0.32f, Apagada, PrimitiveType.Sphere)
                .GetComponent<Renderer>();

        public void Mostrar(Luz luz)
        {
            Formas.Pintar(_roja, luz == Logica.Luz.Roja ? Rojo : Apagada, luz == Logica.Luz.Roja ? 2f : 0);
            Formas.Pintar(_amarilla, luz == Logica.Luz.Amarilla ? Amarillo : Apagada, luz == Logica.Luz.Amarilla ? 2f : 0);
            Formas.Pintar(_verde, luz == Logica.Luz.Verde ? Verde : Apagada, luz == Logica.Luz.Verde ? 2f : 0);
        }
    }
}
