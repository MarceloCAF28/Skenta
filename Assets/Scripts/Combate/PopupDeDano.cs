using UnityEngine;

/// <summary>
/// Numero que sobe e some no lugar onde o dano aconteceu.
///
/// Usa TextMesh, nao Canvas. Texto de mundo em Canvas exigiria um Canvas por popup
/// ou reposicionar tudo todo quadro; o TextMesh ja vive no mundo e some junto com o objeto.
/// </summary>
public static class PopupDeDano
{
    static Font fonte;

    public static readonly Color Recebido = new Color(1.00f, 0.29f, 0.29f);
    public static readonly Color Causado  = new Color(1.00f, 0.84f, 0.35f);
    public static readonly Color Curado   = new Color(0.38f, 0.90f, 0.48f);

    public static void Mostrar(Vector3 posicao, string texto, Color cor, float tamanho = 1f)
    {
        if (fonte == null) fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var go = new GameObject("PopupDeDano");

        // espalha um pouco na horizontal para dois numeros seguidos nao se cobrirem
        go.transform.position = posicao + new Vector3(Random.Range(-0.18f, 0.18f), 0f, 0f);

        var texto3d = go.AddComponent<TextMesh>();
        texto3d.font = fonte;
        texto3d.text = texto;
        texto3d.color = cor;
        texto3d.fontSize = 64;
        texto3d.fontStyle = FontStyle.Bold;
        texto3d.characterSize = 0.035f * tamanho;
        texto3d.anchor = TextAnchor.MiddleCenter;
        texto3d.alignment = TextAlignment.Center;

        var malha = go.GetComponent<MeshRenderer>();
        malha.material = fonte.material;
        malha.sortingOrder = 50;

        go.AddComponent<AnimacaoDoPopup>();
    }
}

/// <summary>Sobe, desacelera e some. Depois se destroi.</summary>
public class AnimacaoDoPopup : MonoBehaviour
{
    [SerializeField] private float duracao = 0.85f;
    [SerializeField] private float altura = 0.9f;

    private TextMesh texto;
    private Vector3 inicio;
    private float t;

    private void Awake()
    {
        texto = GetComponent<TextMesh>();
        inicio = transform.position;
    }

    private void Update()
    {
        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / duracao);

        // sobe rapido no comeco e vai freando, que e como salto de numero parece certo
        transform.position = inicio + Vector3.up * (altura * Mathf.Sqrt(p));

        // some so na segunda metade, para dar tempo de ler
        if (texto != null)
        {
            var c = texto.color;
            c.a = p < 0.5f ? 1f : 1f - (p - 0.5f) * 2f;
            texto.color = c;
        }

        if (p >= 1f) Destroy(gameObject);
    }
}
