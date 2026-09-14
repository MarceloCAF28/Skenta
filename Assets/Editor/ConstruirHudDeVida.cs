using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Game.Health;

/// <summary>
/// Monta a interface do jogo por codigo e salva como prefab unico.
///
/// Tres cantos, tres papeis:
///   superior direito  vida em coracoes e pocoes, o que o jogador olha de relance
///   inferior esquerdo registro de acoes, o que ele le depois
///   inferior direito  teclas disponiveis, o que ele consulta quando esquece
/// O centro fica livre, porque o centro pertence ao jogo.
/// </summary>
public static class ConstruirHudDeVida
{
    const string PastaUI = "Assets/UI";
    const string CaminhoPrefab = PastaUI + "/HealthHUD.prefab";
    const string CaminhoCena = "Assets/Scenes/GameScene.unity";
    const int Coracoes = 5;
    const int Pocoes = 5;

    static readonly Color FundoPainel = new Color(0.07f, 0.08f, 0.11f, 0.78f);
    static readonly Color Titulo      = new Color(0.42f, 0.48f, 0.58f);
    static readonly Color Tecla       = new Color(0.98f, 0.80f, 0.35f);
    static readonly Color Acao        = new Color(0.80f, 0.84f, 0.90f);

    [MenuItem("Game/Health/Construir HUD de vida")]
    public static void Construir()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("HUD de vida",
                "Pare o Play antes de construir o HUD.\n\nO Unity nao deixa trocar de cena com o jogo rodando.",
                "Entendi");
            return;
        }

        var prefab = GerarPrefab();
        ColocarNaCena(prefab);
    }

    static GameObject GerarPrefab()
    {
        var raiz = new GameObject("HealthHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        var canvas = raiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = raiz.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        var fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var solido = UiSprites.Solido();

        MontarVida(raiz, solido);
        MontarRegistro(raiz, fonte, solido);
        MontarControles(raiz, fonte, solido);

        System.IO.Directory.CreateDirectory(PastaUI);
        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, CaminhoPrefab);
        Object.DestroyImmediate(raiz);

        Debug.Log($"[hud] prefab salvo em {CaminhoPrefab}");
        return prefab;
    }

    // ---------------- canto superior direito ----------------

    static void MontarVida(GameObject raiz, Sprite solido)
    {
        const float lado = 40f, espaco = 6f;
        float largura = Coracoes * lado + (Coracoes - 1) * espaco;

        var grupo = Vazio(raiz.transform, "Vida", new Vector2(1, 1), new Vector2(-28, -22), new Vector2(largura, 92));
        var opacidade = grupo.gameObject.AddComponent<CanvasGroup>();

        var fundos = new Image[Coracoes];
        var frentes = new Image[Coracoes];

        for (int i = 0; i < Coracoes; i++)
        {
            float x = -(Coracoes - 1 - i) * (lado + espaco);
            var slot = Vazio(grupo, $"Coracao {i}", new Vector2(1, 1), new Vector2(x, 0), new Vector2(lado, lado));
            fundos[i]  = Grafico(slot, "Fundo",  null, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(lado, lado));
            frentes[i] = Grafico(slot, "Frente", null, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(lado, lado));
        }

        // fileira de frascos logo abaixo dos coracoes
        const float ladoFrasco = 26f, espacoFrasco = 4f;
        var frascos = new Image[Pocoes];

        for (int i = 0; i < Pocoes; i++)
        {
            float x = -(Pocoes - 1 - i) * (ladoFrasco + espacoFrasco);
            frascos[i] = Grafico(grupo, $"Pocao {i}", null, Color.white,
                new Vector2(1, 1), new Vector2(x, -50), new Vector2(ladoFrasco, ladoFrasco));
        }

        var painelPocoes = grupo.gameObject.AddComponent<PainelDePocoes>();
        var soPocoes = new SerializedObject(painelPocoes);
        PreencherArray(soPocoes, "frascos", frascos);
        soPocoes.ApplyModifiedPropertiesWithoutUndo();

        var hud = raiz.AddComponent<HealthHud>();
        raiz.AddComponent<HealthHudBinder>();

        var so = new SerializedObject(hud);
        so.FindProperty("grupo").objectReferenceValue = grupo;
        so.FindProperty("opacidade").objectReferenceValue = opacidade;
        PreencherArray(so, "fundos", fundos);
        PreencherArray(so, "frentes", frentes);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------- canto inferior esquerdo ----------------

    static void MontarRegistro(GameObject raiz, Font fonte, Sprite solido)
    {
        var painel = Grafico(raiz.transform, "Registro", solido, FundoPainel,
            new Vector2(0, 0), new Vector2(24, 24), new Vector2(430, 196));

        var cabecalho = Texto(painel.rectTransform, "Cabecalho", "REGISTRO DE ACOES", fonte, 12,
            new Vector2(0, 1), new Vector2(14, -8), new Vector2(240, 16), TextAnchor.UpperLeft);
        cabecalho.color = Titulo;

        var linhas = Texto(painel.rectTransform, "Linhas", "", fonte, 14,
            new Vector2(0, 0), Vector2.zero, Vector2.zero, TextAnchor.LowerLeft);
        Esticar(linhas.rectTransform, 14, 12, 14, 30);
        linhas.horizontalOverflow = HorizontalWrapMode.Overflow;
        linhas.verticalOverflow = VerticalWrapMode.Overflow;

        var log = painel.gameObject.AddComponent<ActionLog>();
        painel.gameObject.AddComponent<LogBinder>();

        var so = new SerializedObject(log);
        so.FindProperty("texto").objectReferenceValue = linhas;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------- canto inferior direito ----------------

    static void MontarControles(GameObject raiz, Font fonte, Sprite solido)
    {
        var painel = Grafico(raiz.transform, "Controles", solido, FundoPainel,
            new Vector2(1, 0), new Vector2(-24, 24), new Vector2(248, 150));

        var cabecalho = Texto(painel.rectTransform, "Cabecalho", "CONTROLES", fonte, 12,
            new Vector2(0, 1), new Vector2(14, -8), new Vector2(160, 16), TextAnchor.UpperLeft);
        cabecalho.color = Titulo;

        var teclas = Texto(painel.rectTransform, "Teclas", "A D\nESPACO\nJ\nE\nR", fonte, 14,
            new Vector2(0, 1), new Vector2(14, -30), new Vector2(74, 100), TextAnchor.UpperRight);
        teclas.color = Tecla;
        teclas.lineSpacing = 1.25f;

        var acoes = Texto(painel.rectTransform, "Acoes", "mover\npular\ngolpe de espada\nbeber pocao\nreviver",
            fonte, 14, new Vector2(0, 1), new Vector2(98, -30), new Vector2(140, 100), TextAnchor.UpperLeft);
        acoes.color = Acao;
        acoes.lineSpacing = 1.25f;
        acoes.horizontalOverflow = HorizontalWrapMode.Overflow;

        var painelControles = painel.gameObject.AddComponent<PainelDeControles>();
        var so = new SerializedObject(painelControles);
        so.FindProperty("colunaDeAcoes").objectReferenceValue = acoes;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------- cena ----------------

    static void ColocarNaCena(GameObject prefab)
    {
        var cena = EditorSceneManager.OpenScene(CaminhoCena, OpenSceneMode.Single);

        foreach (var obj in cena.GetRootGameObjects())
            if (obj.name == "HealthHUD") Object.DestroyImmediate(obj);

        var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instancia.name = "HealthHUD";

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log($"[hud] instancia colocada em {CaminhoCena}");
    }

    // ---------------- ajudantes ----------------

    static void PreencherArray(SerializedObject so, string campo, Object[] valores)
    {
        var p = so.FindProperty(campo);
        p.arraySize = valores.Length;
        for (int i = 0; i < valores.Length; i++)
            p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
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
        t.supportRichText = true;
        t.color = Acao;
        t.raycastTarget = false;
        return t;
    }
}
