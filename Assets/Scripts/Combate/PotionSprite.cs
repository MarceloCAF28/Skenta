using UnityEngine;

/// <summary>
/// Frasco de pocao gerado por codigo, mesmo principio do coracao e do slime.
/// Corpo redondo, gargalo reto, rolha em cima e liquido vermelho ate certa altura.
/// </summary>
public static class PotionSprite
{
    static Sprite cheia;
    static Sprite vazia;

    static readonly Color Vidro    = new Color32(0xC8, 0xDC, 0xE8, 0x66);
    static readonly Color Borda    = new Color32(0x2A, 0x38, 0x48, 0xFF);
    static readonly Color Liquido  = new Color32(0xE0, 0x3A, 0x52, 0xFF);
    static readonly Color LiqTopo  = new Color32(0xF4, 0x6A, 0x7E, 0xFF);
    static readonly Color Rolha    = new Color32(0x9A, 0x6B, 0x3C, 0xFF);
    static readonly Color Brilho   = new Color32(0xFF, 0xFF, 0xFF, 0xCC);

    /// <param name="comLiquido">false devolve o frasco apagado, para pocao ja usada</param>
    public static Sprite Obter(bool comLiquido = true, int lado = 64, int amostras = 3)
    {
        if (comLiquido && cheia != null) return cheia;
        if (!comLiquido && vazia != null) return vazia;

        var tex = new Texture2D(lado, lado, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var px = new Color[lado * lado];
        float passo = 1f / amostras;
        int total = amostras * amostras;

        for (int j = 0; j < lado; j++)
        for (int i = 0; i < lado; i++)
        {
            int dentro = 0;
            Color soma = Color.clear;

            for (int sj = 0; sj < amostras; sj++)
            for (int si = 0; si < amostras; si++)
            {
                float x = ((i + (si + 0.5f) * passo) / lado - 0.5f) * 2f;
                float y = ((j + (sj + 0.5f) * passo) / lado - 0.5f) * 2f;

                var c = CorDoPonto(x, y, comLiquido);
                if (c.a <= 0f) continue;
                dentro++;
                soma += c;
            }

            if (dentro == 0) { px[j * lado + i] = Color.clear; continue; }
            var cor = soma / dentro;
            cor.a *= (float)dentro / total;
            px[j * lado + i] = cor;
        }

        tex.SetPixels(px);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, lado, lado), new Vector2(0.5f, 0.5f), 64f);
        sprite.name = comLiquido ? "PocaoCheia" : "PocaoVazia";

        if (comLiquido) cheia = sprite; else vazia = sprite;
        return sprite;
    }

    static Color CorDoPonto(float x, float y, bool comLiquido)
    {
        const float nivelDoLiquido = 0.10f;

        bool corpo    = Elipse(x, y + 0.30f, 0.62f, 0.58f);
        bool gargalo  = Mathf.Abs(x) <= 0.22f && y > 0.10f && y < 0.62f;
        bool rolha    = Mathf.Abs(x) <= 0.28f && y >= 0.58f && y <= 0.84f;

        if (rolha) return EhBorda(Mathf.Abs(x), 0.28f) ? Borda : Rolha;
        if (!corpo && !gargalo) return Color.clear;

        // contorno do vidro
        bool bordaCorpo = corpo && Elipse(x, y + 0.30f, 0.52f, 0.48f) == false;
        bool bordaGargalo = gargalo && Mathf.Abs(x) > 0.16f;
        if (bordaCorpo || bordaGargalo) return Borda;

        if (comLiquido && y < nivelDoLiquido)
        {
            // faixa clara na superficie do liquido
            if (y > nivelDoLiquido - 0.10f) return LiqTopo;
            return Liquido;
        }

        // brilho no vidro
        if (Elipse(x + 0.26f, y + 0.18f, 0.10f, 0.20f)) return Brilho;

        var v = Vidro;
        if (!comLiquido) v.a *= 0.55f;
        return v;
    }

    static bool Elipse(float x, float y, float rx, float ry)
    {
        float a = x / rx, b = y / ry;
        return a * a + b * b <= 1f;
    }

    static bool EhBorda(float valor, float limite) => valor > limite - 0.05f;
}
