using Fisobs.Core;
using UnityEngine;

namespace EasternExpansion;

class FisherMaskAbstract : AbstractPhysicalObject
{
    public FisherMaskAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, EnumExt_VultureMask.FisherMask, null, pos, ID)
    {
        scaleX = 1;
        scaleY = 1;
        saturation = 0.5f;
        hue = 1f;
    }

    public override void Realize()
    {
        base.Realize();
        Debug.Log($"Chimeric fishermask realize 1: {realizedObject}");
        if (realizedObject == null)
            realizedObject = new FisherMask(this);
        Debug.Log($"Chimeric fishermask realize 2: {realizedObject}");
    }

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;
    public float damage;
    public int colorSeed;

    public override string ToString()
    {
        return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY};{damage}");
    }
}