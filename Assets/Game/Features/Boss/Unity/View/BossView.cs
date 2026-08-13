using System; using UnityEngine; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Ports;
namespace ZombieWar.Features.Boss.Unity.View
{
    [DisallowMultipleComponent]
    public sealed class BossView:MonoBehaviour,IBossView
    {
        [SerializeField] private Animator animator;[SerializeField] private Collider[] gameplayColliders;
        private static readonly int MovementSpeedHash=Animator.StringToHash("MovementSpeed"),SpawnHash=Animator.StringToHash("Spawn"),AttackHash=Animator.StringToHash("Attack"),HitHash=Animator.StringToHash("Hit"),DeathHash=Animator.StringToHash("Death");
        public event Action AttackImpact,AttackFinished,HitFinished,DeathFinished; public BossPoint Position{get{Vector3 p=transform.position;return new BossPoint(p.x,p.y,p.z);}}
        private void Awake(){if(animator==null)animator=GetComponentInChildren<Animator>();if(animator!=null)animator.applyRootMotion=false;}
        public void ResetForReuse(){if(animator!=null){animator.applyRootMotion=false;animator.speed=1f;animator.Rebind();animator.Update(0f);animator.SetFloat(MovementSpeedHash,0f);}}
        public void SetActive(bool active){if(gameObject.activeSelf!=active)gameObject.SetActive(active);} public void SetScale(float scale)=>transform.localScale=Vector3.one*Mathf.Max(0.01f,scale);
        public void SetLocomotionSpeed(float speed){if(animator!=null)animator.SetFloat(MovementSpeedHash,Mathf.Clamp01(speed));}
        public void SetGameplayCollisionEnabled(bool enabled){if(gameplayColliders==null)return;for(int i=0;i<gameplayColliders.Length;i++)if(gameplayColliders[i]!=null)gameplayColliders[i].enabled=enabled;}
        public void FaceTarget(in BossPoint target,float rotationSpeed,float dt){Vector3 d=new Vector3(target.X-transform.position.x,0f,target.Z-transform.position.z);if(d.sqrMagnitude<=0.000001f)return;Quaternion q=Quaternion.LookRotation(d.normalized,Vector3.up);transform.rotation=Quaternion.RotateTowards(transform.rotation,q,Mathf.Max(0f,rotationSpeed)*Mathf.Max(0f,dt));}
        public void PlaySpawn(){if(animator!=null)animator.SetTrigger(SpawnHash);} public void PlayAttack(){if(animator!=null)animator.SetTrigger(AttackHash);} public void PlayHit(){if(animator!=null)animator.SetTrigger(HitHash);} public void PlayDeath(){if(animator!=null)animator.SetTrigger(DeathHash);} public void SetAnimationPaused(bool paused){if(animator!=null)animator.speed=paused?0f:1f;}
        public void AnimationEvent_AttackImpact()=>AttackImpact?.Invoke(); public void AnimationEvent_AttackFinished()=>AttackFinished?.Invoke(); public void AnimationEvent_HitFinished()=>HitFinished?.Invoke(); public void AnimationEvent_DeathFinished()=>DeathFinished?.Invoke();
    }
}
