using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using static Pom.Pom;

namespace EasternExpansion;

public class LanternKelpPlaced
{
    class LanternKelpPOM : UpdatableAndDeletable
    {
        public LanternKelpPOM(PlacedObject pObj, Room room) {
            Debug.Log($"EExpansion made new type of {typeof(LanternKelpPOM)}, and placedObject type is {pObj.type}");
        }
    }
    public static void RegisterLanternKelp() {
        List<ManagedField> fields = new List<ManagedField>{

        };
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(LanternKelpPOM), "Lantern Kelp", "Eastern Expansion");
    }
}