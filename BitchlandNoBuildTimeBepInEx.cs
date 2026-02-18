using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BitchlandNoBuildTimeBepInEx
{
    [BepInPlugin("com.wolfitdm.BitchlandNoBuildTimeBepInEx", "BitchlandNoBuildTimeBepInEx Plugin", "1.0.0.0")]
    public class BitchlandNoBuildTimeBepInEx : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private static ConfigEntry<bool> configEnableMe;

        public BitchlandNoBuildTimeBepInEx()
        {
        }

        public static Type MyGetType(string originalClassName)
        {
            return Type.GetType(originalClassName + ",Assembly-CSharp");
        }

        public static Type MyGetTypeUnityEngine(string originalClassName)
        {
            return Type.GetType(originalClassName + ",UnityEngine");
        }

        private static string pluginKey = "General.Toggles";

		public static bool enableMe = false;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            configEnableMe = Config.Bind(pluginKey,
                                              "EnableMe",
                                              true,
                                             "Whether or not you want enable this mod (default true also yes, you want it, and false = no)");
            
			enableMe = configEnableMe.Value;
			
			PatchAllHarmonyMethods();

            Logger.LogInfo($"Plugin BitchlandNoBuildTimeBepInEx BepInEx is loaded!");
        }
		
		public static void PatchAllHarmonyMethods()
        {
			if (!enableMe)
            {
                return;
            }
			
            try
            {
                PatchHarmonyMethodUnity(typeof(WeaponSystem), "Update", "WeaponSystem_Update", true, false);
            } catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        public static void PatchHarmonyMethodUnity(Type originalClass, string originalMethodName, string patchedMethodName, bool usePrefix, bool usePostfix, Type[] parameters = null)
        {
            string uniqueId = "com.wolfitdm.BitchlandNoBuildTimeBepInEx";
            Type uniqueType = typeof(BitchlandNoBuildTimeBepInEx);

            // Create a new Harmony instance with a unique ID
            var harmony = new Harmony(uniqueId);

            if (originalClass == null)
            {
                Logger.LogInfo($"GetType originalClass == null");
                return;
            }

            MethodInfo patched = null;

            try
            {
                patched = AccessTools.Method(uniqueType, patchedMethodName);
            }
            catch (Exception ex)
            {
                patched = null;
            }

            if (patched == null)
            {
                Logger.LogInfo($"AccessTool.Method patched {patchedMethodName} == null");
                return;

            }

            // Or apply patches manually
            MethodInfo original = null;

            try
            {
                if (parameters == null)
                {
                    original = AccessTools.Method(originalClass, originalMethodName);
                }
                else
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
            }
            catch (AmbiguousMatchException ex)
            {
                Type[] nullParameters = new Type[] { };
                try
                {
                    if (patched == null)
                    {
                        parameters = nullParameters;
                    }

                    ParameterInfo[] parameterInfos = patched.GetParameters();

                    if (parameterInfos == null || parameterInfos.Length == 0)
                    {
                        parameters = nullParameters;
                    }

                    List<Type> parametersN = new List<Type>();

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo parameterInfo = parameterInfos[i];

                        if (parameterInfo == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name.StartsWith("__"))
                        {
                            continue;
                        }

                        Type type = parameterInfos[i].ParameterType;

                        if (type == null)
                        {
                            continue;
                        }

                        parametersN.Add(type);
                    }

                    parameters = parametersN.ToArray();
                }
                catch (Exception ex2)
                {
                    parameters = nullParameters;
                }

                try
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
                catch (Exception ex2)
                {
                    original = null;
                }
            }
            catch (Exception ex)
            {
                original = null;
            }

            if (original == null)
            {
                Logger.LogInfo($"AccessTool.Method original {originalMethodName} == null");
                return;
            }

            HarmonyMethod patchedMethod = new HarmonyMethod(patched);
            var prefixMethod = usePrefix ? patchedMethod : null;
            var postfixMethod = usePostfix ? patchedMethod : null;

            harmony.Patch(original,
                prefix: prefixMethod,
                postfix: postfixMethod);
        }

		public static bool WeaponSystem_Update(object __instance)
        {
            if (!enableMe)
            {
                return true;
            }

            WeaponSystem _this = (WeaponSystem)__instance;

            try
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(_this.transform.position, _this.transform.TransformDirection(Vector3.forward), out hitInfo, _this.RayDistance, (int)_this.PromptLayers))
                {
                    Interactible[] obj__ = hitInfo.transform.GetComponents<Interactible>();
                    Interactible[] obj2__ = hitInfo.transform.root.GetComponents<Interactible>();

                    if (obj__ != null)
                    {
                        for (int i = 0; i < obj__.Length; i++)
                        {
                            Interactible obj_ = obj__[i];

                            if (obj_ == null)
                            {
                                continue;
                            }

                            if (!(obj_ is int_ConstructionPlan))
                            {
                                continue;
                            }

                            int_ConstructionPlan obj = (int_ConstructionPlan)obj_;

                            if (obj != null)
                            {
                                obj.AllResourcesIn = true;
                                obj.BuiltProgress = obj.BuiltProgresspointsNeeded;
                            }
                        }
                    }

                    if (obj2__ != null)
                    {
                        for (int i = 0; i < obj2__.Length; i++)
                        {
                            Interactible obj2_ = obj2__[i];

                            if (obj2_ == null)
                            {
                                continue;
                            }

                            if (!(obj2_ is int_ConstructionPlan))
                            {
                                continue;
                            }

                            int_ConstructionPlan obj2 = (int_ConstructionPlan)obj2_;

                            if (obj2 != null)
                            {
                                obj2.AllResourcesIn = true;
                                obj2.BuiltProgress = obj2.BuiltProgresspointsNeeded;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }


            return true;
        }
    }
}