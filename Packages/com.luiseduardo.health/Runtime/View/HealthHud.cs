using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Health
{
    /// <summary>
    /// HUD de vida: fileira de coracoes.
    /// So desenha e anima. Nenhuma regra de vida mora aqui.
    /// </summary>
    [ExecuteAlways]
    public class HealthHud : MonoBehaviour
    {
        [SerializeField] RectTransform grupo;
        [SerializeField] CanvasGroup opacidade;
        [SerializeField] Image[] fundos;
        [SerializeField] Image[] frentes;

        static readonly Color Cheio  = new Color(0.93f, 0.26f, 0.36f);
        static readonly Color Vazio  = new Color(0.21f, 0.14f, 0.18f);
        static readonly Color Morto  = new Color(0.36f, 0.36f, 0.40f);
        static readonly Color Cura   = new Color(0.38f, 0.88f, 0.46f);
        static readonly Color Alerta = new Color(0.97f, 0.78f, 0.22f);

        Vector2 ancoraOriginal;
        Coroutine batida, tremor;
        readonly Coroutine[] pulsos = new Coroutine[8];

        // ExecuteAlways faz o Awake rodar tambem no editor, fora do Play.
        // Sem isso os coracoes so aparecem depois de apertar Play, e a cena
        // fica com cinco quadrados brancos, que parece bug e nao e.
        void Awake()
        {
            // Em play mode, AddComponent dispara o Awake na hora, antes de quem
            // esta montando o objeto conseguir ligar os campos. Sem esta guarda
            // isso vira NullReferenceException e o componente nasce quebrado.
            if (grupo == null || fundos == null || frentes == null ||
                fundos.Length == 0 || frentes.Length == 0)
            {
                enabled = false;
                return;
            }

            var coracao = HeartSprite.Obter();

            foreach (var img in fundos)
            {
                img.sprite = coracao;
                img.type = Image.Type.Simple;
                img.color = Vazio;
            }
            foreach (var img in frentes)
            {
                img.sprite = coracao;
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillOrigin = (int)Image.OriginHorizontal.Left;
                img.color = Cheio;
            }

            ancoraOriginal = grupo.anchoredPosition;

            // no editor, mostra a vida cheia para a cena nao parecer quebrada
            if (!Application.isPlaying) Desenhar(100, 100);
        }

        /// <summary>Redesenha sem animar. Usado no primeiro quadro e ao carregar estado.</summary>
        public void Desenhar(int atual, int max)
        {
            float porCoracao = (float)max / frentes.Length;
            for (int i = 0; i < frentes.Length; i++)
                frentes[i].fillAmount = Mathf.Clamp01((atual - i * porCoracao) / porCoracao);

            float n = max <= 0 ? 0f : (float)atual / max;

            bool morto = atual <= 0;
            Color tom = morto ? Morto : Cheio;
            foreach (var img in frentes) img.color = tom;

            opacidade.alpha = morto ? 0.55f : 1f;

            AjustarBatida(n, morto);
        }

        public void AnimarDano(int atual, int max)
        {
            Desenhar(atual, max);
            Tremer();
            Pulsar(IndiceDaBorda(atual, max), Color.white, 1.35f);
        }

        public void AnimarCura(int atual, int max)
        {
            Desenhar(atual, max);
            Pulsar(IndiceDaBorda(atual, max), Cura, 1.30f);
        }

        public void AnimarMorte(int max)
        {
            Desenhar(0, max);
            Tremer(0.22f, 14f);
        }

        /// <summary>Coracoes voltam a encher um a um, da esquerda para a direita.</summary>
        public void AnimarRevive(int atual, int max)
        {
            Desenhar(atual, max);
            if (!Application.isPlaying) return;
            StartCoroutine(Cascata());
        }

        IEnumerator Cascata()
        {
            for (int i = 0; i < frentes.Length; i++)
            {
                if (frentes[i].fillAmount <= 0f) continue;
                Pulsar(i, Cura, 1.4f);
                yield return new WaitForSeconds(0.07f);
            }
        }

        /// <summary>Qual coracao esta na fronteira do valor atual, ou seja, o que acabou de mudar.</summary>
        int IndiceDaBorda(int atual, int max)
        {
            float porCoracao = (float)max / frentes.Length;
            int i = Mathf.CeilToInt(atual / porCoracao) - 1;
            return Mathf.Clamp(i, 0, frentes.Length - 1);
        }

        void Pulsar(int indice, Color flash, float escala)
        {
            if (!Application.isPlaying) return;
            if (indice < 0 || indice >= frentes.Length) return;
            if (pulsos[indice] != null) StopCoroutine(pulsos[indice]);
            pulsos[indice] = StartCoroutine(RotinaPulso(indice, flash, escala));
        }

        IEnumerator RotinaPulso(int indice, Color flash, float escala)
        {
            var alvo = frentes[indice].rectTransform;
            var corBase = frentes[indice].color;
            float duracao = 0.26f, t = 0f;

            while (t < duracao)
            {
                t += Time.unscaledDeltaTime;
                float p = t / duracao;
                // sobe rapido e volta devagar: da peso ao impacto
                float curva = Mathf.Sin(p * Mathf.PI);
                alvo.localScale = Vector3.one * Mathf.LerpUnclamped(1f, escala, curva);
                frentes[indice].color = Color.Lerp(corBase, flash, curva * 0.8f);
                yield return null;
            }

            alvo.localScale = Vector3.one;
            frentes[indice].color = corBase;
            pulsos[indice] = null;
        }

        void Tremer(float duracao = 0.18f, float forca = 9f)
        {
            if (!Application.isPlaying) return;
            if (tremor != null) StopCoroutine(tremor);
            tremor = StartCoroutine(RotinaTremor(duracao, forca));
        }

        IEnumerator RotinaTremor(float duracao, float forca)
        {
            float t = 0f;
            while (t < duracao)
            {
                t += Time.unscaledDeltaTime;
                float decaimento = 1f - t / duracao;
                grupo.anchoredPosition = ancoraOriginal + new Vector2(
                    Random.Range(-forca, forca) * decaimento,
                    Random.Range(-forca, forca) * decaimento);
                yield return null;
            }
            grupo.anchoredPosition = ancoraOriginal;
            tremor = null;
        }

        /// <summary>Abaixo de um quarto da vida o ultimo coracao pulsa sozinho, como batimento.</summary>
        void AjustarBatida(float normalizado, bool morto)
        {
            bool deveBater = !morto && normalizado > 0f && normalizado <= 0.25f;

            if (!deveBater)
            {
                if (batida != null) { StopCoroutine(batida); batida = null; }
                foreach (var img in frentes) img.rectTransform.localScale = Vector3.one;
                return;
            }
            if (!Application.isPlaying) return;
            if (batida == null) batida = StartCoroutine(RotinaBatida());
        }

        IEnumerator RotinaBatida()
        {
            while (true)
            {
                int ultimo = UltimoComVida();
                float escala = 1f + 0.10f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 5f));
                for (int i = 0; i < frentes.Length; i++)
                    frentes[i].rectTransform.localScale = i == ultimo ? Vector3.one * escala : Vector3.one;
                yield return null;
            }
        }

        /// <summary>Indice do coracao mais a direita que ainda tem alguma vida.</summary>
        int UltimoComVida()
        {
            for (int i = frentes.Length - 1; i >= 0; i--)
                if (frentes[i].fillAmount > 0f) return i;
            return 0;
        }
    }
}
