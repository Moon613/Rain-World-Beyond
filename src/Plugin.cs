using BepInEx;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using System;
using Fisobs.Core;
using BepInEx.Logging;
using System.Diagnostics.CodeAnalysis;


#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete

namespace EasternExpansion;


[BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
public class Plugin : BaseUnityPlugin
{
    [AllowNull] new internal static ManualLogSource Logger;
    public Plugin() {
        Logger = base.Logger;
    }
    public const string MOD_ID = "eastern.expansion";
    public const string MOD_NAME = "Eastern Expansion";
    public const string MOD_VERSION = "1.0";
    internal bool init = false;
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += OnModsInit;
        Content.Register(new KingFisher());
        Content.Register(new FisherMaskFisob());
    }
    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
        orig(self);

        if (!init) {
            init = true;
            try {
                Futile.atlasManager.LoadAtlas("atlases/FisherMark");
                Futile.atlasManager.LoadAtlas("atlases/FisherDrop");
                Futile.atlasManager.LoadAtlas("atlases/FisherArenaSprites");
            } catch (Exception err) {
                Logger.LogError(err);
            }
        }
    }
}