using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System;
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
            // Create a new Harmony instance with a unique ID
            var harmony = new Harmony("com.wolfitdm.BitchlandMoreWeaponSlotsBepInEx");

            if (originalClass == null)
            {
                Logger.LogInfo($"GetType originalClass == null");
                return;
            }

            // Or apply patches manually
            MethodInfo original = null;

            if (parameters == null)
            {
                original = AccessTools.Method(originalClass, originalMethodName);
            } else
            {
                original = AccessTools.Method(originalClass, originalMethodName, parameters);
            }

            if (original == null)
            {
                Logger.LogInfo($"AccessTool.Method original {originalMethodName} == null");
                return;
            }

            MethodInfo patched = AccessTools.Method(typeof(BitchlandNoBuildTimeBepInEx), patchedMethodName);

            if (patched == null)
            {
                Logger.LogInfo($"AccessTool.Method patched {patchedMethodName} == null");
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
                    int_ConstructionPlan obj = hitInfo.transform.GetComponent<int_ConstructionPlan>();
                    int_ConstructionPlan obj2 = hitInfo.transform.root.GetComponent<int_ConstructionPlan>();

                    if (obj != null)
                    {
                        obj.AllResourcesIn = true;
                        obj.BuiltProgress = obj.BuiltProgresspointsNeeded;
                    }

                    if (obj2  != null)
                    {
                        obj2.AllResourcesIn = true;
                        obj2.BuiltProgress = obj2.BuiltProgresspointsNeeded;
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