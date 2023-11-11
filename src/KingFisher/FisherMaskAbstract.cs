using Fisobs.Core;
using UnityEngine;

namespace EasternExpansion;

class FisherMaskAbstract : AbstractPhysicalObject
{
    public FisherMaskAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, EnumExt_VultureMask.FisherMask, null, pos, ID)
    {
    }

    public override void Realize()
    {
        base.Realize();
        Debug.Log($"EExpansion fishermask realize 1: {realizedObject}");
        if (realizedObject == null)
            realizedObject = new FisherMask(this);
        Debug.Log($"EExpansion fishermask realize 2: {realizedObject}");
    }
    public int colorSeed;

    public override string ToString()
    {
        return this.SaveToString($"{colorSeed}");
    }
}