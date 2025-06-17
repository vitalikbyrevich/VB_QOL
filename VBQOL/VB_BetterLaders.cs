namespace VBQOL
{
    [HarmonyPatch]
    public class VB_BetterLaders
    {
        [HarmonyPatch(typeof(AutoJumpLedge), "OnTriggerStay")]
        public static class Ladder_Patch
        {
            private static bool Prefix(AutoJumpLedge __instance, Collider collider)
            {
                Character component = collider.GetComponent<Character>();
                if (component && component == Player.m_localPlayer)
                {
                    Vector3 position = component.transform.position;
                    float y = __instance.gameObject.transform.rotation.eulerAngles.y;
                    float y2 = component.transform.rotation.eulerAngles.y;
                    float num = Math.Abs(Mathf.DeltaAngle(y, y2));
                    if (num <= 12f)
                    {
                        if (!component.m_running) component.transform.position = new Vector3(position.x, position.y + 0.06f, position.z) + component.transform.forward * 0.08f;
                        else component.transform.position = new Vector3(position.x, position.y + 0.08f, position.z) + component.transform.forward * 0.08f;
                    }
                }
                return !(component == Player.m_localPlayer);
            }
        }
    }
}