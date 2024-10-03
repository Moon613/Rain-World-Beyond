using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace EasternExpansion;
internal class ElevatorHooks
{
    public static void RegisterHooks()
    {
        //On.RoofTopView.ctor += RoofTopView_ctor;
        //On.BackgroundScene.PosFromDrawPosAtNeutralCamPos += BackgroundScene_PosFromDrawPosAtNeutralCamPos;
        //On.BackgroundScene.RoomToWorldPos += BackgroundScene_RoomToWorldPos;
        On.RegionGate.AllDoorsInPosition += RegionGate_AllDoorsInPosition;
        On.RegionGate.ctor += RegionGate_ctor;
        On.RegionGate.Update += RegionGate_Update;
        On.RegionGate.ChangeDoorStatus += RegionGate_ChangeDoorStatus;
        On.WaterGate.ctor += WaterGate_ctor;
        On.WaterGate.Update += WaterGate_Update;
        ElevatorGraphicsHooks.RegisterHooks();
    }

    //private static Vector2 BackgroundScene_PosFromDrawPosAtNeutralCamPos(On.BackgroundScene.orig_PosFromDrawPosAtNeutralCamPos orig, BackgroundScene self, Vector2 input, float depth)
    //{
    //    if (self.room.abstractRoom.name == "GATE_SF_LC")
    //    {
    //        return new Vector2(200, 200);
    //    }
    //    return orig(self, input, depth);
    //}

    //private static Vector2 BackgroundScene_RoomToWorldPos(On.BackgroundScene.orig_RoomToWorldPos orig, BackgroundScene self, Vector2 inRoomPos)
    //{
    //    if (self.room.abstractRoom.name == "GATE_SF_LC")
    //    {
    //        Debug.Log("ROOM POS CHECK ++++++++++++++++++++++++++++++++++++++++++++++++++++");
    //        //return new Vector2(100f, 5000f)  / 3f + new Vector2(10f, 10f) * 20f + inRoomPos - new Vector2(((float)self.room.world.GetAbstractRoom(self.room.abstractRoom.index).size.x - 300) * 20f, (float)self.room.world.GetAbstractRoom(self.room.abstractRoom.index).size.y * 20f) / 2f;
    //        return new Vector2(0, 0);
    //    }
    //    return orig(self, inRoomPos);
    //}

    //private static void RoofTopView_ctor(On.RoofTopView.orig_ctor orig, RoofTopView self, Room room, RoomSettings.RoomEffect effect)
    //{
    //    if (self.room.abstractRoom.name == "GATE_SF_LC")
    //    {
    //        self.isLC = true;
    //    }
    //    orig(self, room, effect);
    //}
    private static bool RegionGate_AllDoorsInPosition(On.RegionGate.orig_AllDoorsInPosition orig, RegionGate self)
    {
        if (self.room.abstractRoom.name == "GATE_SF_LC")
        {
            for (int i = 0; i < 2; i++)
            {
                if (self.doors[i].closedFac != self.goalDoorPositions[i])
                {
                    return false;
                }
            }
            return true;

        }
        else
        {
            return orig(self);
        }
    }
    private static void RegionGate_ChangeDoorStatus(On.RegionGate.orig_ChangeDoorStatus orig, RegionGate self, int door, bool open)
    {
        if (self.room.abstractRoom.name == "GATE_SF_LC")
        {
            Debug.Log("change start=====");
            bool isOpen;
            if (self.goalDoorPositions[0] == 0f)
            {
                isOpen = true;
            }
            else
            {
                isOpen = false;
            }
            int num = 10 + 26 * door;
            for (int x = num; x <= num + 1; x++)
            {
                for(int y = 18; y >= 10; y--)
                {
                    Debug.Log(Convert.ToString(x) + " , " + Convert.ToString(y));
                    self.room.GetTile(x, y).Terrain = isOpen ? Room.Tile.TerrainType.Air : Room.Tile.TerrainType.Solid;
                }
            }
            Debug.Log("change finish=====");
        }
        else
        {
            Debug.Log("other gate change start=====");
            orig(self, door, open);
        }
    }
    private static void WaterGate_Update(On.WaterGate.orig_Update orig, WaterGate self, bool eu)
    {
        if (self.room.abstractRoom.name == "GATE_SF_LC")
        {
            self.evenUpdate = eu;
            self.graphics.Update();
            if (self.mode == RegionGate.Mode.MiddleClosed)
            {
                int num = self.PlayersInZone();
                if (num > 0 && num < 3 && self.PlayersStandingStill())
                {
                    self.startCounter++;
                    if (self.startCounter == 10)
                    {
                        self.room.PlaySound(SoundID.SANDBOX_Play, 0f, 0.7f, 0.4f);
                    }
                    if (self.startCounter > 60)
                    {
                        
                        self.mode = RegionGate.Mode.ClosingAirLock;
                        self.goalDoorPositions[0] = 1f; self.goalDoorPositions[1] = 1f;
                        self.startCounter = 0;
                        ElevatorMovement.LockShortcuts(self.room);
                    }
                }
            }
            if (self.mode == RegionGate.Mode.ClosingAirLock)
            {
                if (self.AllDoorsInPosition())
                {
                    self.startCounter++;
                    if (self.startCounter > 150)
                    {
                        self.startCounter = 0;
                        self.mode = RegionGate.Mode.Waiting;
                    }
                }
            }
            if (self.mode == RegionGate.Mode.Waiting)
            {
                self.room.game.cameras[0].shortcutGraphics.ClearSprites();
                ElevatorMovement.StartAnimation(self.room, self);
                if (ElevatorMovement.phase == 5)
                {
                    self.room.lockedShortcuts.Clear();
                    self.room.game.overWorld.GateRequestsSwitchInitiation(self);

                    self.goalDoorPositions[0] = 0f; self.goalDoorPositions[1] = 0f;
                    self.mode = RegionGate.Mode.OpeningMiddle;
                }
            }
            if (self.mode == RegionGate.Mode.OpeningMiddle)
            {
                if (self.AllDoorsInPosition())
                {
                    self.mode = RegionGate.Mode.Closed;
                }
            }
            for (int j = 0; j < 2; j++)
            {
                self.doors[j].Update();
            }
        }
        else
        {
            orig(self, eu);
        }
    }
    private static void WaterGate_ctor(On.WaterGate.orig_ctor orig, WaterGate self, Room room)
    {
        if (room.abstractRoom.name == "GATE_SF_LC")
        {
            Debug.Log(new NotImplementedException("Water ctor started ======="));
            self.room = room;
            self.rainCycle = room.world.rainCycle;
            self.doors = new RegionGate.Door[2];
            self.mode = RegionGate.Mode.MiddleClosed;
            self.goalDoorPositions = new float[2]{0f, 0f};
            for (int k = 0; k < 2; k++)
            {
                self.doors[k] = new RegionGate.Door(self, k){closedFac = 0f};
            }
            Debug.Log(new NotImplementedException("Doors finished assignment"));
            self.graphics = new RegionGateGraphics(self);
            Debug.Log(new NotImplementedException("Graphics finished assignment"));
        }
        else
        {
            orig(self, room);
        }
    }
    private static void RegionGate_Update(On.RegionGate.orig_Update orig, RegionGate self, bool eu)
    {
        if (self.room.abstractRoom.name != "GATE_SF_LC")
        {
            orig(self, eu);
        }
    }
    private static void RegionGate_ctor(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
    {
        if (room.abstractRoom.name != "GATE_SF_LC")
        {
            orig(self, room);
        }
    }
}