using UnityEngine;
using Fisobs.Core;

namespace EasternExpansion;

class LanternKelpAbstract : AbstractPhysicalObject
{
    public LanternKelpAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, EnumExt_LanternKelp.LanternKelp, null, pos, ID)
    {
    }
    public void Realize(Room room, Vector2 realPos)
    {
        base.Realize();
        if (realizedObject == null){
            realizedObject = new LanternKelp(room, realPos, this);
        }
    }
    public int colorSeed;

    public override string ToString()
    {
        return this.SaveToString($"{colorSeed}");
    }
}