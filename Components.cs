using KitchenMods;
using System.Drawing;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace FireEvent
{
    public enum FireGameMode
    {
        None,
        CountUp,
        CountDown
    }

    [InternalBufferCapacity(12)]
    public struct CFireScoreRestored : IBufferElementData
    {
        public Entity Item;
    }

    public struct CItemDeliveredAddFireScore : IComponentData, IModComponent { }
    public struct CGroupLeavingAddFireScore : IComponentData, IModComponent
    {
        public int Count;
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct SFireGameActive : IComponentData, IModComponent
    {
    }

    public struct SFireScore : IComponentData, IModComponent
    {
        public FireGameMode GameMode;
        public float Score;

        // Count Down
        public float BaseDecaySpeed;
        public float ItemDeliveredRecovery;
        public float GroupLeavingRecovery;
        public float CustomerProximityMultiplier;
        public float FrameMultiplier;
        public bool LoseWhenScoreEmpty;

        // Count Up
        public UpdateFireScoreCountUp.ScoringMode CountUpScoringMode;
        public int StartOfDayCoins;

        public bool RandomFireActive;
        public float RandomFireInterval;

        public bool Endless;
        public int Target;
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct SReset : IComponentData, IModComponent { }
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct SInitialized : IComponentData, IModComponent { }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct CGroupMemberOnFire : IComponentData, IModComponent { }

    public struct SInsuranceSpending : IComponentData, IModComponent
    {
        public int Amount;
    }
}
