using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace SpawnAtAltar
{
    [BepInPlugin(modGUID, modName, modVersion)]
    
    public class SpawnAtAltar : BaseUnityPlugin
    {
        private const string modGUID = "blacks7ar.SpawnAtAltar";
        private const string modName = "SpawnAtAltar";
        private const string modVersion = "1.0.0.0";

        private void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony(modGUID).PatchAll(assembly);
        }
        
        [HarmonyPatch(typeof(Game), "FindSpawnPoint")]
        private class GameStart_Patch
        {
            private static void Postfix(Game __instance, out Vector3 point, out bool usedLogoutPoint, float dt)
            {
                __instance.m_respawnWait += dt;
                usedLogoutPoint = false;
                if (__instance.m_playerProfile.HaveLogoutPoint() || __instance.m_playerProfile.HaveCustomSpawnPoint())
                {
                    PlayerProfile playerProfile =
                        (PlayerProfile)Traverse.Create(__instance).Field("m_playerProfile").GetValue();
                    Vector3 customSpawnPoint = playerProfile.GetCustomSpawnPoint();
                    customSpawnPoint.y += 0.5f;
                    ZNet.instance.SetReferencePosition(customSpawnPoint);
                    if (__instance.m_respawnWait > 8f && ZNetScene.instance.IsAreaReady(customSpawnPoint))
                    {
                        point = customSpawnPoint;
                        //__instance.m_playerProfile.ClearCustomSpawnPoint();
                        __instance.m_playerProfile.ClearLoguoutPoint();
                        __instance.m_respawnWait = 0f;
                        usedLogoutPoint = false;
                    }
                    point = Vector3.zero;
                }
                else
                {
                    if (ZoneSystem.instance.GetLocationIcon(__instance.m_StartLocation, out var pos))
                    {
                        point = pos + Vector3.up * 2f;
                        ZNet.instance.SetReferencePosition(point);
                        ZNetScene.instance.IsAreaReady(point);
                    }
                    point = Vector3.zero;
                }
                
                ZNet.instance.SetReferencePosition(Vector3.zero);
                point = Vector3.zero;
            }
        }
    }
}