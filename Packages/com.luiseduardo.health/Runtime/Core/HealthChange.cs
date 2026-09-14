namespace Game.Health
{
    /// <summary>
    /// Foto do que mudou numa alteracao de vida.
    /// Struct readonly: nao aloca no heap, entao pode disparar todo frame sem pressionar o garbage collector.
    /// </summary>
    public readonly struct HealthChange
    {
        public readonly int Previous;
        public readonly int Current;
        public readonly int Max;

        /// <summary>Negativo = dano. Positivo = cura.</summary>
        public int Delta => Current - Previous;

        public bool IsDamage => Delta < 0;
        public bool IsHeal => Delta > 0;
        public bool IsDead => Current <= 0;

        /// <summary>Vida de 0 a 1. Serve direto para preencher barra de UI.</summary>
        public float Normalized => Max <= 0 ? 0f : (float)Current / Max;

        public HealthChange(int previous, int current, int max)
        {
            Previous = previous;
            Current = current;
            Max = max;
        }
    }
}
