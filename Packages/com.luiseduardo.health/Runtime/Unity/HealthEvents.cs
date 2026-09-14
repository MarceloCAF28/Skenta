using System;
using UnityEngine.Events;

namespace Game.Health
{
    /// <summary>
    /// UnityEvent generico so aparece no Inspector se existir uma classe concreta herdando dele.
    /// Por isso estas tres linhas existem.
    /// </summary>
    [Serializable] public class HealthChangedEvent : UnityEvent<int, int> { }

    [Serializable] public class HealthNormalizedEvent : UnityEvent<float> { }
}
