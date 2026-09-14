using System;

namespace Game.Health
{
    /// <summary>
    /// Nucleo da vida. C# puro, sem nenhum using de UnityEngine.
    /// Consequencia pratica: roda em teste automatizado sem abrir o editor e nunca fica preso a um GameObject.
    /// </summary>
    public class Health
    {
        int max;
        int current;

        public int Max => max;
        public int Current => current;
        public bool IsDead => current <= 0;
        public bool IsFull => current >= max;

        /// <summary>Vida de 0 a 1. Serve direto para preencher barra de UI.</summary>
        public float Normalized => max <= 0 ? 0f : (float)current / max;

        /// <summary>Disparado em qualquer alteracao de vida, inclusive a que mata.</summary>
        public event Action<HealthChange> Changed;

        /// <summary>Disparado uma unica vez na transicao de vivo para morto.</summary>
        public event Action Died;

        /// <summary>Disparado na transicao de morto para vivo.</summary>
        public event Action Revived;

        public Health(int max, int current = -1)
        {
            if (max < 1) throw new ArgumentOutOfRangeException(nameof(max), "Vida maxima precisa ser ao menos 1.");
            this.max = max;
            this.current = current < 0 ? max : Clamp(current);
        }

        /// <summary>Aplica dano. Retorna quanto de dano realmente entrou.</summary>
        public int TakeDamage(int amount)
        {
            if (amount <= 0) return 0;
            if (IsDead) return 0;
            return Apply(current - amount);
        }

        /// <summary>Cura. Nao ressuscita: curar quem esta morto nao faz nada, use Revive.</summary>
        public int Heal(int amount)
        {
            if (amount <= 0) return 0;
            if (IsDead) return 0;
            return Apply(current + amount);
        }

        /// <summary>Traz de volta com a vida indicada. Sem argumento, volta com vida cheia.</summary>
        public void Revive(int amount = -1)
        {
            if (!IsDead) return;
            Apply(amount < 0 ? max : amount);
            Revived?.Invoke();
        }

        /// <summary>Muda o teto de vida. Util para item de inventario que aumenta vida maxima.</summary>
        public void SetMax(int newMax, bool keepRatio = false)
        {
            if (newMax < 1) throw new ArgumentOutOfRangeException(nameof(newMax));
            float ratio = Normalized;
            max = newMax;
            Apply(keepRatio ? (int)Math.Round(ratio * newMax) : current);
        }

        /// <summary>Tira a foto do estado para quem salva o jogo.</summary>
        public HealthState Capture() => new HealthState(current, max);

        /// <summary>Recoloca um estado salvo. Dispara Changed, entao a UI se atualiza sozinha ao carregar.</summary>
        public void Restore(HealthState state)
        {
            bool wasDead = IsDead;
            max = Math.Max(1, state.max);
            Apply(state.current);
            if (wasDead && !IsDead) Revived?.Invoke();
        }

        /// <summary>Caminho unico de escrita: tudo passa por aqui, entao nenhum evento pode ser esquecido.</summary>
        int Apply(int value)
        {
            int previous = current;
            current = Clamp(value);
            if (current == previous) return 0;

            Changed?.Invoke(new HealthChange(previous, current, max));

            if (previous > 0 && current <= 0) Died?.Invoke();
            return current - previous;
        }

        int Clamp(int value) => value < 0 ? 0 : (value > max ? max : value);
    }
}
