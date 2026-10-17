using System;

namespace SimulacionSemaforo.Logica
{
    public enum Carril { A, B }

    /// <summary>
    /// Duraciones fijas del ciclo, en segundos. Es una clase y no un struct: con
    /// un struct, new Parametros() daría todo en cero y las fases no avanzarían.
    /// </summary>
    public sealed class Parametros
    {
        public readonly float Ciclo, VerdeMinimo, Amarillo, TodoRojo;

        public Parametros(float ciclo = 60, float verdeMinimo = 12, float amarillo = 3, float todoRojo = 2)
        {
            if (verdeMinimo <= 0 || amarillo <= 0 || todoRojo < 0)
                throw new ArgumentException("El verde mínimo y el amarillo tienen que durar más de 0 s.");
            if (ciclo - 2 * (amarillo + todoRojo) < 2 * verdeMinimo)
                throw new ArgumentException($"Un ciclo de {ciclo} s no alcanza para dos verdes de {verdeMinimo} s.");
            Ciclo = ciclo;
            VerdeMinimo = verdeMinimo;
            Amarillo = amarillo;
            TodoRojo = todoRojo;
        }

        public float VerdeTotal => Ciclo - 2 * (Amarillo + TodoRojo);
    }

    /// <summary>Segundos de verde de cada carril en un ciclo.</summary>
    public readonly struct Plan
    {
        public readonly float VerdeA, VerdeB;

        public Plan(float verdeA, float verdeB)
        {
            VerdeA = verdeA;
            VerdeB = verdeB;
        }

        public float Verde(Carril carril) => carril == Carril.A ? VerdeA : VerdeB;
    }

    /// <summary>
    /// Reparte el verde en proporción a la cola de cada carril, con un mínimo por
    /// carril. Es la misma regla del programa en Python del proyecto
    /// (ia-practicas, 07-semaforo-inteligente), sin la parte difusa.
    /// </summary>
    public static class Reparto
    {
        public static Plan Planificar(int colaA, int colaB, Parametros p)
        {
            if (colaA < 0 || colaB < 0) throw new ArgumentException("Una cola no puede ser negativa.");
            float total = p.VerdeTotal;
            float proporcion = colaA + colaB == 0 ? 0.5f : (float)colaA / (colaA + colaB);
            float verdeA = MedioSegundo(proporcion * total);
            verdeA = Math.Min(Math.Max(verdeA, p.VerdeMinimo), total - p.VerdeMinimo);
            return new Plan(verdeA, total - verdeA);
        }

        private static float MedioSegundo(float valor) => (float)Math.Round(valor * 2, MidpointRounding.AwayFromZero) / 2;
    }
}
