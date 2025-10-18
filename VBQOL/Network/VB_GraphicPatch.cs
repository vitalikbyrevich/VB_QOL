namespace VBQOL.Network
{
	[HarmonyPatch]
    public class VB_GraphicPatch
    {
	    internal static ConfigEntry<bool> soft_particles;
	    internal static ConfigEntry<bool> soft_vegetation;
	    internal static ConfigEntry<int> particle_raycast_budget;
	    internal static ConfigEntry<float> streaming_mipmaps_memory_budget;
	    internal static ConfigEntry<float> lod_bias;
	    internal static ConfigEntry<int> anti_aliasing;
	    internal static ConfigEntry<AnisotropicFiltering> anisotropic_filtering;
        
	    internal static ConfigEntry<float>  grass_distance;
	    internal static ConfigEntry<float>  grass_playerPushFade;
	    internal static ConfigEntry<float>  grass_amountScale;
	    internal static ConfigEntry<float> grass_size;

	    public static void SetGraphicsSettings()
	    {
		    QualitySettings.softParticles = soft_particles.Value;
		    QualitySettings.particleRaycastBudget = particle_raycast_budget.Value;
		    QualitySettings.softVegetation = soft_vegetation.Value;
		    QualitySettings.streamingMipmapsMemoryBudget = streaming_mipmaps_memory_budget.Value;
		    QualitySettings.lodBias = lod_bias.Value;
		    QualitySettings.anisotropicFiltering = anisotropic_filtering.Value;
		    QualitySettings.antiAliasing = anti_aliasing.Value;
		    UpdateClutterSystem();
	    }

	    private static void UpdateClutterSystem()
	    {
		    ClutterSystem clutterSystem = Object.FindObjectOfType<ClutterSystem>();
		    if (clutterSystem)
		    {
			    clutterSystem.m_grassPatchSize = grass_size.Value;
			    clutterSystem.m_distance = grass_distance.Value;
			    clutterSystem.m_playerPushFade = grass_playerPushFade.Value;
			    clutterSystem.m_amountScale = grass_amountScale.Value;
		    }
	    }
	    
	    [HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake))]
		private static class ClutterSystem_Awake_Patch
		{
			private static void Prefix(ClutterSystem __instance)
			{
				__instance.m_grassPatchSize = grass_size.Value;
				__instance.m_distance = grass_distance.Value;
				__instance.m_playerPushFade = grass_playerPushFade.Value;
				__instance.m_amountScale = grass_amountScale.Value;
			}
		}
    }
}