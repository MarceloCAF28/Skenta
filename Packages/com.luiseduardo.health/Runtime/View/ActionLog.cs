using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Health
{
    /// <summary>
    /// Painel de registro das acoes. Mostra as ultimas linhas, com hora, cor por tipo
    /// e as mais antigas esmaecendo, para o olho ir direto no que acabou de acontecer.
    ///
    /// Uma unica Text com rich text resolve tudo: nada de criar e destruir um objeto por linha,
    /// o que geraria lixo de memoria a cada evento.
    ///
    /// Detalhe que custou um bug: tag de cor aninhada nao multiplica alfa, a de dentro
    /// simplesmente sobrescreve a de fora. Entao o alfa e calculado e escrito em cada
    /// pedaco da linha, na hora de redesenhar.
    /// </summary>
    public class ActionLog : MonoBehaviour
    {
        public enum Tipo { Dano, Cura, Morte, Revive, Ataque, Item, Sistema }

        [SerializeField] Text texto;
        [SerializeField] int maximoLinhas = 8;

        struct Linha
        {
            public string hora;
            public Tipo tipo;
            public string mensagem;
        }

        readonly List<Linha> linhas = new List<Linha>();

        static Color CorDe(Tipo t) => t switch
        {
            Tipo.Dano   => new Color(0.94f, 0.27f, 0.27f),
            Tipo.Cura   => new Color(0.29f, 0.87f, 0.50f),
            Tipo.Morte  => new Color(0.97f, 0.44f, 0.44f),
            Tipo.Revive => new Color(0.38f, 0.65f, 0.98f),
            Tipo.Ataque => new Color(0.99f, 0.75f, 0.30f),
            Tipo.Item   => new Color(0.70f, 0.55f, 0.98f),
            _           => new Color(0.58f, 0.64f, 0.72f)
        };

        static string RotuloDe(Tipo t) => t switch
        {
            Tipo.Dano   => "DANO",
            Tipo.Cura   => "CURA",
            Tipo.Morte  => "MORTE",
            Tipo.Revive => "REVIVE",
            Tipo.Ataque => "GOLPE",
            Tipo.Item   => "ITEM",
            _           => "INFO"
        };

        public void Registrar(Tipo tipo, string mensagem)
        {
            float t = Time.timeSinceLevelLoad;
            linhas.Add(new Linha
            {
                hora = $"{(int)(t / 60):00}:{(int)(t % 60):00}",
                tipo = tipo,
                mensagem = mensagem
            });

            while (linhas.Count > maximoLinhas) linhas.RemoveAt(0);
            Redesenhar();
        }

        public void Limpar()
        {
            linhas.Clear();
            Redesenhar();
        }

        void Redesenhar()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < linhas.Count; i++)
            {
                // a linha mais nova fica opaca, as antigas vao sumindo
                float peso = (i + 1f) / linhas.Count;
                float alfa = Mathf.Lerp(0.28f, 1f, peso * peso);
                var l = linhas[i];

                sb.Append(Pinta(new Color(0.39f, 0.45f, 0.55f), alfa, l.hora)).Append("   ")
                  .Append("<b>").Append(Pinta(CorDe(l.tipo), alfa, Rotulo(l.tipo))).Append("</b>")
                  .Append(Pinta(new Color(0.80f, 0.84f, 0.88f), alfa, l.mensagem));

                if (i < linhas.Count - 1) sb.Append('\n');
            }
            texto.text = sb.ToString();
        }

        // alinha o rotulo com espacos para as mensagens comecarem na mesma coluna
        static string Rotulo(Tipo t) => RotuloDe(t).PadRight(7);

        static string Pinta(Color cor, float alfa, string conteudo)
        {
            var c = cor;
            c.a = alfa;
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(c)}>{conteudo}</color>";
        }
    }
}
