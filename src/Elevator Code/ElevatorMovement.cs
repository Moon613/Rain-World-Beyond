using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EasternExpansion;
internal class ElevatorMovement : BackgroundScene
{
    public ElevatorMovement(Room room) : base(room)
    {
        this.daySky = new Simple2DBackgroundIllustration(this, "AtC_Sky", new Vector2(683f, 384f));
        this.duskSky = new Simple2DBackgroundIllustration(this, "AtC_DuskSky", new Vector2(683f, 384f));
        this.nightSky = new Simple2DBackgroundIllustration(this, "AtC_NightSky", new Vector2(683f, 384f));

        AddElement(this.nightSky);
        AddElement(this.duskSky);
        AddElement(this.daySky);
        

        Vector2 pos = new Vector2(480f, -70f);
        terrainPos = pos;
        phase = 0;
        counter = 0;
        AddElement(new Building(this, "city2", new Vector2(pos.x - 250, pos.y - 100), 620.5f, 0.25f, "Background"));
        AddElement(new Building(this, "city2", new Vector2(pos.x - 250, pos.y + 200), 420.5f, 2f, "House"));
        AddElement(new Building(this, "city1", new Vector2(pos.x - 250, pos.y + 50), 180f, 2f, "House"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_repeat_1", new Vector2(pos.x, pos.y - 1382), 10f, "Foreground", "Background"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_repeat_2", new Vector2(pos.x, pos.y - 1382 * 2), 10f, "Foreground", "Background"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_lower", new Vector2(pos.x, pos.y - 3605), 10f, "Foreground", "Background"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_sky_transition", pos, 10f, "Foreground", "Background"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_BackGround_rendered", pos, 10f, "Background", "Basic"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_BackGround_rendered_layer2", pos, 10f, "Foreground", "Basic"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_end", new Vector2(pos.x, pos.y - 822), 10f, "Foreground", "Background"));
        AddElement(new FlatRoom(room, this, "GATE_SF_LC_end_layer2", new Vector2(pos.x, pos.y - 822), 10f, "Foreground", "Basic"));
    }
    public static void LockShortcuts(Room room)
    {
        for (int i = 0; i < room.shortcutsIndex.Length; i++)
        {
            room.lockedShortcuts.Add(room.shortcutsIndex[i]);
        }
    }
    public static void StartAnimation(Room room, RegionGate regionGate)
    {
        if (counter == nextShake)
        {
            int bumpX = UnityEngine.Random.Range(-5, 5);
            int bumpY = UnityEngine.Random.Range(-5, 5);
            float shake = UnityEngine.Random.Range(0.5f, 1f);
            room.ScreenMovement(null, new Vector2(bumpX, bumpY), shake);

            float volume = UnityEngine.Random.Range(0.4f, 1f);
            float pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            room.PlaySound(SoundID.Miros_Piston_Big_Scrape, 0f, volume, pitch);
            if (phase == 0)
            {
                nextShake = counter + UnityEngine.Random.Range(300, 450);
            }
            else
            {
                nextShake = counter - UnityEngine.Random.Range(300, 450);
            }
        }
        if (phase == 0)
        {
            if (counter == 20)
            {
                //room.PlaySound(SoundID.SANDBOX_Play, 0f, 0.7f, 0.4f);
                //room.PlaySound(SoundID., 0f, 1f, 1f);
                for (int i = 0; i < room.roomSettings.ambientSounds.Count; i++)
                {
                    if (room.roomSettings.ambientSounds[i].sample == "AM_IND-Midsex02.ogg")
                    {
                        room.roomSettings.ambientSounds[i].volume = 0.21f;
                    }
                    else if (room.roomSettings.ambientSounds[i].sample == "AM_WIN-StrongWnd2.ogg")
                    {
                        room.roomSettings.ambientSounds[i].volume = 0.65f;
                    }
                    else if (room.roomSettings.ambientSounds[i].sample == "Mech Active.wav")
                    {
                        room.roomSettings.ambientSounds[i].volume = 0.22f;
                    }
                    else if (room.roomSettings.ambientSounds[i].sample == "MSC-LowGroans.ogg")
                    {
                        room.roomSettings.ambientSounds[i].volume = 0.29f;
                    }
                }
            }
            if (counter >= 150)
            {
                if (room.roomSettings.Clouds != 1f)
                {
                    room.roomSettings.Clouds += 0.01f;
                }
            }
            if (counter > 800)
            {
                phase++;
                nextShake = counter - 1;
            }
        }
        else if (phase == 1)
        {
            if (room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount < .80f)
            {
                Debug.Log("darkness started");
                room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.Darkness).amount += .001f;
            }
            else
            {
                phase++;
            }
        }
        else if (phase == 2)
        {
            if (counter == 400)
            {
                phase++;
            }
        }
        else if (phase == 3)
        {
        }
        else if (phase == 4)
        {
            if (counter > 0)
            {
                changeRate = 0;
                counter = 0;
                for (int i = 0; i < room.roomSettings.ambientSounds.Count; i++)
                {
                    room.roomSettings.ambientSounds[i].volume = 0f;
                }
                tempLoop = new DisembodiedDynamicSoundLoop(regionGate);
                tempLoop.sound = SoundID.Screen_Shake_LOOP;
                tempLoop.Volume = 1f;
                room.ScreenMovement(new Vector2(480, 200), new Vector2(480, 200), 1f);
                //room.PlaySound(SoundID., 0f, 1f, 0.8f);
            }

            room.PlaySound(SoundID.Leviathan_Heavy_Terrain_Impact, 0f, 1f, 1f);
            tempLoop.Update();
            if (counter < -30)
            {
                tempLoop.Volume -= 0.01f;
            }
            if (counter == -150)
            {
                tempLoop.Volume = 0f;
                phase++;
            }

        }
        if (phase != 4)
        {
            changeRate = (counter / 115f) * (counter / 115f);
            terrainPos.y += changeRate;
        }

        if (phase != 5)
        {
            if (phase == 0) { counter++; }
            else { counter--; }
        }


        Debug.Log(Convert.ToString(counter) + " counter");

    }

    public static int nextShake = 30;
    public static DisembodiedDynamicSoundLoop tempLoop;
    public static float changeRate = 0;
    public static int phase = 0;
    public static int counter = 0;
    public static Vector2 terrainPos;

    public BackgroundScene.Simple2DBackgroundIllustration daySky;
    public BackgroundScene.Simple2DBackgroundIllustration duskSky;
    public BackgroundScene.Simple2DBackgroundIllustration nightSky;
    public class Building : BackgroundScene.BackgroundSceneElement
    {
        public Building(BackgroundScene backGround, string assetName, Vector2 pos, float depth, float scale, string shader) : base(backGround, pos, depth)
        {
            this.shader = shader;
            this.depth = depth;
            this.scale = scale;
            this.assetName = assetName;
            backGround.LoadGraphic(assetName, false, true);
            this.elementSize = Futile.atlasManager.GetElementWithName(assetName).sourceSize;
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite(this.assetName, true);
            sLeaser.sprites[0].scale = this.scale;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[shader];
            sLeaser.sprites[0].anchorY = 0f;
            sLeaser.sprites[0].anchorX = 0f;

            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = base.DrawPos(camPos, rCam.hDisplace);
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;
            sLeaser.sprites[0].alpha = 1f;

            sLeaser.sprites[0].scale = 5f;
            sLeaser.sprites[0].color = new Color(this.elementSize.x * this.scale / 4000f, this.elementSize.y * this.scale / 1500f, 1f / (this.depth / 20f));

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Foreground");
            }
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (shader == "House")
            {
                pos.y += changeRate/2f;
            }
        }
        private string shader;
        private float scale;
        private Vector2 elementSize;
        private string assetName;
    }
    public class FlatRoom : BackgroundScene.BackgroundSceneElement
    {
        public FlatRoom(Room room, BackgroundScene scene, string assetName, Vector2 pos, float depth, string layer, string shader) : base(scene, pos, depth)
        {
            Debug.Log("FlatRoom ctor start ====");
            this.layer = layer;
            this.pos = pos;
            this.assetName = assetName;
            this.shader = shader;
            scene.LoadGraphic(assetName, true, false);
            Debug.Log("FlatRoom ctor end ====");
        }
        public override void Update(bool eu)
        {
            //Debug.Log(Convert.ToString(ElevatorMovement.terrainPos.x) + " , " + Convert.ToString(ElevatorMovement.terrainPos.y));
            base.Update(eu);

            if (this.pos.y < 720f)
            {
                if (this.assetName == "GATE_SF_LC_lower")
                {
                    this.pos.y = ElevatorMovement.terrainPos.y - 3605;
                    this.pos.x = ElevatorMovement.terrainPos.x;
                    if (ElevatorMovement.phase != 0)
                    {
                        foreach (UpdatableAndDeletable uad in this.room.updateList)
                        {
                            if (uad is Redlight redlight)
                            {
                                redlight.placedObject.pos.y += ElevatorMovement.changeRate;
                            }
                            else if (uad is LightSource lightSource)
                            {
                                lightSource.pos.y += ElevatorMovement.changeRate;
                            }
                        }
                    }
                }
                else if (this.assetName != "GATE_SF_LC_repeat_1" && this.assetName != "GATE_SF_LC_repeat_2" && this.assetName != "GATE_SF_LC_end" && this.assetName != "GATE_SF_LC_end_layer2")
                {
                    this.pos = terrainPos;
                    base.pos = terrainPos;
                }

            }
            if (terrainPos.y >= 3375)
            {
                if (this.assetName == "GATE_SF_LC_repeat_1" || this.assetName == "GATE_SF_LC_repeat_2")
                {
                    if (this.pos.y >= 730f)
                    {
                        this.pos.y = -1452;
                    }
                    this.pos.y += changeRate;
                }
            }
            if (phase == 3)
            {
                if (this.assetName == "GATE_SF_LC_end" || this.assetName == "GATE_SF_LC_end_layer2")
                {
                    if (this.pos.y >= -65)
                    {
                        this.pos.y = -65 - changeRate;
                        phase++;
                    }
                    this.pos.y += changeRate;
                }
            }
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            Debug.Log("FlatRoom init start ====");
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite(this.assetName, true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders[this.shader];
            sLeaser.sprites[0].anchorY = 0f;
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer(this.layer));
            Debug.Log("FlatRoom init end ====");
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {

            //Vector2 vector = base.DrawPos(camPos, rCam.hDisplace);
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;
            sLeaser.sprites[0].alpha = 1f;
            //sLeaser.sprites[0].color = new Color(Mathf.Pow(Mathf.InverseLerp(0f, 600f, this.depth), 0.3f) * 0.9f, 0f, 0f);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer(this.layer);
            }
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }
        public string layer;
        public string shader;
        public string assetName;
    }
}