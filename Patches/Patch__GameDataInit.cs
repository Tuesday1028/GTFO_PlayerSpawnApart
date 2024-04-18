using GameData;
using HarmonyLib;
using Hikaria.PlayerSpawnApart.Managers;

namespace Hikaria.PlayerSpawnApart.Patches;

[HarmonyPatch(typeof(GameDataInit))]
public class Patch__GameDataInit
{
    [HarmonyPatch(nameof(GameDataInit.Initialize))]
    [HarmonyPostfix]
    private static void GameDataInit__Initialize__Postfix()
    {
        PlayerSpawnApartManager.OnGameDataInitialized();
    }
}
