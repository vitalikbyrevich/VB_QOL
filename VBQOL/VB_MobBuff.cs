using Debug = UnityEngine.Debug;

namespace VBQOL
{
    [HarmonyPatch]
    public class VB_MobBuff
    {
      /*  [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.ApplyDamage))]
        class TrackAttackerPatch
        {
            static void Prefix(Humanoid __instance, HitData hit)
            {
                try
                {
                    if (__instance is Player player && player.m_nview?.IsValid() == true)
                    {
                        Character attacker = hit.GetAttacker();
                        if (attacker?.m_nview?.IsValid() == true)
                        {
                            player.m_nview.GetZDO().Set("lastAttacker", attacker.m_nview.GetZDO().m_uid);
                            Debug.Log($"Установлен атакующий: {attacker.name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка в TrackAttackerPatch: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
        class PlayerDeathPatch
        {
            static void Postfix(Player __instance)
            {
                try
                {
                    if (__instance.m_nview?.IsValid() != true) return;

                    ZDOID attackerId = __instance.m_nview.GetZDO().GetZDOID("lastAttacker");
                    Debug.Log($"Найден ID атакующего: {attackerId}");

                    if (attackerId == ZDOID.None) return;

                    GameObject attackerObj = ZNetScene.instance.FindInstance(attackerId);
                    if (attackerObj == null)
                    {
                        Debug.Log("Не найден объект атакующего");
                        return;
                    }

                    if (attackerObj.TryGetComponent<Humanoid>(out var mob) && !mob.IsPlayer())
                    {
                        Debug.Log($"Обработка моба: {mob.name}");
                        ZDO mobZdo = mob.m_nview?.GetZDO();
                        if (mobZdo == null) return;

                        if (!mobZdo.GetBool("isEnhanced"))
                        {
                            Debug.Log("Первое убийство - клонируем");
                            CloneAndEnhanceMob(mob, __instance.GetPlayerName());
                        }
                        else
                        {
                            Debug.Log("Повторное убийство - усиливаем");
                            EnhanceExistingMob(mob, __instance.GetPlayerName());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка в PlayerDeathPatch: {ex}");
                }
            }
        }

        static void CloneAndEnhanceMob(Humanoid original, string playerName)
        {
            var originalPrefab = ZNetScene.instance.GetPrefab(original.m_nview.GetPrefabName());
            if (originalPrefab == null)
            {
                Debug.LogError("Не найден префаб оригинала");
                return;
            }

            var clone = Object.Instantiate(originalPrefab);
            var cloneView = clone.GetComponent<ZNetView>();

            // Генерируем уникальное имя
            string newName = $"{original.m_nview.GetPrefabName()}_{Guid.NewGuid():N}";

            // Устанавливаем имя через рефлексию
            typeof(ZNetView).GetField("m_prefabName", BindingFlags.NonPublic | BindingFlags.Instance)?
                .SetValue(cloneView, newName);

            // Настраиваем ZDO
            cloneView.GetZDO().Set("isEnhanced", true);
            cloneView.GetZDO().Set("enhanceCount", 1);
            cloneView.GetZDO().Set("originalPrefab", original.m_nview.GetPrefabName());

            // Копируем важные параметры
            var cloneHumanoid = clone.GetComponent<Humanoid>();
            cloneHumanoid.m_faction = original.m_faction;
            cloneHumanoid.m_name = original.m_name;

            // Заменяем оригинал
            ZNetScene.instance.Destroy(original.gameObject);
            ZNetScene.instance.SpawnObject(clone.transform.position, clone.transform.rotation, clone);

            Debug.Log($"Моб клонирован: {newName}");
            EnhanceExistingMob(cloneHumanoid, playerName);
        }

        static void EnhanceExistingMob(Humanoid mob, string playerName)
        {
            ZDO zdo = mob.m_nview.GetZDO();
            int count = zdo.GetInt("enhanceCount", 1);

            // Усиление характеристик
            mob.m_health *= 1.2f;
            //  mob.m_health = mob.GetMaxHealth();

            // Усиление атак
            foreach (ItemDrop attack in mob.GetComponentsInChildren<ItemDrop>())
            {
                attack.m_itemData.m_shared.m_attack.m_damageMultiplier *= 1.1f;
                attack.m_itemData.m_shared.m_attackForce *= 1.05f;
            }

            // Добавляем скальп
            AddScalpToMob(mob, playerName);

            // Обновляем счетчик
            zdo.Set("enhanceCount", count + 1);
        }

        static void AddScalpToMob(Humanoid mob, string playerName)
        {
            var drop = mob.GetComponent<CharacterDrop>();
            if (drop == null) drop = mob.gameObject.AddComponent<CharacterDrop>();

            if (drop.m_drops == null)
                drop.m_drops = new List<CharacterDrop.Drop>();

            drop.m_drops.Add(new CharacterDrop.Drop
            {
                m_prefab = CreateScalpPrefab(playerName),
                m_amountMin = 1,
                m_amountMax = 1,
                m_chance = 1f
            });
        }

        static GameObject CreateScalpPrefab(string playerName)
        {
            var scalp = Object.Instantiate(ZNetScene.instance.GetPrefab("TrophySkeleton"));
            var itemDrop = scalp.GetComponent<ItemDrop>();

            itemDrop.m_itemData.m_shared.m_name = $"Скальп {playerName}";
            itemDrop.m_itemData.m_shared.m_description = $"Трофей: {playerName}";

            return scalp;
        }*/
        
           [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
           class Patch_PlayerDeath
           {
               static void Postfix(Player __instance, HitData hit)
               {
                   var attacker = hit.GetAttacker();
                   if (attacker is Humanoid mob && !mob.IsPlayer())
                   {
                       if (!mob.m_nview.GetZDO().GetBool("hasCloned")) CloneAndEnhanceMob(mob, __instance.GetPlayerName());
                       else EnhanceMob(mob, __instance.GetPlayerName());
                   }
               }

               static void CloneAndEnhanceMob(Humanoid originalMob, string playerName)
               {
                   // Сохраняем позицию и ориентацию
                   var pos = originalMob.transform.position;
                   var rot = originalMob.transform.rotation;

                   // Удаляем оригинального моба
                   Object.Destroy(originalMob.gameObject);

                   // Получаем префаб усиленного моба
                   var prefab = ZNetScene.instance.GetPrefab(originalMob.m_nview.GetPrefabName());
                   if (prefab == null)
                   {
                       Debug.LogError("Префаб 'EnhancedMob' не найден в ZNetScene");
                       return;
                   }

                   // Создаём нового моба
                   var newMobGO = UnityEngine.Object.Instantiate(prefab, pos, rot);
                   var newMob = newMobGO.GetComponent<Character>();
                   if (newMob == null)
                   {
                       Debug.LogError("Префаб 'EnhancedMob' не содержит компонент Character");
                       return;
                   }

                   // Проверяем наличие ZNetView
                   var znetView = newMob.GetComponent<ZNetView>();
                   if (znetView != null && znetView.IsValid()) znetView.GetZDO().Set("hasCloned", true);
                   else Debug.LogWarning("ZNetView отсутствует или недействителен у клонированного моба");

                   // Усиливаем моба и добавляем скальп
                   EnhanceMob(newMob, playerName);
               }


               static void EnhanceMob(Character mob, string playerName)
               {
                   // Усиление здоровья
                   mob.SetMaxHealth(mob.GetMaxHealth() + 100);
                   mob.Heal(100f); // Чтобы сразу восстановить здоровье

                   // Усиление атак
                   var attacks = mob.GetComponentsInChildren<ItemDrop>();
                   foreach (var attack in attacks)
                   {
                       attack.m_itemData.m_shared.m_attack.m_damageMultiplier *= 1.5f;
                       attack.m_itemData.m_shared.m_attackForce *= 1.2f;
                   }

                   // Добавление скальпа
                   var scalp = CreateScalpItem(playerName);
                   var drop = mob.GetComponent<CharacterDrop>();
                   if (drop != null)
                   {
                       drop.m_drops.Add(new CharacterDrop.Drop
                       {
                           m_prefab = scalp,
                           m_chance = 1f,
                           m_amountMin = 1,
                           m_amountMax = 1
                       });
                   }
               }


               static GameObject CreateScalpItem(string playerName)
               {
                   var item = Object.Instantiate(ZNetScene.instance.GetPrefab("TrophySkeleton"));
                   item.name = $"Scalp_{playerName}";
                   var itemData = item.GetComponent<ItemDrop>().m_itemData;
                   itemData.m_customData["owner"] = playerName;
                   itemData.m_shared.m_name = $"Скальп {playerName}";
                   return item;
               }
           }
    }
}