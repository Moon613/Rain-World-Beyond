using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using static System.Reflection.BindingFlags;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;

namespace EasternExpansion;
public class KingFisherHooks
{
    #pragma warning disable CS8602
    class KingFisherEx
    {
        public int startIndex;
    }
    static ConditionalWeakTable<VultureGraphics, KingFisherEx> KingFisherCWT = new ConditionalWeakTable<VultureGraphics, KingFisherEx>();
    internal static void Apply()
    {
        new Hook(typeof(Vulture).GetMethod("get_IsKing", Public | NonPublic | Instance), (Func<Vulture, bool> orig, Vulture self) => self.Template.type == CreatureTemplateType.KingFisher || orig(self));
        // new Hook(typeof(CreatureTemplate).GetMethod("get_IsVulture", Public | NonPublic | Instance), (Func<CreatureTemplate, bool> orig, CreatureTemplate self) => self.type == CreatureTemplateType.KingFisher || orig(self));
        On.KingTusks.Tusk.TuskBend += TuskBend;
        On.KingTusks.Tusk.TuskProfBend += TuskProfBend;
        On.KingTusks.Tusk.DrawSprites += Tusk_DrawSprites;
        On.KingTusks.Tusk.ShootUpdate += KingTusks_Tusk_ShootUpdate;
        On.Player.Grabability += Player_Grabability;
        On.Player.IsObjectThrowable += Player_IsObjectThrowable;
        On.VultureGraphics.ctor += VultureGraphics_ctor;
        On.VultureGraphics.InitiateSprites += VultureGraphics_InitiateSprites;
        On.VultureGraphics.DrawSprites += VultureGraphics_DrawSprites;
        On.PreyTracker.TrackedPrey.Attractiveness += PreyTracker_TrackedPrey_Attractiveness;
        On.KingTusks.WantToShoot += KingTusks_WantToShoot;
        On.Vulture.ctor += Vulture_ctor;
        IL.AImap.TileAccessibleToCreature_IntVector2_CreatureTemplate += AImap_TileAccessibleToCreature;
        IL.SlugcatHand.Update += SlugcatHand_Update;
        IL.Vulture.DropMask += Vulture_DropMask;
        IL.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_LizardAI_UpdateDynamicRelationship;
        IL.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_Physobj_bool;
    }
    private static void Vulture_ctor(On.Vulture.orig_ctor orig, Vulture self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.Template.type == CreatureTemplateType.KingFisher) {
            for (int i = 0; i < self.bodyChunkConnections.Length; i++) {
                if (i!=6 && i!=7) {
                    self.bodyChunkConnections[i].distance *= 0.85f;
                }
            }
            for (int i = 0; i < self.bodyChunks.Length-1; i++) {
                self.bodyChunks[i].rad *= 0.95f;
            }
        }
    }
    #region King Fisher Creature Hooks
    private static void AImap_TileAccessibleToCreature(ILContext il)
    {
        var cursor = new ILCursor(il);
        var label = cursor.DefineLabel();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldarg_2);
        cursor.EmitDelegate((AImap self, IntVector2 pos, CreatureTemplate crit) => {
            if (crit.type == CreatureTemplateType.KingFisher && self.room.GetTile(pos).AnyWater && !(self.room.GetTile(new IntVector2(pos.x, pos.y+8)).AnyWater || self.room.GetTile(pos).Solid)) {
                return true;
            }
            return false;
        });
        cursor.Emit(OpCodes.Brtrue, label);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchRet())) {
            Plugin.Logger.LogDebug("EExpansion AImap IL Failed!");
            return;
        }
        cursor.MarkLabel(label);
    }
    private static bool KingTusks_WantToShoot(On.KingTusks.orig_WantToShoot orig, KingTusks self, bool checkVisualOnAnyTargetChunk, bool checkMinDistance)
    {
        if (self.vulture.Template.type == CreatureTemplateType.KingFisher) {
            checkMinDistance = false;
        }
        return orig(self, checkVisualOnAnyTargetChunk, checkMinDistance);
    }
    private static float PreyTracker_TrackedPrey_Attractiveness(On.PreyTracker.TrackedPrey.orig_Attractiveness orig, PreyTracker.TrackedPrey self)
    {
        float result = orig(self);
        if (self.owner.AI.creature.creatureTemplate.type == CreatureTemplateType.KingFisher) {
            Debug.Log($"EE: target creature: {self.critRep.representedCreature.creatureTemplate.type}");
            Debug.Log($"EE: target creature submersion: {self.critRep.representedCreature.realizedCreature.Submersion}");
            Debug.Log($"EE: Result before modify: {result}");
            if (self.critRep.representedCreature.realizedCreature.Submersion > 0f) {
                result *= 10 * self.critRep.representedCreature.realizedCreature.Submersion;
            }
            else {
                result /= 10;
            }
            Debug.Log($"EE: Result after modify: {result}");
        }
        return result;
    }
    private static void KingTusks_Tusk_ShootUpdate(On.KingTusks.Tusk.orig_ShootUpdate orig, KingTusks.Tusk self, float speed)
    {
        if (self.vulture.Template.type == CreatureTemplateType.KingFisher) {
            speed *= 3f;
        }
        orig(self, speed);
    }
    private static void VultureGraphics_DrawSprites(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.vulture.Template.type == CreatureTemplateType.KingFisher && KingFisherCWT.TryGetValue(self, out var kingFisherEx)) {
            sLeaser.sprites[self.MaskArrowSprite].element = Futile.atlasManager.GetElementWithName("FisherDrop" + self.headGraphic.ToString());
            sLeaser.sprites[kingFisherEx.startIndex].x = sLeaser.sprites[self.MaskSprite].x;
            sLeaser.sprites[kingFisherEx.startIndex].y = sLeaser.sprites[self.MaskSprite].y;
            sLeaser.sprites[kingFisherEx.startIndex].rotation = sLeaser.sprites[self.MaskSprite].rotation;
            sLeaser.sprites[kingFisherEx.startIndex].element = Futile.atlasManager.GetElementWithName("FisherMark" + self.headGraphic.ToString());
            sLeaser.sprites[kingFisherEx.startIndex].scaleX = sLeaser.sprites[self.MaskSprite].scaleX;
            sLeaser.sprites[kingFisherEx.startIndex].scaleY = sLeaser.sprites[self.MaskSprite].scaleY;
            sLeaser.sprites[kingFisherEx.startIndex].isVisible = sLeaser.sprites[self.MaskSprite].isVisible;
            sLeaser.sprites[kingFisherEx.startIndex].color = sLeaser.sprites[self.MaskArrowSprite].color;
            sLeaser.sprites[kingFisherEx.startIndex].MoveInFrontOfOtherNode(sLeaser.sprites[self.MaskSprite]);
        }
    }
    // Modify the effect colors for the King Fisher, and add a CWT to the Graphics module to keep track of the array position of the extra cheek sprite.
    private static void VultureGraphics_ctor(On.VultureGraphics.orig_ctor orig, VultureGraphics self, Vulture ow)
    {
        orig(self, ow);
        if (self.vulture.Template.type == CreatureTemplateType.KingFisher) {
            self.ColorB = new HSLColor(Mathf.Lerp(0.4f, 0.53f, Random.value), Mathf.Lerp(0.58f, 0.73f, 1f - Random.value * Random.value), Mathf.Lerp(0.28f, 0.76f, Random.value * Random.value));
            self.ColorA = new HSLColor(170f/360f, 0.92f, 0.72f);
            if (!KingFisherCWT.TryGetValue(self, out _)) {
                KingFisherCWT.Add(self, new KingFisherEx());
            }
        }
    }
    private static void VultureGraphics_InitiateSprites(On.VultureGraphics.orig_InitiateSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.vulture.Template.type == CreatureTemplateType.KingFisher && KingFisherCWT.TryGetValue(self, out var kingFisherEx)) {
            kingFisherEx.startIndex = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
            sLeaser.sprites[kingFisherEx.startIndex] = new FSprite("pixel", true);
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    private static void Vulture_DropMask(ILContext il)
    {
        var cursor = new ILCursor(il);
        var label = il.DefineLabel();
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStfld(out _))) {
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate((Vulture self, Vector2 violenceDir) => {
            if (self.Template.type == CreatureTemplateType.KingFisher) {
                FisherMaskAbstract abstractFisherMask = new FisherMaskAbstract(self.room.world, self.abstractPhysicalObject.pos, self.room.game.GetNewID())
                {
                    colorSeed = self.abstractCreature.ID.RandomSeed
                };
                self.room.abstractRoom.AddEntity(abstractFisherMask);
                abstractFisherMask.pos = self.abstractCreature.pos;
                abstractFisherMask.RealizeInRoom();
                abstractFisherMask.realizedObject.firstChunk.HardSetPosition(self.bodyChunks[4].pos);
                abstractFisherMask.realizedObject.firstChunk.vel = self.bodyChunks[4].vel + violenceDir;
                (abstractFisherMask.realizedObject as FisherMask).fallOffVultureMode = 1f;
                return true;
            }
            return false;
        });

        cursor.Emit(OpCodes.Brtrue, label);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(1), i => i.MatchStfld(out _))) {
            return;
        }
        cursor.MarkLabel(label);
    }
    private static void SlugcatHand_Update(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchIsinst(nameof(VultureMask)))) {
            return;
        }
        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdarg(0))) {
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((SlugcatHand self) => {
			if ((self.owner.owner as Creature)?.grasps[self.limbNumber].grabbed is FisherMask fisherMask)
			{
				self.relativeHuntPos *= 1f - fisherMask.donned;
			}
        });
    }
    private static void ScavengerAI_CollectScore_Physobj_bool(ILContext il)
    {
        var cursor = new ILCursor(il);
        var label = il.DefineLabel();
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchIsinst(nameof(Mushroom)))) {
            return;
        }
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(1))) {
            return;
        }
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Isinst, typeof(FisherMask));
        cursor.Emit(OpCodes.Brfalse, label);
        cursor.Emit(OpCodes.Ldc_I4, 7);
        cursor.Emit(OpCodes.Ret);
        cursor.MarkLabel(label);
    }
    private static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
    {
        return orig(self, obj) && obj is not FisherMask;
    }
    private static void IL_LizardAI_UpdateDynamicRelationship(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchIsinst(nameof(Spear)))) {
            return;
        }
        if (!cursor.TryGotoPrev(MoveType.Before, i => i.MatchLdarg(1))) {
            return;
        }
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldloc, 4);
        cursor.EmitDelegate((RelationshipTracker.DynamicRelationship dRelation, int i) => {
            if (dRelation.trackerRep.representedCreature.realizedCreature.grasps[i].grabbed is FisherMask) {
                (dRelation.state as LizardAI.LizardTrackState).vultureMask = Math.Max((dRelation.state as LizardAI.LizardTrackState).vultureMask, 2);
            }
        });
    }
    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            Player.ObjectGrabability result = orig(self, obj);
            if (obj is FisherMask)
            {
                return Player.ObjectGrabability.OneHand;
            }
            return result;
        }
    private static void VultureGraphics_ctor(On.VultureGraphics.orig_ctor orig, VultureGraphics self, Vulture ow)
    {
        orig(self, ow);
        if (self.vulture.Template.type == CreatureTemplateType.KingFisher) {
            self.ColorA = new HSLColor(170f/360f, 0.63f, 0.52f);
            self.ColorB = new HSLColor(170f/360f, 0.92f, 0.72f);
            KingFisherCWT.Add(self, new KingFisherEx());
        }
    }
    private static float TuskBend(On.KingTusks.Tusk.orig_TuskBend orig, KingTusks.Tusk self, float f)
    {
        float origValue = orig(self, f);
        if (self.vulture.Template.type != CreatureTemplateType.KingFisher) {
            return origValue;
        }
        return 0.3f*Mathf.Cos(Mathf.Pow(3.75f*f, 0.85f)) + (0.45f*Mathf.Cos(Mathf.Pow(1.15f*f,0.85f))) - 0.3f;
    }
    private static float TuskProfBend(On.KingTusks.Tusk.orig_TuskProfBend orig, KingTusks.Tusk self, float f)
    {
        float origValue = orig(self, f);
        if (self.vulture.Template.type != CreatureTemplateType.KingFisher) {
            return origValue;
        }
        return 0.3f*Mathf.Cos(Mathf.Pow(3.75f*f, 0.85f)) + (0.45f*Mathf.Cos(Mathf.Pow(1.15f*f,0.85f))) - 0.3f;
    }
    // A copy-paste of the normal Vulture DrawSprites method, but edited to make the barbs on the King Fisher's tusks.
    // Done by just moving one of the verticies of the trianglemesh back and to the side.
    private static void Tusk_DrawSprites(On.KingTusks.Tusk.orig_DrawSprites orig, KingTusks.Tusk self, VultureGraphics vGraphics, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (self.vulture.Template.type != CreatureTemplateType.KingFisher) {
            orig(self, vGraphics, sLeaser, rCam, timeStacker, camPos);
            return;
        }

        if (vGraphics.shadowMode)
        {
            camPos.y -= rCam.room.PixelHeight + 300f;
        }
        if (ModManager.MMF)
        {
            self.UpdateTuskColors(sLeaser);
        }
        Vector2 a = Vector2.Lerp(self.vulture.bodyChunks[4].lastPos, self.vulture.bodyChunks[4].pos, timeStacker);
        Vector2 vector = Custom.DirVec(Vector2.Lerp(self.vulture.neck.tChunks[self.vulture.neck.tChunks.Length - 1].lastPos, self.vulture.neck.tChunks[self.vulture.neck.tChunks.Length - 1].pos, timeStacker), Vector2.Lerp(self.vulture.bodyChunks[4].lastPos, self.vulture.bodyChunks[4].pos, timeStacker));
        Vector2 a2 = Custom.PerpendicularVector(vector);
        Vector2 vector2 = Vector3.Slerp(self.lastZRot, self.zRot, timeStacker);
        float num = Mathf.Lerp(self.lastLaserAlpha, self.laserAlpha, timeStacker);
        Color color = Custom.HSL2RGB(vGraphics.ColorB.hue, 1f, 0.5f);
        if (self.mode == KingTusks.Tusk.Mode.Charging)
        {
            num = ((self.modeCounter % 6 < 3) ? 1f : 0f);
            if (self.modeCounter % 2 == 0)
            {
                color = Color.Lerp(color, Color.white, Random.value);
            }
        }
        Vector2 vector3 = a + vector * 15f + a2 * Vector3.Slerp(Custom.DegToVec(self.owner.lastHeadRot + ((self.side == 0) ? -90f : 90f)), Custom.DegToVec(self.owner.headRot + ((self.side == 0) ? -90f : 90f)), timeStacker).x * 7f;
        Vector2 vector4 = self.AimDir(timeStacker);
        if (num <= 0f)
        {
            sLeaser.sprites[self.LaserSprite(vGraphics)].isVisible = false;
        }
        else
        {
            sLeaser.sprites[self.LaserSprite(vGraphics)].isVisible = true;
            sLeaser.sprites[self.LaserSprite(vGraphics)].alpha = num;
            Vector2 corner = Custom.RectCollision(vector3, vector3 + vector4 * 100000f, rCam.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(rCam.room, vector3, corner);
            if (intVector != null)
            {
                corner = Custom.RectCollision(corner, vector3, rCam.room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
            }
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite).verticeColors[0] = Custom.RGB2RGBA(color, num);
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite).verticeColors[1] = Custom.RGB2RGBA(color, num);
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite).verticeColors[2] = Custom.RGB2RGBA(color, Mathf.Pow(num, 2f) * ((self.mode == KingTusks.Tusk.Mode.Charging) ? 1f : 0.5f));
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite).verticeColors[3] = Custom.RGB2RGBA(color, Mathf.Pow(num, 2f) * ((self.mode == KingTusks.Tusk.Mode.Charging) ? 1f : 0.5f));
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite)?.MoveVertice(0, vector3 + vector4 * 2f + Custom.PerpendicularVector(vector4) * 0.5f - camPos);
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite)?.MoveVertice(1, vector3 + vector4 * 2f - Custom.PerpendicularVector(vector4) * 0.5f - camPos);
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite)?.MoveVertice(2, corner - Custom.PerpendicularVector(vector4) * 0.5f - camPos);
            (sLeaser.sprites[self.LaserSprite(vGraphics)] as CustomFSprite)?.MoveVertice(3, corner + Custom.PerpendicularVector(vector4) * 0.5f - camPos);
        }
        Vector2 vector5 = (Vector2.Lerp(self.chunkPoints[0, 1], self.chunkPoints[0, 0], timeStacker) + Vector2.Lerp(self.chunkPoints[1, 1], self.chunkPoints[1, 0], timeStacker)) / 2f;
        Vector2 vector6 = Custom.DirVec(Vector2.Lerp(self.chunkPoints[1, 1], self.chunkPoints[1, 0], timeStacker), Vector2.Lerp(self.chunkPoints[0, 1], self.chunkPoints[0, 0], timeStacker));
        Vector2 a3 = Custom.PerpendicularVector(vector6);
        if (self.mode == KingTusks.Tusk.Mode.Charging)
        {
            vector5 += vector6 * Mathf.Lerp(-6f, 6f, Random.value);
        }
        Vector2 vector7 = a - vector6 * 10f;
        Vector2 vector8 = Vector2.Lerp(a, vector5, Mathf.InverseLerp(0f, 0.25f, self.attached));
        sLeaser.sprites[vGraphics.NeckLumpSprite(self.side)].x = vector7.x - camPos.x;
        sLeaser.sprites[vGraphics.NeckLumpSprite(self.side)].y = vector7.y - camPos.y;
        sLeaser.sprites[vGraphics.NeckLumpSprite(self.side)].scaleY = (Vector2.Distance(vector7, vector8) + 4f) / 20f;
        sLeaser.sprites[vGraphics.NeckLumpSprite(self.side)].rotation = Custom.AimFromOneVectorToAnother(vector7, vector8);
        sLeaser.sprites[vGraphics.NeckLumpSprite(self.side)].scaleX = 0.6f;
        Vector2 vector9 = vector5 + vector6 * -35f + a3 * vector2.y * ((self.side == 0) ? -1f : 1f) * -15f;
        float num2 = 0f;
        for (int i = 0; i < KingTusks.Tusk.tuskSegs; i++)
        {
            float num3 = Mathf.InverseLerp(0f, (float)(KingTusks.Tusk.tuskSegs - 1), (float)i);
            Vector2 vector10 = vector5 + vector6 * Mathf.Lerp(-30f, 60f, num3) + self.TuskBend(num3) * a3 * 20f * vector2.x + self.TuskProfBend(num3) * a3 * vector2.y * ((self.side == 0) ? -1f : 1f) * 10f;
            Vector2 normalized = (vector10 - vector9).normalized;
            Vector2 a4 = Custom.PerpendicularVector(normalized);
            float d = Vector2.Distance(vector10, vector9) / 5f;
            float num4 = self.TuskRad(num3, Mathf.Abs(vector2.y));
            // Edits start here
            float additionalx = 0f;
            float additionaly = 0f;
            if (i == 13 && self.vulture.Template.type == CreatureTemplateType.KingFisher) { additionalx = 7f; additionaly = 10f; }
            if (i == 11 && self.vulture.Template.type == CreatureTemplateType.KingFisher) { additionalx = 8.75f; additionaly = 12f; }
            // Debug.Log($"Chimerical: Vector2 {vector2}, and {self.side}");

            (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4, vector9 - a4 * (num4 + num2) * 0.5f + normalized * d - camPos);
            (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 1, vector9 + a4 * (num4 + num2) * 0.5f + normalized * d - camPos);
            if (i == KingTusks.Tusk.tuskSegs - 1)
            {
                (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 2, vector10 + normalized * d - camPos);
            }
            else
            {
                // Originally I tried to make these using ternary operations, but that did not work and was too hard to follow along/debug
                // Also, vector2.x and vector2.y are responsible for the 'Z-axis' rotation's influence on the x and y axis, respectively, it would seem
                if (self.side == 1) {
                    if (vector2.x <= 0) {
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 2, vector10 - a4 * (num4 + additionalx * -vector2.x) - normalized * (d + additionaly) - camPos);
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 3, vector10 + a4 * num4 - normalized * d - camPos);
                    }
                    else {
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 2, vector10 - a4 * num4- normalized * d - camPos);
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 3, vector10 + a4 * (num4 + additionalx * vector2.x) - normalized * (d + additionaly) - camPos);
                    }
                }
                if (self.side == 0) {
                    if (vector2.x >= 0) {
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 2, vector10 - a4 * num4- normalized * d - camPos);
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 3, vector10 + a4 * (num4 + additionalx * vector2.x) - normalized * (d + additionaly) - camPos);
                    }
                    else {
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 2, vector10 - a4 * (num4 + additionalx * -vector2.x) - normalized * (d + additionaly) - camPos);
                        (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh)?.MoveVertice(i * 4 + 3, vector10 + a4 * num4 - normalized * d - camPos);
                    }
                }
            }
            // Edits done
            num2 = num4;
            vector9 = vector10;
        }
        for (int j = 0; j < (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh).vertices.Length; j++)
        {
            (sLeaser.sprites[self.TuskDetailSprite(vGraphics)] as TriangleMesh)?.MoveVertice(j, (sLeaser.sprites[self.TuskSprite(vGraphics)] as TriangleMesh).vertices[j]);
        }
        if (self.lastWireLoose > 0f || self.wireLoose > 0f)
        {
            sLeaser.sprites[vGraphics.TuskWireSprite(self.side)].isVisible = true;
            float num5 = Mathf.Lerp(self.lastWireLoose, self.wireLoose, timeStacker);
            vector9 = a - vector * 14f;
            for (int k = 0; k < self.wire.GetLength(0); k++)
            {
                Vector2 vector11 = Vector2.Lerp(self.wire[k, 1], self.wire[k, 0], timeStacker);
                if (num5 < 1f)
                {
                    vector11 = Vector2.Lerp(Vector2.Lerp(a - vector * 14f, vector5 + vector6 * 6f, Mathf.InverseLerp(0f, (float)(self.wire.GetLength(0) - 1), (float)k)), vector11, num5);
                }
                if (k == self.wire.GetLength(0) - 1)
                {
                    vector11 = self.WireAttachPos(timeStacker);
                }
                Vector2 normalized2 = (vector11 - vector9).normalized;
                Vector2 b = Custom.PerpendicularVector(normalized2);
                float d2 = Vector2.Distance(vector11, vector9) / 5f;
                if (k == self.wire.GetLength(0) - 1)
                {
                    d2 = 0f;
                }
                (sLeaser.sprites[vGraphics.TuskWireSprite(self.side)] as TriangleMesh)?.MoveVertice(k * 4, vector9 - b + normalized2 * d2 - camPos);
                (sLeaser.sprites[vGraphics.TuskWireSprite(self.side)] as TriangleMesh)?.MoveVertice(k * 4 + 1, vector9 + b + normalized2 * d2 - camPos);
                (sLeaser.sprites[vGraphics.TuskWireSprite(self.side)] as TriangleMesh)?.MoveVertice(k * 4 + 2, vector11 - b - normalized2 * d2 - camPos);
                (sLeaser.sprites[vGraphics.TuskWireSprite(self.side)] as TriangleMesh)?.MoveVertice(k * 4 + 3, vector11 + b - normalized2 * d2 - camPos);
                vector9 = vector11;
            }
            return;
        }
        sLeaser.sprites[vGraphics.TuskWireSprite(self.side)].isVisible = false;
    }
    #pragma warning restore CS8602
}