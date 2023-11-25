using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using RWCustom;
using UnityEngine;

namespace EasternExpansion;

class FisherMaskFisob : Fisob
{
    public FisherMaskFisob() : base(EnumExt_VultureMask.FisherMask)
    {
        Icon = new SimpleIcon("Kill_FisherMask", Custom.HSL2RGB(170f/360f, 0.63f, 0.52f));
        RegisterUnlock(EnumExt_VultureMask.FisherMaskUnlock);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock? unlock)
    {
        // Centi shield data is just floats separated by ; characters.
        string[] p = entitySaveData.CustomData.Split(';');

        if (p.Length < 1) {
            p = new string[1];
        }

        FisherMaskAbstract result = new FisherMaskAbstract(world, entitySaveData.Pos, entitySaveData.ID) {
            colorSeed = int.TryParse(p[0], out var cSeed) ? cSeed : Random.Range(0, 10000)
        };

        // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CentiShieldIcon below).
        if (unlock is SandboxUnlock u) {
        }

        return result;
    }
}