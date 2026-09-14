using System;
using UnityEngine;

namespace Game.Health
{
    /// <summary>
    /// Canal de eventos em ScriptableObject: um arquivo no projeto funciona como ponto de encontro.
    /// Quem emite e quem escuta arrastam o mesmo asset e nunca precisam de referencia um ao outro.
    /// E o jeito de desacoplar no Unity sem singleton e sem procurar objeto na cena.
    /// </summary>
    [CreateAssetMenu(fileName = "HealthEventChannel", menuName = "Game/Health/Event Channel")]
    public class HealthEventChannel : ScriptableObject
    {
        /// <summary>Quem mudou e o que mudou.</summary>
        public event Action<GameObject, HealthChange> Changed;
        public event Action<GameObject> Died;
        public event Action<GameObject> Revived;

        public void RaiseChanged(GameObject owner, HealthChange change) => Changed?.Invoke(owner, change);
        public void RaiseDied(GameObject owner) => Died?.Invoke(owner);
        public void RaiseRevived(GameObject owner) => Revived?.Invoke(owner);

        /// <summary>
        /// ScriptableObject sobrevive ao stop do editor e guardaria assinantes mortos da sessao anterior.
        /// Limpar aqui evita o vazamento classico desse padrao.
        /// </summary>
        void OnDisable()
        {
            Changed = null;
            Died = null;
            Revived = null;
        }
    }
}
