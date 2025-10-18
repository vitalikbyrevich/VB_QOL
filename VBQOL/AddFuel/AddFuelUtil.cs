namespace VBQOL.AddFuel;

public static class AddFuelUtil
{
    public static ConfigEntry<KeyCode> AFModifierKeyConfig;
    public static KeyCode AFModifierKeyUseConfig = KeyCode.E;
    public static ConfigEntry<string> AFTextConfig;
    public static ConfigEntry<bool> AFEnable;

    public static int CalculateStackToAdd(bool isSingleMode, int availableStack, int spaceLeft) => isSingleMode ? 1 : Math.Min(availableStack, spaceLeft);
}