namespace VBQOL.Debuging
{
    [HarmonyPatch]
    public static class VB_PickableDebug
    {
        private static bool _isDebugging;
        private static readonly Dictionary<string, List<string>> _errorLog = new Dictionary<string, List<string>>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
        private static void PickableAwake_Prefix(Pickable __instance)
        {
            try
            {
                _isDebugging = true;
                string prefabName = GetPrefabNameSafe(__instance);
               // Debug.Log($" - STARTED - Pickable.Awake on \"{prefabName}\"");
                
                TrackPickableStart(prefabName);
            }
            catch (Exception e)
            {
                LogError("PickableAwake_Prefix", e, __instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
        private static void PickableAwake_Postfix(Pickable __instance)
        {
            try
            {
                string prefabName = GetPrefabNameSafe(__instance);
              //  Debug.Log($" - FINISHED - Pickable.Awake on \"{prefabName}\"");
                _isDebugging = false;
                
                TrackPickableCompletion(prefabName);
            }
            catch (Exception e)
            {
                LogError("PickableAwake_Postfix", e, __instance);
                _isDebugging = false;
            }
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
        private static void PickableAwake_Finalizer(Pickable __instance, Exception __exception)
        {
            if (__exception != null) LogError("Pickable.Awake_Original", __exception, __instance);
        }

        [HarmonyPatch, HarmonyWrapSafe]
        private static class LogZNetViewRegister_bool
        {
            private static MethodBase TargetMethod()
            {
                try
                {
                    MethodInfo genericMethodDefinition = AccessTools.Method(typeof(ZNetView), nameof(ZNetView.Register), new Type[] { typeof(string), typeof(Action<long, object>) });

                    if (genericMethodDefinition == null)
                    {
                        var allRegisterMethods = typeof(ZNetView).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == nameof(ZNetView.Register) && m.IsGenericMethodDefinition);

                        foreach (var m in allRegisterMethods)
                        {
                            var parameters = m.GetParameters();
                            if (parameters.Length == 2 &&
                                parameters[0].ParameterType == typeof(string) &&
                                parameters[1].ParameterType.IsGenericType &&
                                parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<,>))
                            {
                                genericMethodDefinition = m;
                                break;
                            }
                        }
                    }

                    if (genericMethodDefinition == null) 
                        throw new Exception("Не удалось найти определение дженерик-метода ZNetView.Register<T>");

                    MethodInfo specificGenericMethod = genericMethodDefinition.MakeGenericMethod(typeof(bool));
                    return specificGenericMethod;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Error in TargetMethod for bool: {e}");
                    throw;
                }
            }

            [HarmonyPrefix]
            private static void ZNetViewRegister_Prefix_bool(ZNetView __instance, string name)
            {
                if (!_isDebugging) return;
                
                try
                {
                //    Debug.Log($" --- STARTED - ZNetView.Register(name=\"{name}\") on \"{GetPrefabNameSafe(__instance)}\"");
                }
                catch (Exception e)
                {
                    LogError("ZNetViewRegister_Prefix_bool", e, __instance);
                }
            }

            [HarmonyPostfix]
            private static void ZNetViewRegister_Postfix_bool(ZNetView __instance, string name)
            {
                if (!_isDebugging) return;
                
                try
                {
                 //   Debug.Log($" --- FINISHED - ZNetView.Register(name=\"{name}\") on \"{GetPrefabNameSafe(__instance)}\"");
                }
                catch (Exception e)
                {
                    LogError("ZNetViewRegister_Postfix_bool", e, __instance);
                }
            }

            [HarmonyFinalizer]
            private static void ZNetViewRegister_Finalizer_bool(Exception __exception, ZNetView __instance, string name)
            {
                if (__exception != null) LogError($"ZNetView.Register({name})_bool", __exception, __instance);
            }
        }

        [HarmonyPatch, HarmonyWrapSafe]
        private static class LogZNetViewRegister_int
        {
            private static MethodBase TargetMethod()
            {
                try
                {
                    MethodInfo genericMethodDefinition = AccessTools.Method(typeof(ZNetView), nameof(ZNetView.Register), new Type[] { typeof(string), typeof(Action<long, object>) });

                    if (genericMethodDefinition == null)
                    {
                        var allRegisterMethods = typeof(ZNetView).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == nameof(ZNetView.Register) && m.IsGenericMethodDefinition);

                        foreach (var m in allRegisterMethods)
                        {
                            var parameters = m.GetParameters();
                            if (parameters.Length == 2 &&
                                parameters[0].ParameterType == typeof(string) &&
                                parameters[1].ParameterType.IsGenericType &&
                                parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<,>))
                            {
                                genericMethodDefinition = m;
                                break;
                            }
                        }
                    }

                    if (genericMethodDefinition == null) throw new Exception("Не удалось найти определение дженерик-метода ZNetView.Register<T>");

                    MethodInfo specificGenericMethod = genericMethodDefinition.MakeGenericMethod(typeof(int));
                    return specificGenericMethod;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Error in TargetMethod for int: {e}");
                    throw;
                }
            }

            [HarmonyPrefix]
            private static void ZNetViewRegister_Prefix_int(ZNetView __instance, string name)
            {
                if (!_isDebugging) return;
                
                try
                {
                  //  Debug.Log($" --- STARTED - ZNetView.Register(name=\"{name}\") on \"{GetPrefabNameSafe(__instance)}\"");
                }
                catch (Exception e)
                {
                    LogError("ZNetViewRegister_Prefix_int", e, __instance);
                }
            }

            [HarmonyPostfix]
            private static void ZNetViewRegister_Postfix_int(ZNetView __instance, string name)
            {
                if (!_isDebugging) return;
                
                try
                {
                  //  Debug.Log($" --- FINISHED - ZNetView.Register(name=\"{name}\") on \"{GetPrefabNameSafe(__instance)}\"");
                }
                catch (Exception e)
                {
                    LogError("ZNetViewRegister_Postfix_int", e, __instance);
                }
            }

            [HarmonyFinalizer]
            private static void ZNetViewRegister_Finalizer_int(Exception __exception, ZNetView __instance, string name)
            {
                if (__exception != null) LogError($"ZNetView.Register({name})_int", __exception, __instance);
            }
        }

        // Отслеживание начала инициализации Pickable
        private static readonly Dictionary<string, DateTime> _pickableStartTimes = new Dictionary<string, DateTime>();
        private static readonly HashSet<string> _completedPickables = new HashSet<string>();

        private static void TrackPickableStart(string prefabName)
        {
            _pickableStartTimes[prefabName] = DateTime.Now;
        }

        private static void TrackPickableCompletion(string prefabName)
        {
            _completedPickables.Add(prefabName);
            if (_pickableStartTimes.ContainsKey(prefabName))
            {
                var duration = DateTime.Now - _pickableStartTimes[prefabName];
                if (duration.TotalSeconds > 1.0) UnityEngine.Debug.LogWarning($"Pickable {prefabName} took {duration.TotalSeconds:F2}s to initialize (slow)");
                _pickableStartTimes.Remove(prefabName);
            }
        }

        // Периодическая проверка "зависших" Pickable
        private static DateTime _lastStuckCheck = DateTime.Now;
        
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Update))]
        private static class StuckPickableMonitor
        {
            [HarmonyPostfix]
            private static void CheckForStuckPickables()
            {
                if ((DateTime.Now - _lastStuckCheck).TotalSeconds < 5) return; // Проверяем каждые 5 секунд
                
                _lastStuckCheck = DateTime.Now;
                var now = DateTime.Now;
                
                foreach (var kvp in _pickableStartTimes.ToList())
                {
                    var duration = now - kvp.Value;
                    if (duration.TotalSeconds > 10.0) // Если Pickable "висит" больше 10 секунд
                    {
                        UnityEngine.Debug.LogError($"STUCK PICKABLE DETECTED: {kvp.Key} has been initializing for {duration.TotalSeconds:F2}s");
                        LogError("StuckPickable", new TimeoutException($"Pickable {kvp.Key} stuck in initialization"), null);
                        _pickableStartTimes.Remove(kvp.Key);
                    }
                }
            }
        }

        // Улучшенный безопасный метод получения имени префаба
        private static string GetPrefabNameSafe(Object obj)
        {
            try
            {
                if (!obj) return "NULL_OBJECT";
                return GetPrefabName(obj);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Ошибка при получении prefab: {e}");
                return "ERROR_GETTING_NAME";
            }
        }

        // Централизованное логирование ошибок
        private static void LogError(string methodName, Exception e, Object context)
        {
            try
            {
                string contextInfo = "Unknown";
                if (context)
                {
                    try
                    {
                        contextInfo = GetPrefabNameSafe(context);
                    }
                    catch
                    {
                        contextInfo = context.GetType().Name;
                    }
                }

                string errorKey = $"{methodName}_{contextInfo}";
                string errorMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {methodName} on {contextInfo}: {e.GetType().Name} - {e.Message}";

                if (!_errorLog.ContainsKey(errorKey)) _errorLog[errorKey] = new List<string>();
                _errorLog[errorKey].Add(errorMessage);

                UnityEngine.Debug.LogError($"Ошибка в {methodName} on {contextInfo}: {e}");
                UnityEngine.Debug.LogError($"Stack trace: {e.StackTrace}");

                // Если это первая ошибка для этого ключа, логируем дополнительную информацию
                if (_errorLog[errorKey].Count == 1) UnityEngine.Debug.LogError($"Первое появление ошибки для {contextInfo} in {methodName}");
            }
            catch (Exception logException)
            {
                UnityEngine.Debug.LogError($"КРИТИЧНО: не удалось зарегистрировать ошибку: {logException}");
            }
        }

        // Команда для просмотра ошибок
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
        private static class ErrorReportCommands
        {
            [HarmonyPostfix]
            private static void AddErrorCommands()
            {
                new Terminal.ConsoleCommand("vb_terminal_pickable_errors", "Показать ошибки связанные с Pickable", (args) =>
                {
                    args.Context.AddString("=== PICKABLE Ошибки ===");
                    
                    if (_errorLog.Count == 0)
                    {
                        args.Context.AddString("Ошибки не найдены.");
                        return;
                    }

                    foreach (var kvp in _errorLog)
                    {
                        args.Context.AddString($"{kvp.Key}: {kvp.Value.Count} ошибок");
                        foreach (var error in kvp.Value.Take(3)) args.Context.AddString($"  - {error}");
                        if (kvp.Value.Count > 3) args.Context.AddString($"  ... и {kvp.Value.Count - 3} более");
                    }
                    
                    args.Context.AddString($"Общее количество ошибок: {_errorLog.Count}");
                });

                new Terminal.ConsoleCommand("vb_terminal_pickable_status", "Показать общий статус Pickable", (args) =>
                {
                    args.Context.AddString("=== PICKABLE Общий статус ===");
                    args.Context.AddString($"Выполняется инициализация: {_pickableStartTimes.Count}");
                    args.Context.AddString($"Выполнено успешно: {_completedPickables.Count}");
                    args.Context.AddString($"Всего ошибок: {_errorLog.Sum(x => x.Value.Count)}");
                    
                    foreach (var kvp in _pickableStartTimes)
                    {
                        var duration = DateTime.Now - kvp.Value;
                        args.Context.AddString($"Стак: {kvp.Key} - {duration.TotalSeconds:F1}s");
                    }
                });
            }
        }

        public static string GetPrefabName<T>(this T obj) where T : UnityEngine.Object
        {
            var prefabName = Utils.GetPrefabName(obj.name);
            for (var i = 0; i < 80; i++)
            {
                var replace = prefabName.Replace($" ({i})", "");
                if(prefabName.Length == replace.Length) break;
                prefabName = replace;
            }
            return prefabName;
        }
    }
}