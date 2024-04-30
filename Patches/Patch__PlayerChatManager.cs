using HarmonyLib;
using Hikaria.PlayerSpawnApart.Managers;

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
            var args = msg.Split(' ');
            switch (args.Length)
            {
                case 2:
                    if (args[1] == "check")
                    {
                        GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> Slot assignments:");
                        PlayerSpawnApartManager.ShowAllAssignedSlots();
                        break;
                    }
                    if (args[1] == "reset")
                    {
                        if (!PlayerSpawnApartManager.AllowAssign)
                        {
                            GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> <color=red>Slot assignment is only available when in the lobby.</color>");
                            break;
                        }
                        PlayerSpawnApartManager.ResetLocalSpawnApartSlot();
                        Logs.LogMessage("ResetLocalSpawnApartSlot: Manual reset");
                        break;
                    }
                    if (args[1] == "help")
                    {
                        GameEventLogManager.AddLog("<color=orange>[PlayerSpawnApart]</color> Commands available:");
                        GameEventLogManager.AddLog("/psa assign [slot], assign slot for spawn apart. Range (1-4).");
                        GameEventLogManager.AddLog("/psa reset, reset assigned slot.");
                        GameEventLogManager.AddLog("/psa check, check assigned slots.");
                        break;
                    }
                    goto default;
                case 3:
                    if (args[1] == "assign")
                    {
                        if (!PlayerSpawnApartManager.AllowAssign)
                        {
                            GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> <color=red>Slot assignment is only available when in the lobby.</color>");
                            break;
                        }
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
        catch (Exception ex)
        {
            Logs.LogError(ex);
            GameEventLogManager.AddLog("<color=orange>[PlayerSpawnApart]</color> <color=red>Wrong input! Type in '/psa help' to show helps.</color>");
        }
        finally
        {
            __instance.m_currentValue = string.Empty;
        }
    }
}
