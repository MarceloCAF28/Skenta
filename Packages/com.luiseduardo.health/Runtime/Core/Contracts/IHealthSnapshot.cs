namespace Game.Health
{
    /// <summary>
    /// Contrato para quem salva e carrega o jogo.
    /// O sistema de save so precisa conhecer HealthState, nunca a minha implementacao.
    /// </summary>
    public interface IHealthSnapshot
    {
        HealthState Capture();
        void Restore(HealthState state);
    }
}
