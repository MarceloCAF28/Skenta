namespace Game.Health
{
    /// <summary>
    /// Quem causa dano fala com esta interface, nunca com o HealthComponent direto.
    /// Assim uma armadilha, um inimigo ou um item do inventario nao precisam conhecer meu codigo.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
        bool IsDead { get; }
    }
}
