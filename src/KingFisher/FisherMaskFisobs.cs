using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using UnityEngine;

namespace EasternExpansion;

class FisherMaskFisob : Fisob
{
    public FisherMaskFisob() : base(EnumExt_VultureMask.FisherMask)
    {
        Icon = new SimpleIcon("Kill_Vulture", Color.green);
        RegisterUnlock(EnumExt_VultureMask.FisherMaskUnlock, parent: MultiplayerUnlocks.SandboxUnlockID.Vulture);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock? unlock)
    {
        // Centi shield data is just floats separated by ; characters.
        string[] p = entitySaveData.CustomData.Split(';');

        if (p.Length < 6) {
            p = new string[6];
        }

        var result = new FisherMaskAbstract(world, entitySaveData.Pos, entitySaveData.ID) {
            hue = float.TryParse(p[0], out var h) ? h : 0,
            saturation = float.TryParse(p[1], out var s) ? s : 1,
            scaleX = float.TryParse(p[2], out var x) ? x : 1,
            scaleY = float.TryParse(p[3], out var y) ? y : 1,
            damage = float.TryParse(p[4], out var r) ? r : 0,
            colorSeed = int.TryParse(p[5], out var cSeed) ? cSeed : Random.Range(0, 10000)
        };

        // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CentiShieldIcon below).
        if (unlock is SandboxUnlock u) {
            result.hue = u.Data / 1000f;

            if (u.Data == 0) {
                result.scaleX += 0.2f;
                result.scaleY += 0.2f;
            }
        }

        return result;
    }
}