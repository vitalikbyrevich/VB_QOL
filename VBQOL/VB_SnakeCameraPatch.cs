using static VBQOL.VB_SnakeCameraPatch.SneakConfig;

namespace VBQOL
{
	[HarmonyPatch]
    public class VB_SnakeCameraPatch
	{
		internal class SneakConfig
		{
			public static CrouchPosition LowerCameraWhen = CrouchPosition.Crouching;
			public static float CameraHeightReduction = 0.5f;
			internal const float CameraMin = 0f;
			internal const float CameraMax = 1f;

			public static bool ChangeCollisionCrouchHeight = true;
			public static float CrouchHeightMultiplier = 0.8f;
			internal const float HeightMultMin = 0.75f;
			internal const float HeightMultMax = 1f;

			public enum CrouchPosition
			{
				Disabled,
				Crouching,
				CrouchStanding,
				CrouchWalking
			}
		}

	//	[HarmonyPatch]
		internal class CameraPatches
		{
			[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.GetCameraBaseOffset)), HarmonyPostfix]
			private static void GetCameraBaseOffset(Player player, ref Vector3 __result)
			{
				if (player != Player.m_localPlayer) return;

				if (LowerCameraWhen == CrouchPosition.Disabled) return;

				// if the base game or another mod changed the default, abort
				if (__result != player.m_eye.transform.position - player.transform.position) return;

				bool isCrouching = player.IsCrouching() && player.IsOnGround();
				if (!isCrouching) return;
				bool isCrouchWalking = player.IsSneaking();

				if ((LowerCameraWhen == CrouchPosition.Crouching /*&& isCrouching*/)
					|| (LowerCameraWhen == CrouchPosition.CrouchWalking && isCrouchWalking)
					|| (LowerCameraWhen == CrouchPosition.CrouchStanding && !isCrouchWalking))
					__result += Vector3.up * -Mathf.Clamp(CameraHeightReduction, CameraMin, CameraMax);
			}
		}
			private static float? standingSize;
			private static float? crouchedSize;

			[HarmonyPatch(typeof(Player), nameof(Player.FixedUpdate)), HarmonyPostfix]
			private static void FixedUpdate(Player __instance)
			{
				if (__instance != Player.m_localPlayer) return;
				if (!ChangeCollisionCrouchHeight) return;
				var collider = __instance.GetCollider();
				if (!collider) return;
				if (standingSize == null)
				{
					standingSize = collider.height;
					CalculateCrouchedSize();
				}
				var isCrouched = __instance.IsCrouching() && __instance.IsOnGround();
				if (crouchedSize != null) collider.height = isCrouched ? crouchedSize.Value : standingSize.Value;
				collider.center = new Vector3(0f, collider.height / 2f, 0f);
			}

			internal static void CalculateCrouchedSize()
			{
				if (standingSize == null /*|| CrouchHeightMultiplier == null*/) return;
				crouchedSize = standingSize.Value * Mathf.Clamp(CrouchHeightMultiplier, HeightMultMin, HeightMultMax);
			}
	}
}