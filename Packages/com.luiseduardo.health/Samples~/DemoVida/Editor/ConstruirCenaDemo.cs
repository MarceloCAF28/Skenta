using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Game.Health;
using Game.Health.Demo;

/// <summary>
/// Monta a cena de demonstracao por codigo.
/// Cena montada por script nasce igual em qualquer maquina e o diff no git fica revisavel.
/// </summary>
public static class ConstruirCenaDemo
{
    const string Destino = "Assets/DemoVida/DemoVida.unity";
    const int Coracoes = 5;

    static readonly Color Fundo   = new Color(0.07f, 0.08f, 0.11f);
    static readonly Color Painel  = new Color(0.11f, 0.13f, 0.17f, 0.92f);
    static readonly Color Botao1  = new Color(0.22f, 0.25f, 0.32f);
    static readonly Color Claro   = new Color(0.88f, 0.90f, 0.94f);

    [MenuItem("Game/Health/Construir cena demo")]
    public static void Construir()
    {
        var cena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var cam = Camera.main;
        if (cam != null) { cam.backgroundColor = Fundo; cam.clearFlags = CameraClearFlags.SolidColor; }

        var jogador = new GameObject("Jogador");
        var vida = jogador.AddComponent<HealthComponent>();

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

        var fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var solido = UiSprites.Solido();

        // ---------- HUD no canto superior direito ----------
        var hudGO = Vazio(canvasGO.transform, "HUD", new Vector2(1, 1), new Vector2(-36, -28), new Vector2(300, 86));
        var opacidade = hudGO.gameObject.AddComponent<CanvasGroup>();

        var fundos = new Image[Coracoes];
        var frentes = new Image[Coracoes];
        const float ladoCoracao = 48f, espaco = 8f;

        for (int i = 0; i < Coracoes; i++)
        {
            float x = -(Coracoes - 1 - i) * (ladoCoracao + espaco);
            var slot = Vazio(hudGO, $"Coracao {i}", new Vector2(1, 1),
                new Vector2(x, 0), new Vector2(ladoCoracao, ladoCoracao));
            fundos[i]  = Grafico(slot, "Fundo", null, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ladoCoracao, ladoCoracao));
            frentes[i] = Grafico(slot, "Frente", null, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ladoCoracao, ladoCoracao));
        }

        float larguraBarra = Coracoes * ladoCoracao + (Coracoes - 1) * espaco;

        var hud = hudGO.gameObject.AddComponent<HealthHud>();

        // ---------- registro de acoes no canto inferior esquerdo ----------
        var painel = Grafico(canvasGO.transform, "Registro", solido, Painel,
            new Vector2(0, 0), new Vector2(28, 28), new Vector2(430, 208));
        painel.rectTransform.pivot = new Vector2(0, 0);
        painel.rectTransform.anchoredPosition = new Vector2(28, 28);

        var cabecalho = Texto(painel.rectTransform, "Cabecalho", "REGISTRO DE ACOES", fonte, 13,
            new Vector2(0, 1), new Vector2(16, -8), new Vector2(240, 18), TextAnchor.UpperLeft);
        cabecalho.rectTransform.pivot = new Vector2(0, 1);
        cabecalho.color = new Vector4(0.45f, 0.51f, 0.60f, 1f);

        var linhas = Texto(painel.rectTransform, "Linhas", "", fonte, 15,
            new Vector2(0, 0), Vector2.zero, Vector2.zero, TextAnchor.LowerLeft);
        Esticar(linhas.rectTransform, 16, 14, 16, 32);
        linhas.supportRichText = true;
        linhas.horizontalOverflow = HorizontalWrapMode.Overflow;
        linhas.verticalOverflow = VerticalWrapMode.Overflow;

        var log = painel.gameObject.AddComponent<ActionLog>();

        // ---------- botoes embaixo, no centro ----------
        string[] nomes = { "Dano", "Cura", "Matar", "Reviver" };
        var botoes = new Button[nomes.Length];
        for (int i = 0; i < nomes.Length; i++)
            botoes[i] = BotaoUI(canvasGO.transform, nomes[i], fonte, solido,
                new Vector2(0.5f, 0), new Vector2(-285f + i * 190f, 56), new Vector2(170, 48));

        // ---------- ligacoes ----------
        Ligar(hud, ("grupo", hudGO), ("opacidade", opacidade));
        LigarArray(hud, "fundos", fundos);
        LigarArray(hud, "frentes", frentes);

        Ligar(log, ("texto", linhas));

        Ligar(canvasGO.AddComponent<HealthDemoUI>(),
            ("alvo", vida), ("hud", hud), ("log", log),
            ("botaoDano", botoes[0]), ("botaoCura", botoes[1]),
            ("botaoMatar", botoes[2]), ("botaoReviver", botoes[3]));

        System.IO.Directory.CreateDirectory("Assets/DemoVida");
        EditorSceneManager.SaveScene(cena, Destino);
        AssetDatabase.SaveAssets();
        Debug.Log($"[demo] cena salva em {Destino}");
    }

    // ---------------- ajudantes ----------------

    static void Ligar(Object alvo, params (string campo, Object valor)[] pares)
    {
        var so = new SerializedObject(alvo);
        foreach (var (campo, valor) in pares)
        {
            var p = so.FindProperty(campo);
            if (p == null) { Debug.LogError($"campo {campo} nao existe em {alvo.GetType().Name}"); continue; }
            p.objectReferenceValue = valor;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void LigarArray(Object alvo, string campo, Object[] valores)
    {
        var so = new SerializedObject(alvo);
        var p = so.FindProperty(campo);
        p.arraySize = valores.Length;
        for (int i = 0; i < valores.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static RectTransform Posicionar(Transform pai, GameObject go, Vector2 ancora, Vector2 pos, Vector2 tam)
    {
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = rt.anchorMax = ancora;
        rt.pivot = ancora;
        rt.anchoredPosition = pos;
        rt.sizeDelta = tam;
        return rt;
    }

    static void Esticar(RectTransform rt, float esq, float baixo, float dir, float cima)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(esq, baixo);
        rt.offsetMax = new Vector2(-dir, -cima);
    }

    static RectTransform Vazio(Transform pai, string nome, Vector2 ancora, Vector2 pos, Vector2 tam)
        => Posicionar(pai, new GameObject(nome, typeof(RectTransform)), ancora, pos, tam);

    static Image Grafico(Transform pai, string nome, Sprite sprite, Color cor, Vector2 ancora, Vector2 pos, Vector2 tam)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Posicionar(pai, go, ancora, pos, tam);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }

    static Text Texto(Transform pai, string nome, string valor, Font fonte, int tamanho,
                      Vector2 ancora, Vector2 pos, Vector2 tam, TextAnchor alinhamento)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        Posicionar(pai, go, ancora, pos, tam);
        var t = go.GetComponent<Text>();
        t.font = fonte;
        t.fontSize = tamanho;
        t.text = valor;
        t.alignment = alinhamento;
        t.color = Claro;
        t.raycastTarget = false;
        return t;
    }

    static Button BotaoUI(Transform pai, string rotulo, Font fonte, Sprite sprite,
                          Vector2 ancora, Vector2 pos, Vector2 tam)
    {
        var img = Grafico(pai, "Botao " + rotulo, sprite, Botao1, ancora, pos, tam);
        img.raycastTarget = true;
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        var cores = btn.colors;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(1.25f, 1.25f, 1.25f);
        cores.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        cores.fadeDuration = 0.08f;
        btn.colors = cores;

        var t = Texto(img.rectTransform, "Texto", rotulo, fonte, 18,
            new Vector2(0.5f, 0.5f), Vector2.zero, tam, TextAnchor.MiddleCenter);
        return btn;
    }
}
