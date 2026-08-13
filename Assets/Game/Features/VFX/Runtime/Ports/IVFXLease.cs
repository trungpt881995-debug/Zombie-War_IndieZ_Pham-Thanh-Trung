namespace ZombieWar.Features.VFX.Ports { public interface IVFXLease { IVFXView View{get;} bool IsReleased{get;} void Release(); } }
