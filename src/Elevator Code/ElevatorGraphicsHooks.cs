using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace EasternExpansion;
internal class ElevatorGraphicsHooks
{
    static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
    static BindingFlags myMethodFlags = BindingFlags.Static | BindingFlags.Public;
    public static void RegisterHooks()
    {
        Hook doorGraphicsPosHook = new Hook(
            typeof(RegionGateGraphics.Clamp).GetProperty("posZ", propFlags).GetGetMethod(),
            typeof(ElevatorGraphicsHooks).GetMethod("RegionGateGraphics_Clamp_posZ_get", myMethodFlags)
        );
        On.RegionGateGraphics.DoorGraphic.ctor += DoorGraphic_ctor;
        On.RegionGateGraphics.ctor += RegionGateGraphics_ctor;
        On.RegionGateGraphics.Update += RegionGateGraphics_Update;
        On.RegionGateGraphics.InitiateSprites += RegionGateGraphics_InitiateSprites;
        On.RegionGateGraphics.DrawSprites += RegionGateGraphics_DrawSprites;
        On.RegionGateGraphics.ApplyPalette += RegionGateGraphics_ApplyPalette;
        On.RegionGateGraphics.AddToContainer += RegionGateGraphics_AddToContainer;
    }

    private static void DoorGraphic_ctor(On.RegionGateGraphics.DoorGraphic.orig_ctor orig, RegionGateGraphics.DoorGraphic self, RegionGateGraphics rgGraphics, RegionGate.Door door)
    {
        orig(self, rgGraphics, door);
        if (door.gate.room.abstractRoom.name == "GATE_SF_LC")
        {
            if (door.number == 0)
            {
                self.posZ = new Vector2(220f, 380f);

            }
            else
            {
                self.posZ = new Vector2(740f, 380f);
            }
            
        }
    }

    public delegate Vector2 orig_posZ(RegionGateGraphics.Clamp self);

    public static Vector2 RegionGateGraphics_Clamp_posZ_get(orig_posZ orig, RegionGateGraphics.Clamp self)
    {
        if (self.doorG.door.gate.room.abstractRoom.name == "GATE_SF_LC")
        {
            if (self.doorG.door.number == 0)
            {
                return new Vector2(220f, 380f);
            }
            else
            {
                return new Vector2(740f, 380f);
            }
        }
        else
        {
            return orig(self);
        }
        
    }

    private static void RegionGateGraphics_ctor(On.RegionGateGraphics.orig_ctor orig, RegionGateGraphics self, RegionGate gate)
    {
        if (gate.room.abstractRoom.name == "GATE_SF_LC")
        {
            self.gate = gate;
            self.doorGraphs = new RegionGateGraphics.DoorGraphic[2];
            for (int i = 0; i < 2; i++)
            {
                self.doorGraphs[i] = new RegionGateGraphics.DoorGraphic(self, gate.doors[i]);
            }

            self.totalSprites = self.doorGraphs[0].TotalSprites * 2;
        }
        else
        {
            orig(self, gate);
        }
    }

    private static void RegionGateGraphics_Update(On.RegionGateGraphics.orig_Update orig, RegionGateGraphics self)
    {
        if (self.gate.room.abstractRoom.name == "GATE_SF_LC")
        {
            
            for (int i = 0; i < 2; i++)
            {
                self.doorGraphs[i].Update();
            }
            
        }
        else
        {
            orig(self);
        }
    }

    private static void RegionGateGraphics_InitiateSprites(On.RegionGateGraphics.orig_InitiateSprites orig, RegionGateGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (self.gate.room.abstractRoom.name == "GATE_SF_LC")
        {

            sLeaser.sprites = new FSprite[self.totalSprites];
            for (int i = 0; i < 2; i++)
            {
                self.doorGraphs[i].InitiateSprites(sLeaser, rCam);
            }

        }
        else
        {
            orig(self, sLeaser, rCam);
        }
    }

    private static void RegionGateGraphics_DrawSprites(On.RegionGateGraphics.orig_DrawSprites orig, RegionGateGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
    {
        if (self.gate.room.abstractRoom.name == "GATE_SF_LC")
        {

            for (int i = 0; i < 2; i++)
            {
                self.doorGraphs[i].DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

        }
        else
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
        }
    }

    private static void RegionGateGraphics_ApplyPalette(On.RegionGateGraphics.orig_ApplyPalette orig, RegionGateGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self.gate.room.abstractRoom.name == "GATE_SF_LC")
        {
            for (int i = 0; i < 2; i++)
            {
                self.doorGraphs[i].ApplyPalette(sLeaser, rCam, palette);
            }
        }
        else
        {
            orig(self, sLeaser, rCam, palette);
        }
    }

    private static void RegionGateGraphics_AddToContainer(On.RegionGateGraphics.orig_AddToContainer orig, RegionGateGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (self.gate.room.abstractRoom.name == "GATE_SF_LC")
        {

            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Water");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (i % self.doorGraphs[0].TotalSprites >= 10 && i % self.doorGraphs[0].TotalSprites <= 13)
                {
                    rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[i]);
                }
                else
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
            }

        }
        else
        {
            orig(self, sLeaser, rCam, newContatiner);
        }
    }
}