using UnityEngine;

namespace Game.Health
{
    /// <summary>
    /// Sprites basicos gerados por codigo.
    ///
    /// Existe porque `Resources.GetBuiltinResource&lt;Sprite&gt;("UI/Skin/UISprite.psd")`
    /// deixou de existir no Unity 6.6 e retorna null sem avisar. Depender de recurso
    /// embutido da engine amarra o modulo a uma versao especifica dela.
    /// Um retangulo branco de um pixel resolve o mesmo problema e nunca some.
    /// </summary>
    public static class UiSprites
    {
        static Sprite solido;
        static Sprite solidoEsquerda;

        /// <summary>Retangulo branco. Esticado por Image, preenche qualquer tamanho.</summary>
        public static Sprite Solido()
        {
            if (solido != null) return solido;

            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            solido = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            solido.name = "RetanguloSolido";
            return solido;
        }

        /// <summary>
        /// Igual ao solido, mas com o pivo na ponta esquerda.
        /// Serve para barra que enche: basta escalar o X de 0 a 1 e ela cresce
        /// a partir da esquerda, em vez de crescer para os dois lados.
        /// </summary>
        public static Sprite SolidoEsquerda()
        {
            if (solidoEsquerda != null) return solidoEsquerda;

            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            solidoEsquerda = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
            solidoEsquerda.name = "RetanguloSolidoEsquerda";
            return solidoEsquerda;
        }
    }
}
