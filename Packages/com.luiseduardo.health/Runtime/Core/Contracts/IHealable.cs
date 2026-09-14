namespace Game.Health
{
    /// <summary>Quem cura fala por aqui. Tipico caso da pocao vinda do inventario.</summary>
    public interface IHealable
    {
        void Heal(int amount);
    }
}
