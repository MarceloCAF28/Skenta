using System;

namespace Game.Health
{
    /// <summary>
    /// Estado serializavel da vida. E o unico formato que sai daqui para quem salva o jogo.
    /// Campos publicos e minusculos porque o JsonUtility do Unity so enxerga campos, nao propriedades.
    /// </summary>
    [Serializable]
    public struct HealthState
    {
        public int current;
        public int max;

        public HealthState(int current, int max)
        {
            this.current = current;
            this.max = max;
        }
    }
}
