using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Health;

public class Player : MonoBehaviour
{
  
    public float moveSpeed = 8f;
    public float jumpForce = 6f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public float playerScale = 5f;

    private Rigidbody2D rb;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;

    private string currentAnimation = "";


    // SISTEMA DE VIDA

    // A regra de vida não mora mais aqui dentro. Ela vem do pacote Game.Health,
    // que é um módulo separado e coberto por testes. Este arquivo virou fachada:
    // os métodos públicos continuam exatamente os mesmos, apenas repassam.
    // Quem já usava Player.TakeDamage, Heal, GetCurrentHealth, LoadHealth
    // ou OnHealthChanged não precisa mudar uma linha.

    // Vida máxima do jogador. Continua sendo a fonte da verdade deste Inspector.
    public int maxHealth = 100;

    private HealthComponent health;
    private PlayerCombat combate;
    private Vector3 pontoInicial;
    private SpriteRenderer sprite;

    [Header("Reação ao dano")]
    public float tempoInvulneravel = 0.9f;
    public float empurraoHorizontal = 6f;
    public float empurraoVertical = 4.5f;
    public float tempoSemControle = 0.22f;

    private float invulneravelAte;
    private float semControleAte;
    private Color corViva = Color.white;

    // Cor do corpo caído. Cinza lê como morto em qualquer jogo.
    private static readonly Color CorDeMorto = new Color(0.42f, 0.42f, 0.48f, 1f);

    /// <summary>Janela de graça depois de levar dano. Impede perder tudo encostado no inimigo.</summary>
    public bool Invulneravel => Time.time < invulneravelAte;

    // Atalhos de leitura para quem precisa saber o estado sem mexer na vida.
    public bool EstaMorto => health != null && health.IsDead;
    public bool VidaCheia => health != null && health.Current >= health.Max;

    // Evento que avisa outros sistemas quando a vida muda.
    // Envia: vida atual e vida máxima.
    public event Action<int, int> OnHealthChanged;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Define o tamanho inicial do jogador.
        transform.localScale = new Vector3(
            playerScale,
            playerScale,
            playerScale
        );

        // Liga o módulo de vida. Se o componente não foi colocado pelo Inspector,
        // ele é criado aqui, então nenhuma cena precisa ser ajustada na mão.
        health = GetComponent<HealthComponent>();

        if (health == null)
        {
            health = gameObject.AddComponent<HealthComponent>();
        }

        combate = GetComponent<PlayerCombat>();
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) corViva = sprite.color;
        pontoInicial = transform.position;

        health.Model.Changed += AoMudarVida;
        health.Model.Died += AoMorrer;
        health.Model.Revived += AoVoltar;

        // Aplica o maxHealth deste Inspector e começa com a vida cheia.
        health.Restore(new HealthState(maxHealth, maxHealth));

        // Restore só dispara evento quando o valor realmente muda, e no início
        // ele normalmente não muda. Então o estado inicial precisa ser publicado
        // na mão, senão a UI nasce sem nunca ter recebido nada e só acorda no
        // primeiro dano. Era exatamente isso que o código antigo fazia aqui.
        AoMudarVida(new HealthChange(health.Current, health.Current, health.Max));
    }


    void Update()
    {
        // Morto não anda, não pula e não ataca.
        if (EstaMorto)
        {
            horizontalInput = 0f;
            if (sprite != null) sprite.enabled = true;
            return;
        }

        PiscarQuandoInvulneravel();

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        horizontalInput = 0f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput = 1f;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput = -1f;
        }


        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(
                playerScale,
                playerScale,
                playerScale
            );
        }

        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(
                -playerScale,
                playerScale,
                playerScale
            );
        }


        SetAnimation();
    }


    void FixedUpdate()
    {
        // durante o empurrão o jogador não dirige, senão o empurrão não acontece
        if (Time.time < semControleAte) return;

        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
    }


    // Pisca ligando e desligando o desenho, não mexendo na cor.
    // A cor é do EfeitosDeDano, e dois sistemas escrevendo no mesmo campo
    // no mesmo quadro é receita de bug intermitente.
    private void PiscarQuandoInvulneravel()
    {
        if (sprite == null) return;
        sprite.enabled = !Invulneravel || Mathf.FloorToInt(Time.time * 14f) % 2 == 0;
    }


    void SetAnimation()
    {
        string newAnimation;

        // O golpe manda em qualquer outra animação enquanto dura.
        if (combate != null && combate.Atacando)
        {
            newAnimation = "attack";
        }

        else if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                newAnimation = "jump";
            }
            else
            {
                newAnimation = "fall";
            }
        }

        else
        {
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                newAnimation = "walk";
            }
            else
            {
                newAnimation = "idle";
            }
        }

        if (currentAnimation != newAnimation)
        {
            animator.Play(newAnimation);
            currentAnimation = newAnimation;
        }
    }


    // SISTEMA DE VIDA

    // Causa dano ao jogador.
    // O clamp, a morte e o disparo dos eventos são responsabilidade do módulo.
    public void TakeDamage(int amount)
    {
        health.TakeDamage(amount);
    }


    // Golpe vindo de alguém, com posição de origem.
    // Diferente de TakeDamage: aqui existe janela de invulnerabilidade e empurrão.
    // TakeDamage continua cru de propósito, para quem só quer tirar vida.
    public void ReceberGolpe(int dano, Vector2 origem)
    {
        if (EstaMorto || Invulneravel) return;

        invulneravelAte = Time.time + tempoInvulneravel;

        TakeDamage(dano);

        if (EstaMorto || rb == null) return;

        // joga para o lado oposto de quem bateu
        float direcao = Mathf.Sign(transform.position.x - origem.x);
        if (Mathf.Approximately(direcao, 0f)) direcao = 1f;

        rb.linearVelocity = new Vector2(direcao * empurraoHorizontal, empurraoVertical);

        // sem isto o FixedUpdate sobrescreveria a velocidade no quadro seguinte
        // e o empurrão não sairia do lugar
        semControleAte = Time.time + tempoSemControle;
    }


    // Recupera vida do jogador.
    // Atenção: pelo módulo, quem está morto não recupera vida com Heal.
    // Para trazer de volta existe Revive, que é intenção diferente de curar.
    public void Heal(int amount)
    {
        health.Heal(amount);
    }


    // Traz o jogador de volta com a vida cheia.
    public void Revive()
    {
        health.Revive();
    }


    // Repassa toda mudança de vida para quem escuta.
    private void AoMudarVida(HealthChange mudanca)
    {
        // Quem escutava o Player direto continua recebendo, como antes.
        OnHealthChanged?.Invoke(mudanca.Current, mudanca.Max);

        // E agora a mudança também sai no barramento central do projeto,
        // para quem não conhece o Player.
        EventManager.TriggerHealthChanged(mudanca.Current, mudanca.Max);
    }


    private void OnDestroy()
    {
        if (health != null && health.Model != null)
        {
            health.Model.Changed -= AoMudarVida;
            health.Model.Died -= AoMorrer;
            health.Model.Revived -= AoVoltar;
        }
    }


    // Morto deita no chão, fica cinza e sai do caminho de todo mundo.
    private void AoMorrer()
    {
        horizontalInput = 0f;

        if (animator != null)
        {
            animator.Play("idle");
            currentAnimation = "idle";
        }

        // tomba para o lado que estava virado
        transform.rotation = Quaternion.Euler(0f, 0f, transform.localScale.x >= 0f ? -90f : 90f);

        if (sprite != null)
        {
            sprite.enabled = true;
            sprite.color = CorDeMorto;
        }

        PousarNoChao();

        // Corpo caído não empurra nem é empurrado: vira cenário.
        // Sem isto o cadáver continua sendo um obstáculo sólido, os slimes
        // esbarram nele e ele fica boiando no ar em cima deles.
        foreach (var c in GetComponentsInChildren<Collider2D>())
        {
            c.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }


    // Encosta o corpo no chão em vez de deixá-lo parado onde morreu.
    private void PousarNoChao()
    {
        var chao = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
        if (chao.collider == null || sprite == null) return;

        // depois de tombar, a metade da altura passa a ser a metade da largura original
        float meiaAltura = sprite.bounds.extents.y;

        transform.position = new Vector3(
            transform.position.x,
            chao.point.y + meiaAltura,
            transform.position.z
        );
    }


    private void AoVoltar()
    {
        transform.rotation = Quaternion.identity;

        if (sprite != null)
        {
            sprite.color = corViva;
            sprite.enabled = true;
        }

        foreach (var c in GetComponentsInChildren<Collider2D>())
        {
            c.enabled = true;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
    }


    // Como isto é teste, voltar significa reaparecer onde o jogo começou.
    // Num jogo de verdade isto seria tela de morte ou checkpoint.
    public void ReviverNoPontoInicial()
    {
        Revive();

        transform.position = pontoInicial;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }


    // SISTEMA DE INVENTÁRIO

    // Método chamado quando o jogador utiliza um item.
    public void UseItem(string itemName)
    {
        // Se o item for uma poção, recupera 20 de vida.
        if (itemName == "Potion")
        {
            Heal(20);
        }
    }

    // SISTEMA DE SALVAMENTO

    // Retorna a vida atual para que o SaveSystem
    // possa armazená-la.
    public int GetCurrentHealth()
    {
        return health.Current;
    }


    // SISTEMA DE CARREGAMENTO
    // Recebe a vida salva e aplica ao jogador.
    public void LoadHealth(int savedHealth)
    {
        // Restore dispara o evento de mudança, então a UI se atualiza sozinha
        // depois de carregar. O sistema de save não precisa avisar ninguém.
        health.Restore(new HealthState(savedHealth, health.Max));
    }


    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}