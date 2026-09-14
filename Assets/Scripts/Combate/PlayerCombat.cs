using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Health;

/// <summary>
/// Golpe de espada do jogador.
///
/// O acerto nao procura por Slime: procura por <see cref="IDamageable"/>. Entao qualquer
/// coisa que ganhe vida no futuro, inimigo novo, caixa, porta, ja entra como alvo valido
/// sem tocar nesta classe.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Golpe")]
    [SerializeField] private int dano = 25;
    [SerializeField] private float alcance = 1.2f;
    [SerializeField] private float duracao = 0.38f;
    [SerializeField] private float instanteDoImpacto = 0.14f;
    [SerializeField] private float recarga = 0.15f;

    /// <summary>O Player consulta isto para saber qual animacao tocar.</summary>
    public bool Atacando { get; private set; }

    private Player jogador;
    private SpriteRenderer visual;
    private float proximoGolpe;
    private float golpeComecouEm;

    private void Awake()
    {
        jogador = GetComponent<Player>();
        visual = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Rede de seguranca: se a corrotina do golpe morrer no meio, por qualquer
        // motivo, Atacando ficaria preso em true e o boneco travaria na pose de
        // ataque para sempre. Passou do dobro da duracao, destrava.
        if (Atacando && Time.time > golpeComecouEm + duracao * 2f) Atacando = false;

        if (Keyboard.current == null || Atacando) return;
        if (Time.time < proximoGolpe) return;
        if (jogador != null && jogador.EstaMorto) return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
            StartCoroutine(Golpear());
    }

    private IEnumerator Golpear()
    {
        Atacando = true;
        golpeComecouEm = Time.time;
        proximoGolpe = Time.time + duracao + recarga;

        // o impacto acontece no meio da animacao, nao no primeiro quadro:
        // bater antes de a espada aparecer na tela fica errado de sentir
        yield return new WaitForSeconds(instanteDoImpacto);

        Acertar();

        yield return new WaitForSeconds(Mathf.Max(0f, duracao - instanteDoImpacto));
        Atacando = false;
    }

    private void Acertar()
    {
        var (centro, tamanho) = AreaDoGolpe();
        var atingidos = Physics2D.OverlapBoxAll(centro, tamanho, 0f);

        // o slime tem dois colisores, o solido e o gatilho de dano. Sem este conjunto
        // o mesmo golpe cobraria dano duas vezes do mesmo bicho.
        var jaAcertados = new System.Collections.Generic.HashSet<IDamageable>();

        foreach (var col in atingidos)
        {
            if (col.transform == transform || col.transform.IsChildOf(transform)) continue;

            var alvo = col.GetComponentInParent<IDamageable>();
            if (alvo == null || alvo.IsDead) continue;
            if (!jaAcertados.Add(alvo)) continue;

            alvo.TakeDamage(dano);
        }

        int quantos = jaAcertados.Count;

        GameLog.Escrever(ActionLog.Tipo.Ataque,
            quantos > 0 ? $"golpe acertou {quantos}" : "golpe no vazio");
    }

    /// <summary>
    /// Retangulo na frente do boneco, da altura dele.
    ///
    /// Era um circulo centrado na altura do meio do personagem. Como ele tem quase
    /// duas unidades de altura e o slime fica colado no chao, o circulo passava por
    /// cima da cabeca do bicho e o golpe saia sempre no vazio. Retangulo da altura
    /// inteira resolve, e tirar as medidas do sprite faz a area acompanhar a escala
    /// do personagem em vez de depender de numero chutado.
    /// </summary>
    private (Vector2 centro, Vector2 tamanho) AreaDoGolpe()
    {
        Bounds b = visual != null
            ? visual.bounds
            : new Bounds(transform.position, new Vector3(1f, 1.8f, 1f));

        var tamanho = new Vector2(alcance, b.size.y * 0.95f);
        var centro = new Vector2(b.center.x + Frente * (b.extents.x * 0.5f + alcance * 0.5f), b.center.y);

        return (centro, tamanho);
    }

    /// <summary>O Player vira o personagem invertendo o X da escala.</summary>
    private float Frente => Mathf.Sign(transform.localScale.x);

    private void OnDrawGizmosSelected()
    {
        if (visual == null) visual = GetComponent<SpriteRenderer>();
        var (centro, tamanho) = AreaDoGolpe();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(centro, tamanho);
    }
}
