using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using UnityEngine;
using DevInterface;
using RWCustom;
namespace EasternExpansion;

sealed class KingFisher : Critob{
    public KingFisher() : base(CreatureTemplateType.KingFisher)
    {
        Icon = new SimpleIcon("Kill_KingVulture", Color.green);
        RegisterUnlock(KillScore.Configurable(25), SandboxUnlockID.KingFisher);
        SandboxPerformanceCost = new(3f, 1.5f);
        LoadedPerformanceCost = 20f;
        ShelterDanger = ShelterDanger.TooLarge;
        CreatureName = "King Fisher";
        KingFisherHooks.Apply();
    }
    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.KingVulture, Type, "KingFisher")
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.KingVulture)
        }.IntoTemplate();
        t.shortcutColor = Color.green;
        t.waterVision = 1;
        t.throughSurfaceVision = 1;
        t.movementBasedVision = 0.2f;
        t.waterPathingResistance = 0;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.lungCapacity = 2400;
        return t;
    }
    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        if (map.room.GetTile(connection.destinationCoord).AnyWater && !(map.room.GetTile(new IntVector2(connection.destinationCoord.x, connection.destinationCoord.y+8)).AnyWater || map.room.GetTile(connection.destinationCoord).Solid)) {
            allow = true;
        }
    }
    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow)
    {
        if (map.room.GetTile(tilePos).AnyWater && !(map.room.GetTile(new IntVector2(tilePos.x, tilePos.y+8)).AnyWater || map.room.GetTile(tilePos).Solid)) {
            allow = true;
        }
    }
    public override int ExpeditionScore() => 20;
    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.green;
    public override string DevtoolsMapName(AbstractCreature acrit) => "kngfshr";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] {RoomAttractivenessPanel.Category.All};
    public override IEnumerable<string> WorldFileAliases() => new[] {"kingfisher"};
    public override void EstablishRelationships()
    {
        Relationships fisher = new Relationships(Type);
        fisher.Ignores(Type);
    }
    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new VultureAI(acrit, acrit.world);
    public override AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => new VultureAbstractAI(acrit.world, acrit);
    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Vulture(acrit, acrit.world);
    public override CreatureState CreateState(AbstractCreature acrit) => new Vulture.VultureState(acrit);
    public override void LoadResources(RainWorld rainWorld) {}
    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.KingVulture;
}