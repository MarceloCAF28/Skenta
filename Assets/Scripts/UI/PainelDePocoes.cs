using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fileira de frascos embaixo dos coracoes. Frasco cheio e pocao disponivel,
/// frasco apagado e pocao ja bebida.
///
/// Mesma ideia dos coracoes: contar figura e mais rapido que ler numero.
/// </summary>
[ExecuteAlways]
public class PainelDePocoes : MonoBehaviour
{
    [SerializeField] private Image[] frascos;

    private ControlesDeVida controles;

    private void Awake()
    {
        if (frascos == null) return;

        foreach (var f in frascos)
        {
            if (f == null) continue;
            f.sprite = PotionSprite.Obter(true);
            f.preserveAspect = true;
        }

        // fora do Play mostra tudo cheio, para a cena nao parecer quebrada
        if (!Application.isPlaying) Desenhar(frascos.Length);
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        var jogador = Object.FindFirstObjectByType<Player>();
        if (jogador == null) return;

        controles = jogador.GetComponent<ControlesDeVida>();
        if (controles == null) return;

        controles.PocoesMudaram += Desenhar;
        Desenhar(controles.Pocoes);
    }

    private void OnDestroy()
    {
        if (controles != null) controles.PocoesMudaram -= Desenhar;
    }

    private void Desenhar(int disponiveis)
    {
        if (frascos == null) return;

        for (int i = 0; i < frascos.Length; i++)
        {
            if (frascos[i] == null) continue;

            bool tem = i < disponiveis;
            frascos[i].sprite = PotionSprite.Obter(tem);
            frascos[i].color = tem ? Color.white : new Color(1f, 1f, 1f, 0.32f);
        }
    }
}
