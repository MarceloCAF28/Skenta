using UnityEngine;

/// <summary>
/// Gera o slime por equacao, sem arquivo de imagem no projeto.
/// Mesmo principio do coracao do HUD: a forma vem de matematica e o antialiasing
/// vem de supersampling, ou seja, varias amostras por pixel e o alfa final e a
/// fracao delas que caiu dentro da figura.
///
/// Corpo: elipse achatada embaixo, o que da o jeitao de gosma escorrida.
/// Contorno: banda estreita perto da borda da elipse.
/// </summary>
public static class SlimeSprite
{
    static Sprite cache;

    static readonly Color Corpo     = new Color32(0x46, 0xC6, 0x66, 0xFF);
    static readonly Color CorpoBaixo= new Color32(0x2E, 0x93, 0x4A, 0xFF);
    static readonly Color Contorno  = new Color32(0x1C, 0x5C, 0x30, 0xFF);
    static readonly Color Olho      = new Color32(0x0E, 0x24, 0x18, 0xFF);
    static readonly Color Brilho    = new Color32(0xE8, 0xFF, 0xEE, 0xFF);

    public static Sprite Obter(int lado = 96, int amostras = 3)
    {
        if (cache != null) return cache;

        var tex = new Texture2D(lado, lado, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,   // pixel art: nada de borrar
            wrapMode = TextureWrapMode.Clamp
        };

        var px = new Color[lado * lado];
        float passo = 1f / amostras;
        int total = amostras * amostras;

        for (int j = 0; j < lado; j++)
        for (int i = 0; i < lado; i++)
        {
            int dentro = 0, naBorda = 0;
            Color soma = Color.clear;

            for (int sj = 0; sj < amostras; sj++)
            for (int si = 0; si < amostras; si++)
            {
                float u = (i + (si + 0.5f) * passo) / lado;
                float v = (j + (sj + 0.5f) * passo) / lado;
                float x = (u - 0.5f) * 2f;
                float y = (v - 0.5f) * 2f;

                float f = Dentro(x, y);
                if (f > 1f || y < -0.70f) continue;

                dentro++;
                if (f > 0.80f || y < -0.64f) { naBorda++; soma += Contorno; continue; }
                soma += CorDoPonto(x, y);
            }

            if (dentro == 0) { px[j * lado + i] = Color.clear; continue; }
            var c = soma / dentro;
            c.a = (float)dentro / total;
            px[j * lado + i] = c;
        }

        tex.SetPixels(px);
        tex.Apply();

        // 96 pixels por unidade deixa o slime com cerca de um quadrado de mundo
        cache = Sprite.Create(tex, new Rect(0, 0, lado, lado), new Vector2(0.5f, 0.5f), 96f);
        cache.name = "SlimeProcedural";
        return cache;
    }

    /// <summary>Elipse do corpo. Menor ou igual a 1 esta dentro.</summary>
    static float Dentro(float x, float y)
    {
        float a = x / 0.95f;
        float b = (y + 0.15f) / 0.85f;
        return a * a + b * b;
    }

    static Color CorDoPonto(float x, float y)
    {
        // olhos
        if (Elipse(x + 0.32f, y - 0.18f, 0.13f, 0.17f) || Elipse(x - 0.32f, y - 0.18f, 0.13f, 0.17f))
            return Olho;

        // brilho no alto a esquerda
        if (Elipse(x + 0.30f, y - 0.48f, 0.20f, 0.11f))
            return Brilho;

        // degrade vertical, mais escuro embaixo
        return Color.Lerp(CorpoBaixo, Corpo, Mathf.InverseLerp(-0.70f, 0.70f, y));
    }

    static bool Elipse(float x, float y, float rx, float ry)
    {
        float a = x / rx, b = y / ry;
        return a * a + b * b <= 1f;
    }
}
