using HarmonyLib;
using Hikaria.PlayerSpawnApart.API;
using Hikaria.PlayerSpawnApart.Managers;
using SNetwork;

namespace Hikaria.PlayerSpawnApart.Patches;

[HarmonyPatch(typeof(SNet_SessionHub))]
internal class Patch__SNet_SessionHub
{
    [HarmonyPatch(nameof(SNet_SessionHub.AddPlayerToSession))]
    [HarmonyPostfix]
    private static void SNet_SessionHub__AddPlayerToSession__Postfix(SNet_Player player)
    {
        if (player.IsLocal)
        {
            SNetworkAPI.SendCustomData<pPlayerSpawnApartSlot>();
            PlayerSpawnApartManager.ResetLocalSpawnApartSlot();
            Logs.LogMessage("ResetLocalSpawnApartSlot: AddLocalPlayerToSession");
        }
        else if (!player.IsBot)
        {
            SNetworkAPI.SendCustomData<pPlayerSpawnApartSlot>(player);
        }
    }
}
