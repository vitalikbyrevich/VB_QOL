namespace VBQOL
{
    [HarmonyPatch(typeof(AutoJumpLedge), nameof(AutoJumpLedge.OnTriggerStay))]
    public static class VB_BetterLaders
    {
        private static bool Prefix(AutoJumpLedge __instance, Collider collider)
        {
            if (!(collider.GetComponent<Character>() is Player player) || player != Player.m_localPlayer) return true;

            float ledgeAngle = __instance.gameObject.transform.rotation.eulerAngles.y;
            float playerAngle = player.transform.rotation.eulerAngles.y;
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(ledgeAngle, playerAngle));

            if (angleDiff <= 12f)
            {
                Vector3 position = player.transform.position;
                float yOffset = player.m_running ? 0.08f : 0.06f;
                player.transform.position = new Vector3(position.x, position.y + yOffset, position.z) + player.transform.forward * 0.08f;
            }
            return false;
        }
    }
}