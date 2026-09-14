using System.Collections;
using UnityEngine;
using Game.Health;

/// <summary>
/// Reacao visual a mudanca de vida: pisca vermelho e joga o numero para cima.
///
/// O mesmo componente serve para o jogador e para o slime, porque ele escuta o
/// HealthComponent e nao sabe de quem e a vida. A unica diferenca entre os dois
/// e a cor do numero, que e campo do Inspector.
/// </summary>
[RequireComponent(typeof(HealthComponent))]
public class EfeitosDeDano : MonoBehaviour
{
    [Header("Piscada")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Color corDoFlash = new Color(1f, 0.25f, 0.25f);
    [SerializeField] private float duracaoDaPiscada = 0.09f;
    [SerializeField] private int piscadas = 2;

    [Header("Numero")]
    [SerializeField] private float alturaDoNumero = 0.9f;
    [SerializeField] private float tamanhoDoNumero = 1f;
    [SerializeField] private bool numeroDeDanoRecebido = false;

    private HealthComponent vida;
    private Coroutine piscando;
    private Color corOriginal;

    private void Awake()
    {
        vida = GetComponent<HealthComponent>();
        if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();
        if (visual != null) corOriginal = visual.color;
    }

    private void Start()
    {
        if (!Application.isPlaying || vida == null) return;
        vida.Model.Changed += AoMudar;
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying || vida == null || vida.Model == null) return;
        vida.Model.Changed -= AoMudar;
    }

    private void AoMudar(HealthChange mudanca)
    {
        if (mudanca.Delta == 0) return;

        Vector3 ondeMostrar = transform.position + Vector3.up * alturaDoNumero;

        if (mudanca.IsDamage)
        {
            var cor = numeroDeDanoRecebido ? PopupDeDano.Recebido : PopupDeDano.Causado;
            PopupDeDano.Mostrar(ondeMostrar, (-mudanca.Delta).ToString(), cor, tamanhoDoNumero);

            if (mudanca.IsDead)
            {
                // morrendo nao pisca, e uma piscada em andamento e cortada sem
                // restaurar a cor: quem morre define a propria cor e as duas
                // brigariam pelo mesmo campo no mesmo quadro
                if (piscando != null) { StopCoroutine(piscando); piscando = null; }
                return;
            }

            Piscar();
        }
        else
        {
            PopupDeDano.Mostrar(ondeMostrar, $"+{mudanca.Delta}", PopupDeDano.Curado, tamanhoDoNumero);
        }
    }

    private void Piscar()
    {
        if (visual == null || !isActiveAndEnabled) return;
        if (piscando != null) StopCoroutine(piscando);
        piscando = StartCoroutine(RotinaDaPiscada());
    }

    private IEnumerator RotinaDaPiscada()
    {
        for (int i = 0; i < piscadas; i++)
        {
            visual.color = corDoFlash;
            yield return new WaitForSeconds(duracaoDaPiscada);

            if (visual == null || vida.IsDead) yield break;
            visual.color = corOriginal;
            yield return new WaitForSeconds(duracaoDaPiscada * 0.6f);
        }

        if (visual != null && !vida.IsDead) visual.color = corOriginal;
        piscando = null;
    }
}
