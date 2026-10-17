using System.Collections.Generic;
using SimulacionSemaforo.Logica;
using UnityEngine;

namespace SimulacionSemaforo.Simulacion
{
    /// <summary>Un carril de un solo sentido que cruza la intersección; genera y mueve sus vehículos.</summary>
    public class Via : MonoBehaviour
    {
        public const float Largo = 140f;
        public const float AlcanceCamara = 60f;   // metros antes de la línea que "ve" la cámara

        public Carril Carril { get; private set; }
        public Vector3 Inicio { get; private set; }
        public Vector3 Direccion { get; private set; }
        public float LineaDePare { get; private set; }  // metros desde el inicio
        public float IntervaloMedio { get; set; }        // segundos entre llegadas

        private readonly List<Vehiculo> _vehiculos = new List<Vehiculo>();
        private System.Func<Carril, Luz> _luz;
        private System.Random _azar;
        private float _proximaLlegada;

        public IReadOnlyList<Vehiculo> Vehiculos => _vehiculos;

        public void Iniciar(Carril carril, Vector3 inicio, Vector3 direccion, float lineaDePare, float intervaloMedio,
            System.Func<Carril, Luz> luz, int semilla)
        {
            Carril = carril;
            Inicio = inicio;
            Direccion = direccion.normalized;
            LineaDePare = lineaDePare;
            IntervaloMedio = intervaloMedio;
            _luz = luz;
            _azar = new System.Random(semilla);
            _proximaLlegada = 0.5f;
        }

        public Vector3 Punto(float recorrido) => Inicio + Direccion * recorrido;

        /// <summary>
        /// Vehículos que la cámara del carril cuenta como cola: los que todavía no
        /// pasaron la línea y están a menos de <see cref="AlcanceCamara"/> metros.
        /// </summary>
        public int Cola()
        {
            int cola = 0;
            foreach (var v in _vehiculos)
            {
                float frente = v.Recorrido + v.Largo / 2;
                if (frente <= LineaDePare + 0.2f && LineaDePare - frente <= AlcanceCamara) cola++;
            }
            return cola;
        }

        /// <summary>Con rojo se para; con amarillo, solo si alcanza a frenar antes de la línea.</summary>
        public bool DebeParar(Vehiculo v)
        {
            var luz = _luz(Carril);
            if (luz == Luz.Verde) return false;
            if (luz == Luz.Roja) return true;
            float distancia = LineaDePare - (v.Recorrido + v.Largo / 2);
            return distancia > v.Velocidad * v.Velocidad / (2 * Vehiculo.Frenado);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _proximaLlegada -= dt;
            if (_proximaLlegada <= 0) Llegada();

            for (int i = 0; i < _vehiculos.Count; i++)
                _vehiculos[i].Avanzar(dt, i > 0 ? _vehiculos[i - 1] : null);

            while (_vehiculos.Count > 0 && _vehiculos[0].Recorrido > Largo)
            {
                Destroy(_vehiculos[0].gameObject);
                _vehiculos.RemoveAt(0);
            }
        }

        /// <summary>Llegadas de Poisson: el tiempo entre autos es exponencial.</summary>
        private void Llegada()
        {
            _proximaLlegada = -Mathf.Log(1 - (float)_azar.NextDouble()) * IntervaloMedio;
            var ultimo = _vehiculos.Count > 0 ? _vehiculos[_vehiculos.Count - 1] : null;
            // Si la cola llegó al inicio de la vía, el auto espera a que haya lugar.
            if (ultimo != null && ultimo.Recorrido - ultimo.Largo < 6f) return;

            var tipo = Modelos.Elegir(_azar);
            var vehiculo = Modelos.Crear(tipo, transform, _azar);
            float velocidad = ultimo == null ? Vehiculo.VelocidadMaxima : Mathf.Min(Vehiculo.VelocidadMaxima, ultimo.Velocidad);
            vehiculo.Iniciar(this, tipo.Largo, velocidad);
            _vehiculos.Add(vehiculo);
        }

        /// <summary>Solo para pruebas: pone un vehículo en un punto de la vía.</summary>
        public Vehiculo Agregar(float recorrido, float velocidad)
        {
            var tipo = Modelos.Auto;
            var vehiculo = Modelos.Crear(tipo, transform, _azar);
            vehiculo.Iniciar(this, tipo.Largo, velocidad);
            vehiculo.PonerEn(recorrido);
            int i = _vehiculos.FindIndex(v => v.Recorrido < recorrido);
            if (i < 0) _vehiculos.Add(vehiculo); else _vehiculos.Insert(i, vehiculo);
            return vehiculo;
        }

        /// <summary>Solo para pruebas: deja de generar vehículos.</summary>
        public void SinLlegadas() => _proximaLlegada = float.MaxValue;
    }
}
