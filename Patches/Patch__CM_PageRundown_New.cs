using CellMenu;
using GameData;
using HarmonyLib;
using Hikaria.PlayerSpawnApart.Managers;

namespace Hikaria.PlayerSpawnApart.Patches;

[HarmonyPatch(typeof(CM_MenuBar))]
internal class Patch__CM_MenuBar
{
    [HarmonyPatch(nameof(CM_MenuBar.OnExpeditionUpdated))]
    [HarmonyPostfix]
    private static void CM_MenuBar__OnExpeditionUpdated__Postfix(pActiveExpedition expData, ExpeditionInTierData expInTierData)
    {
        PlayerSpawnApartManager.OnExpeditionUpdated(expData, expInTierData);
    }
}
