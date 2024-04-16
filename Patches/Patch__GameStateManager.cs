using HarmonyLib;
using Hikaria.PlayerSpawnApart.API;
using Hikaria.PlayerSpawnApart.Managers;
using SNetwork;

namespace Hikaria.PlayerSpawnApart.Patches;

[HarmonyPatch(typeof(GameStateManager))]
public class Patch__GameStateManager
{
    [HarmonyPatch(nameof(GameStateManager.IsEveryoneReady))]
    [HarmonyPostfix]
    private static void GameStateManager__IsEveryoneReady__Postfix(ref bool __result)
    {
        if (!SNet.IsMaster) return;

        if (__result) __result = PlayerSpawnApartManager.IsEveryoneReady;
    }

    [HarmonyPatch("set_IsReady")]
    [HarmonyPrefix]
    private static void GameStateManager__set_IsReady__Prefix(ref bool value)
    {
        if (!PlayerSpawnApartManager.IsReady)
        {
            if (value)
            {
                GameEventLogManager.AddLog("<color=orange>[PlayerSpawnApart]</color> <color=red>You must choose a slot before get ready!</color>");
                GameEventLogManager.AddLog("<color=orange>[PlayerSpawnApart]</color> Type in '/psa help' to show helps.");
            }
            value = false;
        }
    }

    [HarmonyPatch(nameof(GameStateManager.OnResetSession))]
    [HarmonyPostfix]
    private static void GameStateManager__OnResetSession__Postfix()
    {
        PlayerSpawnApartManager.ResetLocalSpawnApartSlot();
        Logs.LogMessage($"ResetLocalSpawnApartSlot: OnResetSession");
    }

    [HarmonyPatch(nameof(GameStateManager.OnLevelCleanup))]
    [HarmonyPostfix]
    private static void GameStateManager__OnLevelCleanup__Postfix()
    {
        PlayerSpawnApartManager.ResetLocalSpawnApartSlot();
        Logs.LogMessage($"ResetLocalSpawnApartSlot: OnLevelCleanup");
    }

    [HarmonyPatch(nameof(GameStateManager.SendGameState))]
    [HarmonyPostfix]
    private static void GameStateManager__SendGameState__Postfix()
    {
        SNetworkAPI.SendCustomData<pPlayerSpawnApartSlot>();
    }
}
