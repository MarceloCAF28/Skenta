using UnityEngine;
using Game.Health;

/// <summary>
/// Liga o barramento de eventos do projeto ao HUD de vida.
///
/// O ponto principal: este componente escuta <see cref="EventManager.OnHealthChanged"/>,
/// nao o jogador. O HUD nunca tem referencia ao Player, nao procura ele na cena e nao
/// sabe que ele existe. Se amanha a vida passar a ser de um chefe, de um veiculo ou de
/// uma porta destrutivel, o HUD continua funcionando sem alteracao.
///
/// Quem decide o que e dano e o que e cura e aqui, comparando com o valor anterior,
/// porque o evento do projeto carrega apenas vida atual e maxima.
/// </summary>
[RequireComponent(typeof(HealthHud))]
public class HealthHudBinder : MonoBehaviour
{
    private HealthHud hud;

    private int anterior = -1;
    private int maxAnterior = -1;

    private void Awake()
    {
        hud = GetComponent<HealthHud>();
    }

    // Evento estatico exige assinar e cancelar em par, senao a assinatura
    // sobrevive ao stop do editor e a proxima sessao herda ouvinte morto.
    private void OnEnable()
    {
        EventManager.OnHealthChanged += AoMudarVida;
    }

    private void OnDisable()
    {
        EventManager.OnHealthChanged -= AoMudarVida;
    }

    private void AoMudarVida(int atual, int max)
    {
        // primeira leitura: so desenha, sem animar
        if (anterior < 0)
        {
            hud.Desenhar(atual, max);
            anterior = atual;
            maxAnterior = max;
            return;
        }

        if (atual == anterior && max == maxAnterior)
        {
            hud.Desenhar(atual, max);
            return;
        }

        if (atual <= 0)
        {
            hud.AnimarMorte(max);
        }
        else if (anterior <= 0)
        {
            hud.AnimarRevive(atual, max);
        }
        else if (atual < anterior)
        {
            hud.AnimarDano(atual, max);
        }
        else
        {
            hud.AnimarCura(atual, max);
        }

        anterior = atual;
        maxAnterior = max;
    }
}
