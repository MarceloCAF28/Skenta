using UnityEngine;
using UnityEngine.UI;

namespace Game.Health.Demo
{
    /// <summary>
    /// Cola da demo: liga os botoes ao modulo de vida e manda o HUD e o log reagirem.
    /// Nenhuma regra de vida mora aqui. Se esta classe sumir, o modulo continua inteiro.
    /// </summary>
    public class HealthDemoUI : MonoBehaviour
    {
        [SerializeField] HealthComponent alvo;
        [SerializeField] HealthHud hud;
        [SerializeField] ActionLog log;

        [SerializeField] Button botaoDano;
        [SerializeField] Button botaoCura;
        [SerializeField] Button botaoMatar;
        [SerializeField] Button botaoReviver;

        const int Dano = 10;
        const int Cura = 15;

        // Start, nao Awake: garante que o Awake do HealthComponent ja rodou e o Model existe.
        void Start()
        {
            if (alvo == null) { Debug.LogError("HealthDemoUI sem alvo."); return; }

            alvo.Model.Changed += AoMudar;
            alvo.Model.Died += AoMorrer;
            alvo.Model.Revived += AoReviver;

            botaoDano.onClick.AddListener(() => alvo.TakeDamage(Dano));
            botaoCura.onClick.AddListener(() => alvo.Heal(Cura));
            botaoMatar.onClick.AddListener(() => alvo.TakeDamage(9999));
            botaoReviver.onClick.AddListener(() => alvo.Revive());

            hud.Desenhar(alvo.Current, alvo.Max);
            log.Registrar(ActionLog.Tipo.Sistema, $"vida iniciada em {alvo.Current} de {alvo.Max}");
        }

        void OnDestroy()
        {
            if (alvo == null || alvo.Model == null) return;
            alvo.Model.Changed -= AoMudar;
            alvo.Model.Died -= AoMorrer;
            alvo.Model.Revived -= AoReviver;
        }

        void AoMudar(HealthChange c)
        {
            // O HealthComponent emite o estado inicial no Start dele, com delta zero.
            // Se a ordem de Start cair a favor dele, esse evento chega aqui: so redesenha, nao loga.
            if (c.Delta == 0) { hud.Desenhar(c.Current, c.Max); return; }

            if (c.IsDamage)
            {
                hud.AnimarDano(c.Current, c.Max);
                log.Registrar(ActionLog.Tipo.Dano, $"tomou {-c.Delta}, restam {c.Current}");
            }
            else
            {
                hud.AnimarCura(c.Current, c.Max);
                log.Registrar(ActionLog.Tipo.Cura, $"curou {c.Delta}, agora {c.Current}");
            }
        }

        void AoMorrer()
        {
            hud.AnimarMorte(alvo.Max);
            log.Registrar(ActionLog.Tipo.Morte, "vida chegou a zero");
        }

        void AoReviver()
        {
            hud.AnimarRevive(alvo.Current, alvo.Max);
            log.Registrar(ActionLog.Tipo.Revive, $"voltou com {alvo.Current}");
        }
    }
}
