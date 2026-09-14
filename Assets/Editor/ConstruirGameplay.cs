using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Game.Health;

/// <summary>
/// Monta a parte jogavel da demonstracao: animacao de golpe, prefab do slime
/// e colocacao de tudo na cena.
///
/// Montado por codigo pelo mesmo motivo do HUD: prefab e cena sao YAML ilegivel,
/// e um script que constroi tem diff revisavel e nasce igual em toda maquina.
/// </summary>
public static class ConstruirGameplay
{
    const string Cena = "Assets/Scenes/GameScene.unity";
    const string Controlador = "Assets/Animation/Player.controller";
    const string ClipeDeGolpe = "Assets/Animation/attack.anim";
    const string PastaPrefabs = "Assets/Prefabs";
    const string PrefabSlime = PastaPrefabs + "/Slime.prefab";

    /// <summary>Precisa bater com o campo duracao do PlayerCombat.</summary>
    const float DuracaoDoGolpe = 0.38f;

    /// <summary>Quadro menor que isto e sobra do recorte, nao personagem.</summary>
    const float LadoMinimoDoQuadro = 20f;

    static readonly string[] FolhasDoGolpe =
    {
        "Assets/Player/individual_sheets/male_hero-combo_1.png",
        "Assets/Player/individual_sheets/male_hero-combo_1_end.png"
    };

    [MenuItem("Game/Health/Construir gameplay demo")]
    public static void Construir()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Gameplay",
                "Pare o Play antes de construir.\n\nO Unity nao deixa trocar de cena com o jogo rodando.",
                "Entendi");
            return;
        }

        CriarClipeDeGolpe();
        var slime = CriarPrefabDoSlime();
        MontarCena(slime);

        AssetDatabase.SaveAssets();
        Debug.Log("[gameplay] pronto");
    }

    // ---------------- animacao ----------------

    static void CriarClipeDeGolpe()
    {
        var todos = FolhasDoGolpe.SelectMany(SpritesDe).ToArray();

        // O recorte automatico das folhas deixou fatias soltas: no combo_1 ha um quadro
        // de 6 por 6 pixels, e no _end ha um de 15 por 9 e outro de 9 por 7. Sao poeira,
        // nao personagem. Quando a animacao passava por eles o boneco sumia por um quadro
        // e parecia bug. O corte por area resolve sem precisar reeditar a arte.
        var quadros = todos.Where(EhQuadroDePersonagem).ToArray();

        if (quadros.Length == 0)
        {
            Debug.LogError("[gameplay] nao achei os sprites do golpe");
            return;
        }

        Debug.Log($"[gameplay] {todos.Length - quadros.Length} quadros de lixo descartados");

        // a taxa sai da duracao do golpe, para o clipe terminar exatamente quando
        // o PlayerCombat solta o controle. Assim nao corta no meio nem sobra quadro parado.
        var clipe = new AnimationClip { frameRate = quadros.Length / DuracaoDoGolpe };

        var chaves = new ObjectReferenceKeyframe[quadros.Length];
        for (int i = 0; i < quadros.Length; i++)
            chaves[i] = new ObjectReferenceKeyframe { time = i / clipe.frameRate, value = quadros[i] };

        // mesma ligacao dos clipes que ja existem: o SpriteRenderer da raiz
        var ligacao = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clipe, ligacao, chaves);

        // golpe nao repete, diferente de andar e parado
        var ajustes = AnimationUtility.GetAnimationClipSettings(clipe);
        ajustes.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clipe, ajustes);

        var existente = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipeDeGolpe);
        if (existente != null)
        {
            EditorUtility.CopySerialized(clipe, existente);
            clipe = existente;
            EditorUtility.SetDirty(clipe);
        }
        else
        {
            AssetDatabase.CreateAsset(clipe, ClipeDeGolpe);
        }

        var controlador = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controlador);
        if (controlador == null) { Debug.LogError("[gameplay] Player.controller nao encontrado"); return; }

        var maquina = controlador.layers[0].stateMachine;
        var estado = maquina.states.FirstOrDefault(e => e.state.name == "attack").state;

        if (estado == null) estado = maquina.AddState("attack");
        estado.motion = clipe;

        EditorUtility.SetDirty(controlador);
        Debug.Log($"[gameplay] clipe de golpe com {quadros.Length} quadros");
    }

    static Sprite[] SpritesDe(string caminho)
        => AssetDatabase.LoadAllAssetRepresentationsAtPath(caminho)
            .OfType<Sprite>()
            .OrderBy(s => FinalNumerico(s.name))
            .ToArray();

    static bool EhQuadroDePersonagem(Sprite s)
        => s.rect.width >= LadoMinimoDoQuadro && s.rect.height >= LadoMinimoDoQuadro;

    static int FinalNumerico(string nome)
    {
        var m = Regex.Match(nome, @"(\d+)$");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    // ---------------- slime ----------------

    static GameObject CriarPrefabDoSlime()
    {
        var raiz = new GameObject("Slime",
            typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D),
            typeof(HealthComponent), typeof(Slime));

        var visual = raiz.GetComponent<SpriteRenderer>();
        visual.sprite = SlimeSprite.Obter();
        visual.sortingOrder = 1;

        var corpo = raiz.GetComponent<Rigidbody2D>();
        corpo.gravityScale = 3f;
        corpo.freezeRotation = true;
        // pesado de proposito: o jogador esbarra e sente o corpo, mas nao arrasta o slime pelo mapa
        corpo.mass = 8f;
        corpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // colisor solido: fica de pe no chao. O Slime manda ignorar o jogador em tempo
        // de execucao, para nao empurrar ninguem para fora da plataforma.
        var colisor = raiz.GetComponent<CircleCollider2D>();
        colisor.radius = 0.42f;
        colisor.offset = new Vector2(0f, -0.08f);

        // gatilho: unico responsavel por cobrar o dano de encostar
        var gatilho = raiz.AddComponent<CircleCollider2D>();
        gatilho.isTrigger = true;
        gatilho.radius = 0.50f;
        gatilho.offset = new Vector2(0f, -0.05f);

        MontarBarraDeVida(raiz);

        // piscada e numero de dano. O visual e ligado na mao porque a barra de vida
        // tambem tem SpriteRenderer, e a busca automatica poderia pegar a barra.
        var efeitos = raiz.AddComponent<EfeitosDeDano>();
        var soEfeitos = new SerializedObject(efeitos);
        soEfeitos.FindProperty("visual").objectReferenceValue = visual;
        soEfeitos.FindProperty("alturaDoNumero").floatValue = 0.75f;
        soEfeitos.FindProperty("numeroDeDanoRecebido").boolValue = false;
        soEfeitos.ApplyModifiedPropertiesWithoutUndo();

        // 125 de vida com golpe de 25 da cinco pancadas, tempo suficiente para
        // acompanhar a barra descendo de 100 para 80, 60, 40, 20 e zero
        var so = new SerializedObject(raiz.GetComponent<HealthComponent>());
        so.FindProperty("maxHealth").intValue = 125;
        so.FindProperty("startingHealth").intValue = 125;
        so.ApplyModifiedPropertiesWithoutUndo();

        raiz.transform.localScale = Vector3.one * 0.8f;

        System.IO.Directory.CreateDirectory(PastaPrefabs);
        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, PrefabSlime);
        Object.DestroyImmediate(raiz);

        Debug.Log($"[gameplay] prefab do slime em {PrefabSlime}");
        return prefab;
    }

    /// <summary>Barrinha embaixo do slime. Dois SpriteRenderer, sem Canvas.</summary>
    static void MontarBarraDeVida(GameObject dono)
    {
        const float largura = 1.10f, altura = 0.20f;

        var raiz = new GameObject("BarraDeVida");
        raiz.transform.SetParent(dono.transform, false);
        raiz.transform.localPosition = new Vector3(0f, -0.60f, 0f);

        var fundo = NovoSprite(raiz.transform, "Fundo", UiSprites.Solido(), 3);
        fundo.transform.localPosition = Vector3.zero;
        fundo.transform.localScale = new Vector3(largura, altura, 1f);
        fundo.color = new Color(0.05f, 0.06f, 0.08f, 0f);

        var frente = NovoSprite(raiz.transform, "Preenchimento", UiSprites.SolidoEsquerda(), 4);
        frente.transform.localPosition = new Vector3(-(largura - 0.06f) / 2f, 0f, 0f);
        frente.transform.localScale = new Vector3(largura - 0.06f, altura - 0.06f, 1f);
        frente.color = new Color(0.42f, 0.85f, 0.40f, 0f);

        var barra = raiz.AddComponent<BarraDeVidaMundo>();
        var so = new SerializedObject(barra);
        so.FindProperty("alvo").objectReferenceValue = dono.GetComponent<HealthComponent>();
        so.FindProperty("fundo").objectReferenceValue = fundo;
        so.FindProperty("preenchimento").objectReferenceValue = frente;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static SpriteRenderer NovoSprite(Transform pai, string nome, Sprite sprite, int ordem)
    {
        var go = new GameObject(nome, typeof(SpriteRenderer));
        go.transform.SetParent(pai, false);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = ordem;
        return sr;
    }

    // ---------------- cena ----------------

    static void MontarCena(GameObject prefabSlime)
    {
        var cena = EditorSceneManager.OpenScene(Cena, OpenSceneMode.Single);

        var jogador = Object.FindFirstObjectByType<Player>();
        if (jogador == null) { Debug.LogError("[gameplay] Player nao encontrado na cena"); return; }

        if (jogador.GetComponent<PlayerCombat>() == null) jogador.gameObject.AddComponent<PlayerCombat>();
        if (jogador.GetComponent<ControlesDeVida>() == null) jogador.gameObject.AddComponent<ControlesDeVida>();

        if (jogador.GetComponent<EfeitosDeDano>() == null)
        {
            var efeitos = jogador.gameObject.AddComponent<EfeitosDeDano>();
            var so = new SerializedObject(efeitos);
            so.FindProperty("visual").objectReferenceValue = jogador.GetComponent<SpriteRenderer>();
            so.FindProperty("alturaDoNumero").floatValue = 1.25f;
            so.FindProperty("tamanhoDoNumero").floatValue = 1.25f;
            // no jogador o numero sai vermelho: e dano que ele levou, nao que ele causou
            so.FindProperty("numeroDeDanoRecebido").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // recria os slimes em vez de acumular a cada execucao do menu
        foreach (var velho in Object.FindObjectsByType<Slime>(FindObjectsSortMode.None))
            Object.DestroyImmediate(velho.gameObject);

        // o chao fica com o topo em y = 0 quase toda a largura do mapa
        Vector2[] pontos = { new Vector2(5.5f, 1.5f), new Vector2(-4.5f, 1.5f), new Vector2(8.5f, 1.5f) };

        foreach (var p in pontos)
        {
            var slime = (GameObject)PrefabUtility.InstantiatePrefab(prefabSlime);
            slime.transform.position = p;
        }

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log($"[gameplay] {pontos.Length} slimes na cena, combate no jogador");
    }
}
