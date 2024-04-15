using HarmonyLib;
using Hikaria.PlayerSpawnApart.API;
using Hikaria.PlayerSpawnApart.Managers;
using Player;
using SNetwork;
using static Il2CppSystem.Globalization.CultureInfo;
using static PlayfabMatchmakingManager.MatchResult;

namespace Hikaria.PlayerSpawnApart.Patches;

[HarmonyPatch]
public class Patch__PlayerChatManager
{
    [HarmonyPatch(typeof(PlayerChatManager), nameof(PlayerChatManager.PostMessage))]
    private static void Prefix(PlayerChatManager __instance)
    {
        if (string.IsNullOrEmpty(__instance.m_currentValue)) return;

        var msg = __instance.m_currentValue.ToLowerInvariant();
        if (!msg.StartsWith("/psa")) return;
        try
        {
            if (!PlayerSpawnApartManager.AllowAssign)
            {
                GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> <color=red>Slot assignment is only available when in the lobby.</color>");
                return;
            }
            var args = msg.Split(' ');
            switch (args.Length)
            {
                case 2:
                    if (args[1] == "show")
                    {
                        GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> Slot assignments:");
                        for (int i = 0; i < SNet.Slots.SlottedPlayers.Count; i++)
                        {
                            var player = SNet.Slots.SlottedPlayers[i];
                            if (player.IsBot) continue;
                            GameEventLogManager.AddLog($"{player.NickName}</color>: Slot[{player.LoadCustomData<pPlayerSpawnApartSlot>().slot}]");
                        }
                        break;
                    }
                    if (args[1] == "reset")
                    {
                        PlayerSpawnApartManager.ResetLocalSpawnApartSlot();
                        break;
                    }
                    if (args[1] == "help")
                    {
                        GameEventLogManager.AddLog("<color=orange>[PlayerSpawnApart]</color> Helps:");
                        GameEventLogManager.AddLog("/psa assign [slot], assign slot for spawn apart. Range (1-4).");
                        GameEventLogManager.AddLog("/psa reset, reset assigned slot.");
                        break;
                    }
                    goto default;
                case 3:
                    if (args[1] == "assign")
                    {
                        if (!PlayerSpawnApartManager.TryAssignSpawnApartSlot(Convert.ToInt32(args[2]), out var reason))
                        {
                            GameEventLogManager.AddLog(reason);
                        };
                        break;
                    }
                    goto default;
                default:
                    throw new Exception();
            }
        }
        catch
        {
            GameEventLogManager.AddLog("<color=orange>[PlayerSpawnApart]</color> <color=red>Wrong input! Type in '/psa help' to show helps.</color>");
        }
        finally
        {
            __instance.m_currentValue = string.Empty;
        }
    }
}
