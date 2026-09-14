using UnityEngine;
using Game.Health;

/// <summary>
/// Alimenta o painel de registro. Escuta o canal do jogo e o barramento do projeto,
/// e nao conhece nem jogador nem slime.
/// </summary>
[RequireComponent(typeof(ActionLog))]
public class LogBinder : MonoBehaviour
{
    private ActionLog log;
    private int vidaAnterior = -1;

    private void Awake() => log = GetComponent<ActionLog>();

    private void OnEnable()
    {
        GameLog.AoEscrever += Escrever;
        EventManager.OnHealthChanged += AoMudarVida;
        EventManager.OnEnemyDefeated += AoDerrotarInimigo;
    }

    private void OnDisable()
    {
        GameLog.AoEscrever -= Escrever;
        EventManager.OnHealthChanged -= AoMudarVida;
        EventManager.OnEnemyDefeated -= AoDerrotarInimigo;
    }

    private void Start() => log.Registrar(ActionLog.Tipo.Sistema, "pronto. J golpe, E pocao");

    private void Escrever(ActionLog.Tipo tipo, string mensagem) => log.Registrar(tipo, mensagem);

    private void AoMudarVida(int atual, int max)
    {
        // o slime e o golpe ja escrevem a linha deles pelo GameLog.
        // aqui so sobra o que esses dois nao cobrem: morte e volta.
        if (atual <= 0 && vidaAnterior > 0)
            log.Registrar(ActionLog.Tipo.Morte, "voce morreu. R para voltar");

        else if (vidaAnterior == 0 && atual > 0)
            log.Registrar(ActionLog.Tipo.Revive, $"de volta com {atual}");

        vidaAnterior = atual;
    }

    private void AoDerrotarInimigo(string inimigo) { }
}
