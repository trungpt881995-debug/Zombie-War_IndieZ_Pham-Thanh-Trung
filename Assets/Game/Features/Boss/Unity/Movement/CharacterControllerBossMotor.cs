using UnityEngine; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Ports;
namespace ZombieWar.Features.Boss.Unity.Movement
{
    [DisallowMultipleComponent][RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControllerBossMotor:MonoBehaviour,IBossMotor
    {
        [SerializeField] private float gravity=-20f;[SerializeField] private float groundedStickVelocity=-2f;private CharacterController _controller;private bool _enabled=true;private float _verticalVelocity,_normalizedSpeed;
        public BossPoint Position{get{Vector3 p=transform.position;return new BossPoint(p.x,p.y,p.z);}} public float NormalizedSpeed=>_normalizedSpeed; private void Awake()=>_controller=GetComponent<CharacterController>();
        public void Warp(in BossPoint position){bool was=_controller.enabled;_controller.enabled=false;transform.position=new Vector3(position.X,position.Y,position.Z);_controller.enabled=was;_verticalVelocity=0f;_normalizedSpeed=0f;}
        public void SetEnabled(bool enabled){_enabled=enabled;if(!enabled)Stop();}
        public void MoveTowards(in BossPoint target,float speed,float dt){if(!_enabled||dt<=0f){Stop();return;}Vector3 cur=transform.position,delta=new Vector3(target.X-cur.x,0f,target.Z-cur.z);Vector3 horizontal=Vector3.zero;if(delta.sqrMagnitude>0.000001f&&speed>0f){horizontal=delta.normalized*speed;_normalizedSpeed=1f;}else _normalizedSpeed=0f;if(_controller.isGrounded&&_verticalVelocity<0f)_verticalVelocity=groundedStickVelocity;else _verticalVelocity+=gravity*dt;horizontal.y=_verticalVelocity;_controller.Move(horizontal*dt);} public void Stop(){_normalizedSpeed=0f;}
    }
}
