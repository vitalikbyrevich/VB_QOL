using Debug = UnityEngine.Debug;

namespace VBQOL;

[HarmonyPatch]
public class VB_EquipInWater
{
    internal static ConfigEntry<string> EiW_Custom;
    internal static HashSet<string> EiW_CustomStrings = new();

    public static bool VB_CheckWaterItem(ItemDrop.ItemData item)
    {
        if (item == null)
        {
            HandleNullItemCase();
            return false;
        }
        return IsItemAllowed(item.m_shared.m_name);
    }

    private static void HandleNullItemCase()
    {
        var player = Player.m_localPlayer;
        if (!player) return;

        CheckAndUnequipItem(player.m_leftItem);
        CheckAndUnequipItem(player.m_rightItem);
    }

    private static void CheckAndUnequipItem(ItemDrop.ItemData item)
    {
        if (item != null && !IsItemAllowed(item.m_dropPrefab.name)) Player.m_localPlayer?.UnequipItem(item);
    }

    private static bool IsItemAllowed(string itemName)
    {
        return EiW_CustomStrings.Contains(itemName);
    }

    private static IEnumerable<MethodBase> TargetMethods() =>
    [
        AccessTools.Method(typeof(Player), nameof(Player.Update)),
        AccessTools.Method(typeof(Humanoid), nameof(Humanoid.EquipItem)),
        AccessTools.Method(typeof(Humanoid), nameof(Humanoid.UpdateEquipment))
    ];

    private static readonly CodeMatch[] Matches =
    [
        new CodeMatch(OpCodes.Ldarg_0),
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.IsSwimming))),
        new CodeMatch(OpCodes.Brfalse),
        new CodeMatch(OpCodes.Ldarg_0),
        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.IsOnGround)))
    ];

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var codeMatcher = new CodeMatcher(instructions).MatchStartForward(Matches);
        
        try
        {
            switch (original.Name)
            {
                case nameof(Player.Update):
                    codeMatcher.Advance(1).RemoveInstructions(6);
                    break;
                    
                case nameof(Humanoid.EquipItem):
                    codeMatcher.Advance(6).Insert(GenerateInjectionCode(codeMatcher, OpCodes.Ldarg_1));
                    break;
                    
                case nameof(Humanoid.UpdateEquipment):
                    codeMatcher.Advance(6).Insert(GenerateInjectionCode(codeMatcher, OpCodes.Ldnull));
                    break;
            }
        }
        catch (ArgumentException ex)
        {
            Debug.LogError($"IL Transpiler Error in VB_EquipInWater: {ex.Message}");
            Debug.LogError("Plugin initialization failed. Valheim will shutdown.");
            Environment.Exit(1);
        }
        
        return codeMatcher.InstructionEnumeration();
    }

    private static List<CodeInstruction> GenerateInjectionCode(CodeMatcher matcher, OpCode loadOpCode)
    {
        return new List<CodeInstruction>
        {
            new CodeInstruction(loadOpCode),
            new CodeInstruction(OpCodes.Call, typeof(VB_EquipInWater).GetMethod(nameof(VB_CheckWaterItem))),
            new CodeInstruction(OpCodes.Brfalse, matcher.InstructionAt(-1).operand)
        };
    }
}