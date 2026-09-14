using UnityEngine;
using Game.Health;

/// <summary>
/// Barrinha de vida no mundo, embaixo do dono.
///
/// Ela nasce invisivel e so aparece quando o dono leva dano, sumindo sozinha
/// depois de alguns segundos. Inimigo com barra permanente polui a tela; barra
/// que aparece no impacto conta exatamente o que o jogador precisa saber,
/// na hora em que ele precisa saber.
///
/// Nao usa Canvas: sao dois SpriteRenderer. Um Canvas por inimigo custaria caro
/// e ainda teria que ser reposicionado todo quadro.
///
/// Os sprites sao atribuidos aqui, em execucao, e nao pelo script que monta o prefab.
/// Sprite feito com Sprite.Create existe so em memoria: nao e arquivo do projeto, entao
/// o prefab nao consegue guardar a referencia e ela vira nula ao salvar. Foi exatamente
/// por isso que a barra ficou invisivel sem dar erro nenhum.
/// </summary>
[ExecuteAlways]
public class BarraDeVidaMundo : MonoBehaviour
{
    [SerializeField] private HealthComponent alvo;
    [SerializeField] private SpriteRenderer fundo;
    [SerializeField] private SpriteRenderer preenchimento;

    [SerializeField] private float segundosVisivel = 1.1f;
    [SerializeField] private float velocidadeDoFade = 9f;

    private float visivelAte;
    private float opacidade;
    private float larguraCheia = 1f;
    private float normalizado = 1f;

    private void Reset() => alvo = GetComponentInParent<HealthComponent>();

    private void Awake()
    {
        if (alvo == null) alvo = GetComponentInParent<HealthComponent>();

        if (fundo != null) fundo.sprite = UiSprites.Solido();
        if (preenchimento != null) preenchimento.sprite = UiSprites.SolidoEsquerda();

        // guarda a largura montada no prefab: a escala X vira largura vezes fracao de vida,
        // e sem guardar isto a primeira atualizacao esmagaria a barra para um pixel
        if (preenchimento != null) larguraCheia = preenchimento.transform.localScale.x;

        Opacidade(0f);
    }

    private void Start()
    {
        if (!Application.isPlaying || alvo == null) return;
        alvo.Model.Changed += AoMudar;
        Desenhar(alvo.Current, alvo.Max);
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying || alvo == null || alvo.Model == null) return;
        alvo.Model.Changed -= AoMudar;
    }

    private void AoMudar(HealthChange mudanca)
    {
        Desenhar(mudanca.Current, mudanca.Max);
        if (!mudanca.IsDamage) return;

        visivelAte = Time.time + segundosVisivel;

        // aparece no mesmo quadro da pancada. Entrar por fade atrasa a informacao
        // justamente no instante em que ela importa, e o bicho pode morrer antes
        // de a barra terminar de aparecer.
        Opacidade(1f);
    }

    private void Desenhar(int atual, int max)
    {
        if (preenchimento == null) return;

        normalizado = max <= 0 ? 0f : Mathf.Clamp01((float)atual / max);

        // o sprite tem pivo na esquerda, entao escalar o X enche da esquerda para a direita
        var e = preenchimento.transform.localScale;
        preenchimento.transform.localScale = new Vector3(larguraCheia * normalizado, e.y, e.z);

        preenchimento.color = Cor(normalizado, preenchimento.color.a);
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        // so o sumico e gradual
        if (Time.time < visivelAte || opacidade <= 0f) return;

        Opacidade(Mathf.MoveTowards(opacidade, 0f, velocidadeDoFade * Time.deltaTime));
    }

    private void Opacidade(float a)
    {
        opacidade = a;
        if (fundo != null) fundo.color = new Color(0.04f, 0.05f, 0.07f, 0.92f * a);
        if (preenchimento != null) preenchimento.color = Cor(normalizado, a);
    }

    private static Color Cor(float normalizado, float alfa)
    {
        var c = normalizado > 0.5f ? new Color(0.42f, 0.85f, 0.40f)
              : normalizado > 0.25f ? new Color(0.95f, 0.75f, 0.20f)
                                    : new Color(0.90f, 0.26f, 0.28f);
        c.a = alfa;
        return c;
    }
}
