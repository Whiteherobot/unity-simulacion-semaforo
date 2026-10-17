using System;

namespace SimulacionSemaforo.Logica
{
    public enum Luz { Verde, Amarilla, Roja }

    public enum Fase { VerdeA, AmarilloA, TodoRojoTrasA, VerdeB, AmarilloB, TodoRojoTrasB }

    /// <summary>
    /// Semáforo de dos carriles que se cruzan. Al empezar cada ciclo lee las colas
    /// y reparte el verde con <see cref="Reparto"/>.
    /// </summary>
    public sealed class Semaforo
    {
        private readonly Parametros _parametros;
        private readonly Func<(int a, int b)> _leerColas;

        public Fase Fase { get; private set; }
        public float Restante { get; private set; }
        public Plan Plan { get; private set; }
        public int Ciclos { get; private set; }

        public Semaforo(Parametros parametros, Func<(int a, int b)> leerColas)
        {
            _parametros = parametros;
            _leerColas = leerColas ?? throw new ArgumentNullException(nameof(leerColas));
            EmpezarCiclo();
        }

        public Luz LuzDe(Carril carril)
        {
            bool esA = carril == Carril.A;
            switch (Fase)
            {
                case Fase.VerdeA: return esA ? Luz.Verde : Luz.Roja;
                case Fase.AmarilloA: return esA ? Luz.Amarilla : Luz.Roja;
                case Fase.VerdeB: return esA ? Luz.Roja : Luz.Verde;
                case Fase.AmarilloB: return esA ? Luz.Roja : Luz.Amarilla;
                default: return Luz.Roja;
            }
        }

        /// <summary>Avanza el reloj; puede cruzar varias fases si dt es grande.</summary>
        public void Avanzar(float dt)
        {
            if (dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
            Restante -= dt;
            while (Restante <= 0)
            {
                float sobrante = -Restante;
                Siguiente();
                Restante -= sobrante;
            }
        }

        private void Siguiente()
        {
            switch (Fase)
            {
                case Fase.VerdeA: Entrar(Fase.AmarilloA, _parametros.Amarillo); break;
                case Fase.AmarilloA: Entrar(Fase.TodoRojoTrasA, _parametros.TodoRojo); break;
                case Fase.TodoRojoTrasA: Entrar(Fase.VerdeB, Plan.VerdeB); break;
                case Fase.VerdeB: Entrar(Fase.AmarilloB, _parametros.Amarillo); break;
                case Fase.AmarilloB: Entrar(Fase.TodoRojoTrasB, _parametros.TodoRojo); break;
                default:
                    Ciclos++;
                    EmpezarCiclo();
                    break;
            }
        }

        private void EmpezarCiclo()
        {
            var (a, b) = _leerColas();
            Plan = Reparto.Planificar(a, b, _parametros);
            Entrar(Fase.VerdeA, Plan.VerdeA);
        }

        private void Entrar(Fase fase, float duracion)
        {
            Fase = fase;
            Restante = duracion;
        }
    }
}
