using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lista de teclas no canto inferior direito, para o jogador saber o que apertar.
/// A unica linha viva e a da pocao, que mostra quantas restam.
/// </summary>
public class PainelDeControles : MonoBehaviour
{
    [SerializeField] private Text colunaDeAcoes;

    private ControlesDeVida controles;

    private void Start()
    {
        var jogador = Object.FindFirstObjectByType<Player>();
        if (jogador == null) return;

        controles = jogador.GetComponent<ControlesDeVida>();
        if (controles == null) return;

        controles.PocoesMudaram += Atualizar;
        Atualizar(controles.Pocoes);
    }

    private void OnDestroy()
    {
        if (controles != null) controles.PocoesMudaram -= Atualizar;
    }

    private void Atualizar(int pocoes)
    {
        if (colunaDeAcoes == null) return;

        string pocao = pocoes > 0 ? $"beber pocao  ({pocoes})" : "beber pocao  (acabou)";
        colunaDeAcoes.text = string.Join("\n", "mover", "pular", "golpe de espada", pocao, "reviver");
    }
}
