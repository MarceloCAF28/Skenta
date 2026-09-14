using UnityEngine;

namespace Game.Health
{
    /// <summary>
    /// Adaptador entre o nucleo Health e o mundo do Unity.
    /// Ele nao contem regra de vida nenhuma: so traduz eventos C# para eventos de Inspector e para o canal.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game/Health/Health Component")]
    public class HealthComponent : MonoBehaviour, IDamageable, IHealable, IHealthSnapshot
    {
        [Header("Configuracao")]
        [SerializeField, Min(1)] int maxHealth = 100;
        [SerializeField, Tooltip("Desmarque para comecar a cena com vida diferente da maxima.")]
        bool startAtFullHealth = true;
        [SerializeField, Min(0)] int startingHealth = 100;

        [Header("Canal opcional")]
        [SerializeField, Tooltip("Arraste um HealthEventChannel para avisar sistemas que nao conhecem este objeto.")]
        HealthEventChannel channel;

        [Header("Eventos do Inspector")]
        [Tooltip("Parametros: vida atual, vida maxima.")]
        public HealthChangedEvent onChanged;
        [Tooltip("Parametro: vida de 0 a 1. Ligue direto no fillAmount de uma Image.")]
        public HealthNormalizedEvent onNormalizedChanged;
        public UnityEngine.Events.UnityEvent onDamaged;
        public UnityEngine.Events.UnityEvent onHealed;
        public UnityEngine.Events.UnityEvent onDied;
        public UnityEngine.Events.UnityEvent onRevived;

        Health health;

        /// <summary>Acesso ao nucleo para quem preferir assinar evento C# em vez de UnityEvent.</summary>
        public Health Model => health;

        public int Current => health?.Current ?? 0;
        public int Max => health?.Max ?? maxHealth;
        public bool IsDead => health != null && health.IsDead;

        void Awake()
        {
            int start = startAtFullHealth ? maxHealth : startingHealth;
            health = new Health(maxHealth, start);

            health.Changed += HandleChanged;
            health.Died += HandleDied;
            health.Revived += HandleRevived;
        }

        void Start()
        {
            // Emite o estado inicial depois que todo mundo ja assinou, senao a barra de vida nasce vazia.
            HandleChanged(new HealthChange(health.Current, health.Current, health.Max));
        }

        void OnDestroy()
        {
            if (health == null) return;
            health.Changed -= HandleChanged;
            health.Died -= HandleDied;
            health.Revived -= HandleRevived;
        }

        public void TakeDamage(int amount) => health?.TakeDamage(amount);
        public void Heal(int amount) => health?.Heal(amount);
        public void Revive(int amount = -1) => health?.Revive(amount);
        public void SetMaxHealth(int newMax, bool keepRatio = false) => health?.SetMax(newMax, keepRatio);

        public HealthState Capture() => health?.Capture() ?? new HealthState(maxHealth, maxHealth);

        public void Restore(HealthState state)
        {
            if (health == null) health = new Health(Mathf.Max(1, state.max), state.current);
            else health.Restore(state);
        }

        void HandleChanged(HealthChange change)
        {
            onChanged?.Invoke(change.Current, change.Max);
            onNormalizedChanged?.Invoke(change.Normalized);
            if (change.IsDamage) onDamaged?.Invoke();
            if (change.IsHeal) onHealed?.Invoke();
            if (channel != null) channel.RaiseChanged(gameObject, change);
        }

        void HandleDied()
        {
            onDied?.Invoke();
            if (channel != null) channel.RaiseDied(gameObject);
        }

        void HandleRevived()
        {
            onRevived?.Invoke();
            if (channel != null) channel.RaiseRevived(gameObject);
        }

        void OnValidate()
        {
            if (startingHealth > maxHealth) startingHealth = maxHealth;
        }
    }
}
