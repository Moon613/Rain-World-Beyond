using System;
using System.Collections.Generic;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EasternExpansion;

public class LanternKelpFisobs : Fisob
{
    public LanternKelpFisobs() : base(EnumExt_LanternKelp.LanternKelp)
    {
        Icon = new SimpleIcon("Symbol_DangleFruit", Custom.HSL2RGB(170f/360f, 0.63f, 0.52f));
        RegisterUnlock(EnumExt_LanternKelp.LanternKelpUnlock);
        LanternKelpHooks.Apply();
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock? unlock)
    {
        string[] p = entitySaveData.CustomData.Split(';');

        if (p.Length < 1) {
            p = new string[1];
        }

       LanternKelpAbstract result = new LanternKelpAbstract(world, entitySaveData.Pos, entitySaveData.ID) {
            colorSeed = int.TryParse(p[0], out var cSeed) ? cSeed : Random.Range(0, 10000)
        };

        if (unlock is SandboxUnlock u) {
        }

        return result;
    }
}