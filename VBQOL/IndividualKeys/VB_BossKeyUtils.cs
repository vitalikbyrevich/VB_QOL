namespace VBQOL.IndividualKeys;

public class VB_BossKeyUtils
{
    public static bool IsBossKey(string key) => !string.IsNullOrEmpty(key) && key.StartsWith("defeated_");

    public static bool PlayerHasBossKey(Player player, string key) => player && player.m_customData.ContainsKey(key);

    public static bool AllPlayersHaveKey(string key, List<Player> players)
    {
        foreach (var p in players) if (!PlayerHasBossKey(p, key)) return false;
        return true;
    }

    public static bool AllNearPlayersHaveKey(List<Player> players, string key) => AllPlayersHaveKey(key, players);

    public static bool AllPlayersInEventRadiusHaveKey(string key, Vector3 pos, float radius)
    {
        foreach (var ped in RandEventSystem.s_playerEventDatas)
        {
            if (Utils.DistanceXZ(ped.position, pos) <= radius)
            {
                foreach (var peer in ZNet.instance.GetPeers())
                {
                    if (!peer.IsReady()) continue;
                    if (Utils.DistanceXZ(peer.m_refPos, ped.position) < 2f && !peer.m_serverSyncedPlayerData.ContainsKey(key)) return false;
                }
                if (Utils.DistanceXZ(ZNet.instance.GetReferencePosition(), ped.position) < 2f && !ZNet.instance.m_serverSyncedPlayerData.ContainsKey(key)) return false;
            }
        }
        return true;
    }
}