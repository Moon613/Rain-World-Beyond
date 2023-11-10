using System.Diagnostics.CodeAnalysis;

namespace EasternExpansion;
public sealed class CreatureTemplateType {
    [AllowNull] public static CreatureTemplate.Type KingFisher = new (nameof(KingFisher), true);
}
public static class SandboxUnlockID {
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID KingFisher = new(nameof(KingFisher), true);
}
public static class EnumExt_VultureMask
{
    [AllowNull] public static readonly AbstractPhysicalObject.AbstractObjectType FisherMask = new AbstractPhysicalObject.AbstractObjectType("FisherMask", true);
    [AllowNull] public static readonly MultiplayerUnlocks.SandboxUnlockID FisherMaskUnlock = new MultiplayerUnlocks.SandboxUnlockID("FisherMaskUnlock", true);
}