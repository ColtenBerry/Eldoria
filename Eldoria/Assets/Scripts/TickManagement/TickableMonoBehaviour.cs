// using UnityEngine;

// public abstract class TickableMonoBehaviour : MonoBehaviour, ITickable
// {
//     protected virtual void Start()
//     {
//         if (TickManager.Instance != null)
//         {
//             TickManager.Instance.Register(this);
//         }
//     }

//     protected virtual void OnDestroy()
//     {
//         if (TickManager.Instance != null)
//         {
//             TickManager.Instance.Unregister(this);
//         }
//     }

//     public abstract void HandleTick(int tickCount);
// }
