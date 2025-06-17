namespace VBQOL
{
	[HarmonyPatch(typeof(MessageHud))]
	public static class VB_BetterPickupNotifications
	{
		public static ConfigEntry<float> MessageLifetime;
		public static ConfigEntry<float> MessageFadeTime;
		public static ConfigEntry<float> MessageBumpTime;
		public static ConfigEntry<bool> ResetMessageTimerOnDupePickup;
		public static ConfigEntry<float> MessageVerticalSpacingModifier;
		public static ConfigEntry<float> MessageTextHorizontalSpacingModifier;
		public static ConfigEntry<float> MessageTextVerticalModifier;
		
		private static List<PickupMessage> PickupMessages;
		private static List<PickupDisplay> PickupDisplays;

		[HarmonyPrefix]
		[HarmonyPatch("ShowMessage")]
		public static bool ShowMessagePrefix(MessageHud __instance, MessageHud.MessageType type, string text, int amount, Sprite icon)
		{
			if (Hud.IsUserHidden()) return false;
			text = Localization.instance.Localize(text);
			if (type == MessageHud.MessageType.Center || string.IsNullOrWhiteSpace(text) || amount < 1 || icon == null) return true;
			int num = 0;
			while (num < PickupMessages.Count && (PickupMessages[num] == null || !(PickupMessages[num].m_text == text))) num++;
			if (num == PickupMessages.Count)
			{
				num = PickupMessages.IndexOf(null);
				if (num < 0)
				{
					num = PickupMessages.Count;
                    PickupMessages.Add(null);
                    PickupDisplays.Add(new PickupDisplay(num));
				}
                PickupMessages[num] = new PickupMessage
                {
					m_text = text,
					m_amount = amount,
					m_icon = icon,
					Timer = MessageLifetime.Value
				};
                PickupDisplays[num].Display(PickupMessages[num]);
			}
			else
			{
                PickupMessages[num].m_amount += amount;
                if (ResetMessageTimerOnDupePickup.Value) PickupMessages[num].Timer = MessageLifetime.Value;
                else
                {
                    PickupMessages[num].Timer += MessageBumpTime.Value;
					if (PickupMessages[num].Timer > MessageLifetime.Value) PickupMessages[num].Timer = MessageLifetime.Value;
					PickupDisplays[num].Display(PickupMessages[num]);
				}
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch("UpdateMessage")]
		public static bool UpdateMessagePrefix(MessageHud __instance, float dt)
		{
			for (int i = 0; i < PickupMessages.Count; i++)
			{
				if (PickupMessages[i] != null)
				{
                    PickupMessages[i].Timer -= dt;
					if (PickupMessages[i].Timer <= 0f)
					{
                        PickupMessages[i] = null;
                        PickupDisplays[i].FadeAway();
					}
				}
			}
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch("Awake")]
		public static void AwakePostfix()
		{
            PickupMessages = new List<PickupMessage>();
            PickupDisplays = new List<PickupDisplay>();
		}

		[HarmonyPostfix]
		[HarmonyPatch("OnDestroy")]
		public static void OnDestroyPostfix()
		{
            PickupMessages = null;
            PickupDisplays = null;
		}

		private class PickupMessage : MessageHud.MsgData
		{
			public float Timer;
		}

		private class PickupDisplay
		{
			public PickupDisplay(int index)
			{
				Index = index;
				CreateUI();
			}

			private void CreateUI()
			{
				RootGO = Object.Instantiate(MessageHud.instance.m_messageText.gameObject.transform.parent.gameObject, MessageHud.instance.m_messageText.gameObject.transform.parent.parent);
				RootGO.transform.SetAsFirstSibling();
				IconComp = RootGO.GetComponentInChildren<Image>();
				TextComp = RootGO.GetComponentInChildren<TMP_Text>();
				RootGO.transform.position += Vector3.up * -(IconComp.rectTransform.rect.height * MessageVerticalSpacingModifier.Value) * (Index + 1);
				TextComp.gameObject.transform.position += Vector3.up * -IconComp.rectTransform.rect.height * MessageTextVerticalModifier.Value + Vector3.right * IconComp.rectTransform.rect.width * MessageTextHorizontalSpacingModifier.Value;
			}

			public void Display(PickupMessage msg)
			{
				try
				{
					TextComp.canvasRenderer.SetAlpha(1f);
					TextComp.CrossFadeAlpha(1f, 0f, true);
					if (msg.m_amount > 1) TextComp.text = msg.m_text + " x" + msg.m_amount;
					else TextComp.text = msg.m_text;
					IconComp.sprite = msg.m_icon;
					IconComp.canvasRenderer.SetAlpha(1f);
					IconComp.CrossFadeAlpha(1f, 0f, true);
				}
				catch
				{
					if (msg != null && msg.m_icon != null)
					{
						CreateUI();
						Display(msg);
					}
				}
			}

			public void FadeAway()
			{
                TextComp.CrossFadeAlpha(0f, MessageFadeTime.Value, true);
				IconComp.CrossFadeAlpha(0f, MessageFadeTime.Value, true);
			}
			public GameObject RootGO;
			private Image IconComp;
			private TMP_Text TextComp;
			private int Index;
		}
	}
}
