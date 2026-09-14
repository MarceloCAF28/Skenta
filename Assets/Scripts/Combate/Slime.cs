using System.Collections;
using UnityEngine;
using Game.Health;

/// <summary>
/// Slime verde. Persegue o jogador quando ele chega perto e causa dano por encostar.
///
/// A vida dele usa o MESMO HealthComponent do jogador. Foi de graca: o modulo nunca
/// soube o que e um jogador, entao serve para qualquer coisa que tenha vida.
///
/// Colisao em duas camadas, de proposito:
///   colisor solido   corpo de verdade, bate no chao e no jogador
///   gatilho de dano  um pouco maior, so detecta o encosto e cobra o dano
/// Separar evita depender do contato fisico exato para cobrar dano, que falha quando
/// os dois corpos se resolvem no mesmo quadro e nunca chegam a se tocar de fato.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(Rigidbody2D))]
public class Slime : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 1.6f;
    [SerializeField] private float alcanceDePerseguicao = 7f;
    [SerializeField] private float distanciaMinima = 0.40f;

    [Header("Dano por contato")]
    [SerializeField] private int danoDeContato = 10;
    [SerializeField] private float intervaloEntreDanos = 1.1f;

    [Header("Convivencia")]
    [SerializeField] private float espacoEntreSlimes = 0.55f;
    [SerializeField] private float empurraoNaMorteDoJogador = 4.5f;

    [Header("Reacao ao golpe")]
    [SerializeField] private float empurrao = 5.5f;
    [SerializeField] private float tempoAtordoado = 0.28f;

    private HealthComponent vida;
    private Rigidbody2D corpo;
    private SpriteRenderer visual;
    private Player jogador;
    private Transform alvo;

    private Collider2D solido;
    private Collider2D gatilho;

    private float proximoDano;
    private float atordoadoAte;
    private Vector3 escalaBase;
    private bool morrendo;

    private void Awake()
    {
        vida = GetComponent<HealthComponent>();
        corpo = GetComponent<Rigidbody2D>();

        visual = GetComponentInChildren<SpriteRenderer>();
        if (visual != null && visual.sprite == null) visual.sprite = SlimeSprite.Obter();

        foreach (var c in GetComponents<Collider2D>())
        {
            if (c.isTrigger) gatilho = c;
            else solido = c;
        }

        escalaBase = transform.localScale;
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        jogador = Object.FindFirstObjectByType<Player>();
        if (jogador != null) alvo = jogador.transform;

        vida.Model.Died += AoMorrer;
        vida.Model.Changed += AoLevarDano;

        // a vida do jogador chega pelo barramento do projeto, nao por referencia direta
        EventManager.OnHealthChanged += AoMudarVidaDoJogador;
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;
        if (vida == null || vida.Model == null) return;
        vida.Model.Died -= AoMorrer;
        vida.Model.Changed -= AoLevarDano;
        EventManager.OnHealthChanged -= AoMudarVidaDoJogador;
    }


    /// <summary>
    /// Jogador caiu: o slime empurra o corpo para o lado e desencosta.
    /// Sem isto os tres ficam empilhados em cima do cadaver, um dentro do outro,
    /// porque todos continuam perseguindo o mesmo ponto que parou de se mexer.
    /// </summary>
    private void AoMudarVidaDoJogador(int atual, int max)
    {
        if (atual > 0 || morrendo || corpo == null || alvo == null) return;

        float direcao = Mathf.Sign(transform.position.x - alvo.position.x);
        if (Mathf.Approximately(direcao, 0f)) direcao = Random.value < 0.5f ? -1f : 1f;

        corpo.linearVelocity = new Vector2(direcao * empurraoNaMorteDoJogador, 2.5f);
        atordoadoAte = Time.time + 0.45f;
    }

    private void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        if (morrendo || alvo == null || Time.time < atordoadoAte) return;

        // corpo caido nao e ameaca nem alvo: o slime perde o interesse
        if (jogador != null && jogador.EstaMorto) { Parar(); return; }

        float distancia = alvo.position.x - transform.position.x;

        if (Mathf.Abs(distancia) > alcanceDePerseguicao || Mathf.Abs(distancia) < distanciaMinima)
        {
            Parar();
            return;
        }

        float direcao = Mathf.Sign(distancia);

        // nao entra dentro do colega. Como a velocidade e escrita na mao todo quadro,
        // a fisica nao consegue separar os dois sozinha e eles se atravessam.
        if (TemSlimeNaFrente(direcao)) { Parar(); return; }

        corpo.linearVelocity = new Vector2(direcao * velocidade, corpo.linearVelocity.y);
    }

    private void Update()
    {
        if (!Application.isPlaying || morrendo) return;

        // pulsacao de gosma: estica e achata devagar, sem animacao nem osso
        float p = Mathf.Sin(Time.time * 4f) * 0.06f;
        transform.localScale = new Vector3(
            escalaBase.x * (1f + p),
            escalaBase.y * (1f - p),
            escalaBase.z);
    }

    private bool TemSlimeNaFrente(float direcao)
    {
        var ponto = (Vector2)transform.position + new Vector2(direcao * espacoEntreSlimes, 0f);

        foreach (var c in Physics2D.OverlapCircleAll(ponto, espacoEntreSlimes * 0.55f))
        {
            var outro = c.GetComponentInParent<Slime>();
            if (outro != null && outro != this) return true;
        }
        return false;
    }


    private void Parar()
    {
        if (corpo != null) corpo.linearVelocity = new Vector2(0f, corpo.linearVelocity.y);
    }

    private void OnTriggerStay2D(Collider2D outro)
    {
        if (!Application.isPlaying || morrendo || Time.time < proximoDano) return;

        var jogador = outro.GetComponentInParent<Player>();
        if (jogador == null || jogador.EstaMorto || jogador.Invulneravel) return;

        proximoDano = Time.time + intervaloEntreDanos;

        // ReceberGolpe, nao TakeDamage: alem de tirar vida ele empurra o jogador
        // para longe e abre a janela de invulnerabilidade
        jogador.ReceberGolpe(danoDeContato, transform.position);

        GameLog.Escrever(ActionLog.Tipo.Dano, $"o slime encostou e tirou {danoDeContato}");
    }

    private void AoLevarDano(HealthChange mudanca)
    {
        if (!mudanca.IsDamage) return;

        GameLog.Escrever(ActionLog.Tipo.Ataque,
            $"slime levou {-mudanca.Delta}, resta {mudanca.Current}");

        // empurrao para longe de quem bateu, mais um instante sem andar,
        // senao ele volta grudando no jogador no mesmo quadro
        if (corpo != null && alvo != null && !mudanca.IsDead)
        {
            float direcao = Mathf.Sign(transform.position.x - alvo.position.x);
            if (Mathf.Approximately(direcao, 0f)) direcao = 1f;

            corpo.linearVelocity = new Vector2(direcao * empurrao, 2.2f);
            atordoadoAte = Time.time + tempoAtordoado;
        }

    }

    private void AoMorrer()
    {
        if (morrendo) return;
        morrendo = true;

        GameLog.Escrever(ActionLog.Tipo.Sistema, "slime derrotado");
        EventManager.TriggerEnemyDefeated("Slime");

        // Corpo em dissolucao some da fisica por inteiro.
        // Desligar colisor um a um deixa margem para sobrar algo ativo; simulated = false
        // tira o corpo do motor de fisica de uma vez, colisores, contatos e tudo.
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;

        if (corpo != null)
        {
            corpo.linearVelocity = Vector2.zero;
            corpo.bodyType = RigidbodyType2D.Kinematic;
            corpo.simulated = false;
        }

        StartCoroutine(Derreter());
    }

    /// <summary>Achata e some, que e o jeito mais barato de matar uma gosma.</summary>
    private IEnumerator Derreter()
    {
        const float duracao = 0.30f;
        float t = 0f;

        while (t < duracao)
        {
            t += Time.deltaTime;
            float p = t / duracao;

            transform.localScale = new Vector3(
                escalaBase.x * (1f + p * 0.5f),
                escalaBase.y * (1f - p),
                escalaBase.z);

            if (visual != null)
            {
                var c = visual.color;
                c.a = 1f - p;
                visual.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
