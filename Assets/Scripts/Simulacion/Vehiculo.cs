using UnityEngine;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>
    /// Avanza por su vía en línea recta. Frena para no pasar la línea de pare con
    /// rojo (o con amarillo si todavía puede detenerse) y para no alcanzar al
    /// vehículo de adelante.
    /// </summary>
    public class Vehiculo : MonoBehaviour
    {
        public const float VelocidadMaxima = 11f;   // m/s, unos 40 km/h
        public const float Aceleracion = 3f;
        public const float Frenado = 6f;
        public const float Separacion = 2.5f;       // entre parachoques, detenidos

        public Via Via { get; private set; }
        public float Recorrido { get; private set; }  // metros desde el inicio de la vía
        public float Velocidad { get; private set; }
        public float Largo { get; private set; }
        public bool Detenido => Velocidad < 0.3f;

        public void Iniciar(Via via, float largo, float velocidad)
        {
            Via = via;
            Largo = largo;
            Velocidad = velocidad;
            Recorrido = 0;
            Colocar();
        }

        public void PonerEn(float recorrido)
        {
            Recorrido = recorrido;
            Colocar();
        }

        /// <summary>Metros libres por delante antes de tener que estar detenido.</summary>
        public float EspacioLibre(Vehiculo adelante, bool debeParar)
        {
            float libre = float.MaxValue;
            if (adelante != null)
                libre = adelante.Recorrido - adelante.Largo / 2 - (Recorrido + Largo / 2) - Separacion;
            float frente = Recorrido + Largo / 2;
            if (debeParar && frente <= Via.LineaDePare + 0.2f)
                libre = Mathf.Min(libre, Via.LineaDePare - frente);
            return libre;
        }

        public void Avanzar(float dt, Vehiculo adelante)
        {
            bool debeParar = Via.DebeParar(this);
            float libre = Mathf.Max(0, EspacioLibre(adelante, debeParar));
            // Velocidad con la que todavía se puede frenar dentro del espacio libre.
            float limite = Mathf.Min(VelocidadMaxima, Mathf.Sqrt(2 * Frenado * libre));
            Velocidad = Velocidad < limite
                ? Mathf.Min(limite, Velocidad + Aceleracion * dt)
                : Mathf.Max(limite, Velocidad - Frenado * 1.5f * dt);
            Recorrido += Velocidad * dt;
            Colocar();
        }

        private void Colocar()
        {
            transform.position = Via.Punto(Recorrido);
            transform.rotation = Quaternion.LookRotation(Via.Direccion);
        }
    }
}
