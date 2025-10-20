namespace VBQOL
{
    [HarmonyPatch]
    public class VB_BlastFurnaceTalesAll
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
        private static void BlastFurnacePatch(ref Smelter __instance)
        {
            if (__instance.m_name != "$piece_blastfurnace") return;
            foreach (Smelter.ItemConversion item in new List<Smelter.ItemConversion>
              {
                  new Smelter.ItemConversion
                  {
                      m_from = ObjectDB.instance.GetItemPrefab("CopperOre").GetComponent<ItemDrop>(),
                      m_to = ObjectDB.instance.GetItemPrefab("Copper").GetComponent<ItemDrop>()
                  },
                  new Smelter.ItemConversion
                  {
                      m_from = ObjectDB.instance.GetItemPrefab("CopperScrap").GetComponent<ItemDrop>(),
                      m_to = ObjectDB.instance.GetItemPrefab("Copper").GetComponent<ItemDrop>()
                  },
                  new Smelter.ItemConversion
                  {
                      m_from = ObjectDB.instance.GetItemPrefab("TinOre").GetComponent<ItemDrop>(),
                      m_to = ObjectDB.instance.GetItemPrefab("Tin").GetComponent<ItemDrop>()
                  },
                  new Smelter.ItemConversion
                  {
                      m_from = ObjectDB.instance.GetItemPrefab("IronOre").GetComponent<ItemDrop>(),
                      m_to = ObjectDB.instance.GetItemPrefab("Iron").GetComponent<ItemDrop>()
                  },
                  new Smelter.ItemConversion
                  {
                      m_from = ObjectDB.instance.GetItemPrefab("IronScrap").GetComponent<ItemDrop>(),
                      m_to = ObjectDB.instance.GetItemPrefab("Iron").GetComponent<ItemDrop>()
                  },
                  new Smelter.ItemConversion
                  {
                      m_from = ObjectDB.instance.GetItemPrefab("SilverOre").GetComponent<ItemDrop>(),
                      m_to = ObjectDB.instance.GetItemPrefab("Silver").GetComponent<ItemDrop>()
                  }
              })
            {
                __instance.m_conversion.Add(item);
            }
        }
    }
}