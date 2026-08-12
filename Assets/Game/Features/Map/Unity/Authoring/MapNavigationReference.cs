using UnityEngine;

namespace ZombieWar.Features.Map.Unity.Authoring
{
    [DisallowMultipleComponent]
    public sealed class MapNavigationReference : MonoBehaviour
    {
        [SerializeField] private Object navigationDataOrRoot;
        public bool IsAssigned => navigationDataOrRoot != null;
        public Object Reference => navigationDataOrRoot;
    }
}
