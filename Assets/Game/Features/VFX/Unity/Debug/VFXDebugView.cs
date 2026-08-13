using UnityEngine; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Unity.Runtime;
namespace ZombieWar.Features.VFX.Unity.Debugging
{
    public sealed class VFXDebugView:MonoBehaviour
    {
        [SerializeField]private VFXRuntimeRoot root; [SerializeField]private Transform previewPoint; [SerializeField]private bool showOverlay=true;
        private void OnGUI(){if(!showOverlay||root==null)return;var r=root.Runtime;GUILayout.BeginArea(new Rect(12,12,260,260),GUI.skin.box);GUILayout.Label("VFX DEBUG");if(r==null){GUILayout.Label("Not bound");GUILayout.EndArea();return;}var s=r.Snapshot;GUILayout.Label("Mode: "+s.Mode);GUILayout.Label("Active: "+s.ActiveCount+"  Played: "+s.PlayedCount+"  Rejected: "+s.RejectedCount);if(GUILayout.Button("Explosion"))Play(VFXId.GrenadeExplosion);if(GUILayout.Button("Blood"))Play(VFXId.BloodImpact);if(GUILayout.Button("Boss Death"))Play(VFXId.BossDeath);if(GUILayout.Button("Cancel All"))r.CancelAll();GUILayout.EndArea();}
        private void Play(VFXId id){if(root?.Runtime==null)return;Vector3 p=previewPoint!=null?previewPoint.position:transform.position;Vector3 f=previewPoint!=null?previewPoint.forward:transform.forward;var point=new VFXPoint(p.x,p.y,p.z);var dir=new VFXDirection(f.x,f.y,f.z);var pose=new VFXPose(in point,in dir);var req=new VFXRequest(id,in pose);root.Runtime.Play(in req);}
    }
}
