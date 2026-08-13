using UnityEngine; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Integration.Boss.Unity { [DisallowMultipleComponent] public sealed class TransformBossTargetSource:MonoBehaviour,IBossTargetSource { public BossPoint Position{get{Vector3 p=transform.position;return new BossPoint(p.x,p.y,p.z);}}public bool IsActive=>isActiveAndEnabled&&gameObject.activeInHierarchy; } }
