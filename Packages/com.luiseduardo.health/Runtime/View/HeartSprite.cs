using UnityEngine;

namespace Game.Health
{
    /// <summary>
    /// Gera o coracao por equacao implicita, em tempo de execucao.
    /// Nao existe arquivo de imagem no projeto: a forma nasce de matematica.
    ///
    /// Curva do coracao:  (x2 + y2 - 1)3 - x2 * y3 <= 0
    /// O ponto esta dentro quando o lado esquerdo da a zero ou menos.
    ///
    /// O antialiasing sai de supersampling: cada pixel e dividido numa grade
    /// de N por N amostras e o alfa final e a fracao de amostras que caiu dentro.
    /// Sem isso a borda ficaria serrilhada.
    /// </summary>
    public static class HeartSprite
    {
        static Sprite cache;

        public static Sprite Obter(int lado = 128, int amostras = 4)
        {
            if (cache != null) return cache;

            var tex = new Texture2D(lado, lado, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[lado * lado];
            float passo = 1f / amostras;
            int total = amostras * amostras;

            for (int j = 0; j < lado; j++)
            for (int i = 0; i < lado; i++)
            {
                int dentro = 0;
                for (int sj = 0; sj < amostras; sj++)
                for (int si = 0; si < amostras; si++)
                {
                    float u = (i + (si + 0.5f) * passo) / lado;
                    float v = (j + (sj + 0.5f) * passo) / lado;
                    if (Dentro(u, v)) dentro++;
                }
                // branco com alfa variavel: assim da para tintar de qualquer cor pelo Image.color
                pixels[j * lado + i] = new Color32(255, 255, 255, (byte)(255 * dentro / total));
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            cache = Sprite.Create(tex, new Rect(0, 0, lado, lado), new Vector2(0.5f, 0.5f), 100f);
            cache.name = "CoracaoProcedural";
            return cache;
        }

        static bool Dentro(float u, float v)
        {
            // leva a textura, que vai de 0 a 1, para o plano onde a curva foi desenhada
            float x = (u - 0.50f) * 3.0f;
            float y = (v - 0.47f) * 3.0f;
            float t = x * x + y * y - 1f;
            return t * t * t - x * x * y * y * y <= 0f;
        }
    }
}
