using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EasternExpansion;

class LanternKelp : PhysicalObject, IDrawable
{
    // private List<PhysicalObject> fruit;
    private int age;
    private int segments = 20;
    private float width = 3f;
    private Leaf[] leaves;
    private int leaveSegments = 4;
    private Vector2[] optimalPositions;
    private Vector2 floorPos = Vector2.zero;
    public LanternKelp(Room room, Vector2 pos, AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject) {
        float yPos = pos.y;
        while (yPos > -100f) {
            if (room.GetTile(new Vector2(pos.x, yPos)).Solid) {
                break;
            }
            yPos--;
        }
        floorPos = new Vector2(pos.x, yPos);
        Debug.Log($"EExpansion, found floor at {floorPos}");

        this.room = room;
        bodyChunks = new BodyChunk[segments];
        optimalPositions = new Vector2[segments];
        bodyChunkConnections = new BodyChunkConnection[(segments-1)*2];
        for (int i = 0; i < segments; i++) {
            bodyChunks[i] = new BodyChunk(this, i, Vector2.zero, width/2f, 0.001f);
            if (i != 0) {
                bodyChunkConnections[(i-1)*2] = new BodyChunkConnection(bodyChunks[i], bodyChunks[i-1], Mathf.Min(width*0.95f, Vector2.Distance(pos, floorPos)*0.9f), BodyChunkConnection.Type.Normal, 0.3f, 1f);
                bodyChunkConnections[(i-1)*2+1] = new BodyChunkConnection(bodyChunks[i-1], bodyChunks[i], Mathf.Min(width*0.95f, Vector2.Distance(pos, floorPos)*0.9f), BodyChunkConnection.Type.Normal, 0.2f, 1f);
            }
        }
        leaves = new Leaf[Random.Range(3, 11)];
        for (int i = 0; i < leaves.Length; i++) {
            leaves[i] = new Leaf(this, Random.Range(3, 19));
        }
		airFriction = 0.7f;
        waterFriction = 0.5f;
		gravity = 1f;
		surfaceFriction = 0.875f;
        bounce = 0.1f;
		collisionLayer = 1;
		buoyancy = 1f;
        PositionStem(pos);
    }
    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1 + leaves.Length];
        sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments, true, true);
        sLeaser.sprites[0].color = new Color(26f/255f, 33f/255f, 0f/255f);
        for (int i = 0; i < leaves.Length; i++) {
            sLeaser.sprites[1+i] = TriangleMesh.MakeLongMesh(leaveSegments, true, true);
            sLeaser.sprites[1+i].color = Custom.HSL2RGB(108f/360f, 0.4f, 0.5f);
        }
        AddToContainer(sLeaser, rCam, null);
    }
    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Debug.Log("EExpansion Drawing sprites");
        for (int i = 0; i < segments; i++) {
            Vector2 chunk1Pos = Vector2.Lerp(bodyChunks[i].lastPos, bodyChunks[i].pos, timeStacker);
            if (i != segments-1) {
                Vector2 chunk2Pos = Vector2.Lerp(bodyChunks[i+((i==segments-2)?0:1)].lastPos, bodyChunks[i+((i==segments-2)?0:1)].pos, timeStacker);
                (sLeaser.sprites[0] as TriangleMesh)?.MoveVertice(i * 4, chunk1Pos + (-width * Custom.PerpendicularVector(Custom.DirVec(chunk1Pos, chunk2Pos))) - camPos);
                (sLeaser.sprites[0] as TriangleMesh)?.MoveVertice(i * 4 + 1, chunk1Pos + (width * Custom.PerpendicularVector(Custom.DirVec(chunk1Pos, chunk2Pos))) - camPos);
                (sLeaser.sprites[0] as TriangleMesh)?.MoveVertice(i * 4 + 2, chunk2Pos + (-width * Custom.PerpendicularVector(Custom.DirVec(chunk1Pos, chunk2Pos))) - camPos);
                (sLeaser.sprites[0] as TriangleMesh)?.MoveVertice(i * 4 + 3, chunk2Pos + (width * Custom.PerpendicularVector(Custom.DirVec(chunk1Pos, chunk2Pos))) - camPos);
            }
            else {
                Vector2 prevChunkPos = Vector2.Lerp(bodyChunks[i-1].lastPos, bodyChunks[i-1].pos, timeStacker);
                (sLeaser.sprites[0] as TriangleMesh)?.MoveVertice(i * 4, prevChunkPos + (-width * Custom.PerpendicularVector(Custom.DirVec(prevChunkPos, chunk1Pos))) - camPos);
                (sLeaser.sprites[0] as TriangleMesh)?.MoveVertice(i * 4 + 1, prevChunkPos + (width * Custom.PerpendicularVector(Custom.DirVec(prevChunkPos, chunk1Pos))) - camPos);
                (sLeaser.sprites[0] as TriangleMesh)?.MoveVertice(i * 4 + 2, chunk1Pos - camPos);
                // room.AddObject(new Spark(chunks[i,0], new Vector2(5,5), Color.blue, null, 10, 20));
            }
        }
        for (int i = 1; i < leaves.Length; i++) {
            for (int j = 0; j < leaveSegments; j++) {
                if (j!=leaves[i].chunkData.GetLength(0)-1) {
                    (sLeaser.sprites[i] as TriangleMesh)?.MoveVertice(j*4, leaves[i].chunkData[j,0] + (-1.5f * Custom.PerpendicularVector(Custom.DirVec(leaves[i].chunkData[j,0], leaves[i].chunkData[j+1,0]))) - camPos);
                    (sLeaser.sprites[i] as TriangleMesh)?.MoveVertice(j*4+1, leaves[i].chunkData[j,0] + (1.5f * Custom.PerpendicularVector(Custom.DirVec(leaves[i].chunkData[j,0], leaves[i].chunkData[j+1,0]))) - camPos);
                    (sLeaser.sprites[i] as TriangleMesh)?.MoveVertice(j*4+2, leaves[i].chunkData[j+1,0] + (-1.5f * Custom.PerpendicularVector(Custom.DirVec(leaves[i].chunkData[j,0], leaves[i].chunkData[j+1,0]))) - camPos);
                    (sLeaser.sprites[i] as TriangleMesh)?.MoveVertice(j*4+3, leaves[i].chunkData[j+1,0] + (1.5f * Custom.PerpendicularVector(Custom.DirVec(leaves[i].chunkData[j,0], leaves[i].chunkData[j+1,0]))) - camPos);
                }
                else {

                }
            }
        }
    }
    public override void Update(bool eu)
    {
        for (int i = 0; i < bodyChunks.Length; i++) {
            bodyChunks[i].vel += Custom.DirVec(bodyChunks[i].pos, optimalPositions[i]) * 0.65f * Mathf.InverseLerp(1f, 0f, bodyChunks[i].vel.magnitude);
        }
        base.Update(eu);
        bodyChunks[0].pos = floorPos;
        for (int i = 0; i < leaves.Length; i++) {
            leaves[i].Update();
        }
    }
    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (otherObject == this) { return; }
        base.Collide(otherObject, myChunk, otherChunk);
        if (otherObject.bodyChunks[otherChunk].vel.magnitude > 0.1) {
            bodyChunks[myChunk].vel += 5f * otherObject.bodyChunks[otherChunk].vel;
        }
    }
    public void PositionStem(Vector2 startPos) {
        for (int i = 0; i < bodyChunks.Length; i++) {
            Debug.Log($"EExpansion: chunk {i}");
            bodyChunks[i].pos = new Vector2(floorPos.x+StemCurve(Mathf.InverseLerp(0, segments-1, i)),0) + Vector2.up*Mathf.Lerp(floorPos.y, startPos.y, (float)i/segments);
            Debug.Log($"EExpansion: chunk {i} pos: {bodyChunks[i].pos}");
            bodyChunks[i].lastPos = bodyChunks[i].pos;
            bodyChunks[i].lastLastPos = bodyChunks[i].lastPos;
            bodyChunks[i].vel = Vector2.zero;
            optimalPositions[i] = bodyChunks[i].pos;
        }
    }
    public float StemCurve(float x) {
        // Debug.Log($"EExpansion: x: {x}\n{20f*Mathf.Sin(2f*Mathf.PI*x)}");
        return 20f*Mathf.Sin(2f*Mathf.PI*x);
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        return;
    }
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Midground");
        }
        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContatiner.AddChild(fsprite);
        }
    }
    class Leaf
    {
        public Leaf(LanternKelp kelp, int connectedBodyChunk)
        {
            this.kelp = kelp;
            this.connectedBodyChunk = connectedBodyChunk;
            chunkData = new Vector2[kelp.leaveSegments, 3]; // 0->pos    1->lastPos  2->vel
            for (int i = 0; i < chunkData.GetLength(0); i++) {
                chunkData[i,0] = kelp.bodyChunks[connectedBodyChunk].pos;
                chunkData[i,1] = chunkData[i,0];
                chunkData[i,2] = Vector2.zero;
            }
        }
        public void Update()
        {
            for (int i = 0; i < chunkData.GetLength(0); i++) {
                chunkData[i,1] = chunkData[i,0];
                chunkData[i,0] += chunkData[i,2];
                chunkData[i,2] *= 0.7f;
                if (i==0) {
                    chunkData[i,0] = kelp.bodyChunks[connectedBodyChunk].pos;
                }
                if (i < chunkData.GetLength(0)-1 && Vector2.Distance(chunkData[i,0], chunkData[i+1,0]) > 3f) {
                    chunkData[i+1,2] += Custom.DirVec(chunkData[i+1,0], chunkData[i,0])*3f;
                    Debug.Log($"Leaf {i+1} position {chunkData[i+1,0]}, vel {chunkData[i+1,2]}");
                }
            }
        }
        public int connectedBodyChunk;
        public LanternKelp kelp;
        public Vector2[,] chunkData;
    }
}