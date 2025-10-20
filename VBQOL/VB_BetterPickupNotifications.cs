namespace VBQOL
{
	[HarmonyPatch(typeof(MessageHud))]
	public static class VB_BetterPickupNotifications
	{
		public static float MessageLifetime = 4f;
		public static float MessageFadeTime = 4f;
		public static float MessageBumpTime = 4f;
		public static bool ResetMessageTimerOnDupePickup = false;
		public static float MessageVerticalSpacingModifier = 1f;
		public static float MessageTextHorizontalSpacingModifier = 1.5f;
		public static float MessageTextVerticalModifier = 1f;
		
		private static List<PickupMessage> PickupMessages;
		private static List<PickupDisplay> PickupDisplays;

		[HarmonyPrefix]
		[HarmonyPatch(nameof(MessageHud.ShowMessage))]
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
					Timer = MessageLifetime
				};
                PickupDisplays[num].Display(PickupMessages[num]);
			}
			else
			{
                PickupMessages[num].m_amount += amount;
                if (ResetMessageTimerOnDupePickup) PickupMessages[num].Timer = MessageLifetime;
                else
                {
                    PickupMessages[num].Timer += MessageBumpTime;
					if (PickupMessages[num].Timer > MessageLifetime) PickupMessages[num].Timer = MessageLifetime;
					PickupDisplays[num].Display(PickupMessages[num]);
				}
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(MessageHud.UpdateMessage))]
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
		[HarmonyPatch(nameof(MessageHud.Awake))]
		public static void AwakePostfix()
		{
            PickupMessages = new List<PickupMessage>();
            PickupDisplays = new List<PickupDisplay>();
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(MessageHud.OnDestroy))]
		public static void OnDestroyPostfix()
		{
			foreach (var display in PickupDisplays)
			{
				if (display?.RootGO) Object.Destroy(display.RootGO);
			}
			PickupDisplays.Clear();
			PickupMessages.Clear();

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
				RootGO.transform.position += Vector3.up * -(IconComp.rectTransform.rect.height * MessageVerticalSpacingModifier) * (Index + 1);
				TextComp.gameObject.transform.position += Vector3.up * -IconComp.rectTransform.rect.height * MessageTextVerticalModifier + Vector3.right * IconComp.rectTransform.rect.width * MessageTextHorizontalSpacingModifier;
			}

			public void Display(PickupMessage msg)
			{
				try
				{
					if (!TextComp || !IconComp) return;

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
					if (msg != null && msg.m_icon)
					{
						CreateUI();
						Display(msg);
					}
				}
			}

			public void FadeAway()
			{
                TextComp.CrossFadeAlpha(0f, MessageFadeTime, true);
				IconComp.CrossFadeAlpha(0f, MessageFadeTime, true);
			}
			public GameObject RootGO;
			private Image IconComp;
			private TMP_Text TextComp;
			private int Index;
		}
	}
}
