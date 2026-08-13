using UnityEngine; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Ports;
namespace ZombieWar.Features.VFX.Unity.View
{
    [DisallowMultipleComponent]
    public sealed class ParticleVFXView:MonoBehaviour,IVFXView
    {
        [SerializeField]private ParticleSystem[] systems; private bool _playing;
        private void Awake(){Cache();}
        private void Cache(){if(systems==null||systems.Length==0)systems=GetComponentsInChildren<ParticleSystem>(true);}
        public void Activate(in VFXPose pose,float scale){Apply(in pose);transform.localScale=Vector3.one*scale;if(!gameObject.activeSelf)gameObject.SetActive(true);}
        public void SetPose(in VFXPose pose)=>Apply(in pose);
        private void Apply(in VFXPose pose){transform.position=new Vector3(pose.Position.X,pose.Position.Y,pose.Position.Z);var f=new Vector3(pose.Forward.X,pose.Forward.Y,pose.Forward.Z);if(f.sqrMagnitude>.000001f)transform.rotation=Quaternion.LookRotation(f,Vector3.up);}
        public void Play(){Cache();_playing=true;for(int i=0;i<systems.Length;i++)if(systems[i]!=null)systems[i].Play(true);}
        public void SetPaused(bool paused){Cache();if(!_playing)return;for(int i=0;i<systems.Length;i++){var p=systems[i];if(p==null)continue;if(paused)p.Pause(true);else p.Play(true);}}
        public void Stop(){Cache();_playing=false;for(int i=0;i<systems.Length;i++)if(systems[i]!=null)systems[i].Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        public void Deactivate(){_playing=false;if(gameObject.activeSelf)gameObject.SetActive(false);}
    }
}
