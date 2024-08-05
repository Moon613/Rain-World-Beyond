using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;

namespace EasternExpansion;

class LanternKelpHooks
{
    private static PlacedObject.Type LanternKelp = new PlacedObject.Type("Lantern Kelp", false);
    public static void Apply() {
        IL.Room.Loaded += Room_Loaded;
    }
    private static void Room_Loaded(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<PlacedObject.Type>(nameof(PlacedObject.Type.FlareBomb)))) {
            Plugin.Logger.LogDebug("EExpansion IL Room Placedobjects failed");
            return;
        }
        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdarg(0))) {
            Plugin.Logger.LogDebug("EExpansion IL Room Placedobjects failed");
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc, 41);
        cursor.EmitDelegate((Room self, int i) => {
            Debug.Log($"EExpansion type in IL: {self.roomSettings.placedObjects[i].type}");
            if (self.roomSettings.placedObjects[i].active && self.roomSettings.placedObjects[i].type == LanternKelp) {
                Debug.Log($"EExpansion pobject pos: {self.roomSettings.placedObjects[i].pos}");
                AbstractPhysicalObject abstractPhysicalObject = new LanternKelpAbstract(self.world, self.GetWorldCoordinate(self.roomSettings.placedObjects[i].pos), self.game.GetNewID());
                self.abstractRoom.entities.Add(abstractPhysicalObject);
                (abstractPhysicalObject as LanternKelpAbstract)?.Realize(self, self.roomSettings.placedObjects[i].pos);
                abstractPhysicalObject.realizedObject.PlaceInRoom(self);
                Debug.Log($"EExpansion added to entity list new {abstractPhysicalObject}");
            }
        });
    }
}