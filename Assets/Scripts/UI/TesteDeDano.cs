using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Atalhos de teclado so para provar o HUD enquanto o jogo ainda nao tem
/// inimigo nem armadilha causando dano de verdade.
///
/// TEMPORARIO. Quando existir fonte real de dano, desmarque "ativo" no Inspector
/// ou apague este componente. Ele nao faz parte do modulo de vida.
///
/// K causa dano, L cura, J mata, H revive.
/// </summary>
public class TesteDeDano : MonoBehaviour
{
    [SerializeField] private bool ativo = true;
    [SerializeField] private int dano = 10;
    [SerializeField] private int cura = 15;

    private Player jogador;

    private void Update()
    {
        if (!ativo || Keyboard.current == null) return;

        if (jogador == null)
        {
            jogador = Object.FindFirstObjectByType<Player>();
            if (jogador == null) return;
        }

        if (Keyboard.current.kKey.wasPressedThisFrame) jogador.TakeDamage(dano);
        if (Keyboard.current.lKey.wasPressedThisFrame) jogador.Heal(cura);
        if (Keyboard.current.jKey.wasPressedThisFrame) jogador.TakeDamage(9999);
        if (Keyboard.current.hKey.wasPressedThisFrame) jogador.Revive();
    }
}
