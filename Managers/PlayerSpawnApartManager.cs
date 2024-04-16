using GameData;
using GTFO.API;
using Hikaria.PlayerSpawnApart.API;
using LevelGeneration;
using MTFO.Managers;
using Player;
using SNetwork;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace Hikaria.PlayerSpawnApart.Managers;

public static class PlayerSpawnApartManager
{
    public static void Setup()
    {
        SNetworkAPI.SetupCustomData<pPlayerSpawnApartSlot>(PlayerSpawnApartEventName, OnPlayerSpawnApartSlotChanged);
        EventAPI.OnManagersSetup += delegate
        {
            LevelAPI.OnEnterLevel += ApplySpawnApartData;
        };
    }
    private static readonly string PlayerSpawnApartEventName = typeof(pPlayerSpawnApartSlot).FullName;
    private static List<PlayerSpawnApartData> PlayerSpawnApartDataLookup = new();
    private static uint LevelLayoutID;
    private static PlayerSpawnApartData ActivatedPlayerSpawnApartData;
    private static bool HasActivatedPlayerSpawnApartData => ActivatedPlayerSpawnApartData != null;

    public static bool AllowAssign => GameStateManager.CurrentStateName == eGameStateName.Lobby;

    public static bool IsReady => !HasActivatedPlayerSpawnApartData || SNet.LocalPlayer == null || IsPlayerReady(SNet.LocalPlayer);

    public static bool IsEveryoneReady
    {
        get
        {
            if (!HasActivatedPlayerSpawnApartData) return true;
            if (SNet.Slots.SlottedPlayers.Count == 0) return false;

            for (int i = 0; i < SNet.Slots.SlottedPlayers.Count; i++)
            {
                SNet_Player player = SNet.Slots.SlottedPlayers[i];
                if (player.IsBot) continue;
                var slot = player.LoadCustomData<pPlayerSpawnApartSlot>().slot;
                    
                if (!IsValidSlotRange(slot) || slot == -1) return false;
            }
            return true;
        }
    }

    private static bool IsPlayerReady(SNet_Player player)
    {
        if (!HasActivatedPlayerSpawnApartData) return true;
        if (player == null || player.IsBot) return true;

        var slot = player.LoadCustomData<pPlayerSpawnApartSlot>().slot;
        return slot != -1 && IsValidSlotRange(slot);
    }

    public static void OnGameDataInitialized()
    {
        JsonSerializerOptions opt = new JsonSerializerOptions();
        opt.IncludeFields = false;
        opt.ReadCommentHandling = JsonCommentHandling.Skip;
        opt.PropertyNameCaseInsensitive = true;
        opt.WriteIndented = true;
        opt.Converters.Add(new JsonStringEnumConverter());
        PlayerSpawnApartDataLookup = GTFO.API.JSON.JsonSerializer.Deserialize<List<PlayerSpawnApartData>>(File.ReadAllText(Path.Combine(ConfigManager.CustomPath, PluginInfo.NAME, "SpawnApartData.json")));
    }

    public static void ApplySpawnApartData()
    {
        if (!HasActivatedPlayerSpawnApartData) return;

        var playerAgent = PlayerManager.GetLocalPlayerAgent();
        var slot = playerAgent.Owner.LoadCustomData<pPlayerSpawnApartSlot>().slot;
        Vector3 pos;
        switch (slot)
        {
            case 1:
                pos = ActivatedPlayerSpawnApartData.Slot1;
                break;
            case 2:
                pos = ActivatedPlayerSpawnApartData.Slot2;
                break;
            case 3:
                pos = ActivatedPlayerSpawnApartData.Slot3;
                break;
            case 4:
                pos = ActivatedPlayerSpawnApartData.Slot4;
                break;
            default:
                GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> <color=red>Illegal Slot[{slot}]!</color>");
                return;
        }

        var dimensionIndex = Dimension.GetDimensionFromPos(pos).DimensionIndex;
        if (dimensionIndex != eDimensionIndex.Reality)
        {
            playerAgent.RequestWarpToSync(dimensionIndex, pos, playerAgent.TargetLookDir, PlayerAgent.WarpOptions.None);
        }
        else
        {
            playerAgent.TeleportTo(pos);
        }
    }

    private static bool ChooseSlotValidate(SNet_Player player, int slot)
    {
        if (!IsValidSlotRange(slot)) return false;

        for (int i = 0; i < SNet.Slots.SlottedPlayers.Count; i++)
        {
            var slottedPlayer = SNet.Slots.SlottedPlayers[i];
            if (slottedPlayer == player) continue;

            if (slot != -1 && slottedPlayer.LoadCustomData<pPlayerSpawnApartSlot>().slot == slot)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsValidSlotRange(int slot)
    {
        return slot == -1 || (slot >= 1 && slot <= 4);
    }

    public static void ResetLocalSpawnApartSlot()
    {
        pPlayerSpawnApartSlot data = new(SNet.LocalPlayer, -1);
        SNetworkAPI.SetLocalCustomData(data);
        OnPlayerSpawnApartSlotChanged(SNet.LocalPlayer, data);
    }

    private static bool SpawnApartSlotHasConflict(int slot)
    {
        if (slot == -1) return false;
        for (int i = 0; i < SNet.Slots.SlottedPlayers.Count; i++)
        {
            var player = SNet.Slots.SlottedPlayers[i];
            if (player.IsBot) continue;
            if (!player.IsLocal && player.LoadCustomData<pPlayerSpawnApartSlot>().slot == slot) return true;
        }
        return false;
    }

    public static bool TryAssignSpawnApartSlot(int slot, out string msg)
    {
        msg = string.Empty;
        if (!IsValidSlotRange(slot))
        {
            msg = $"<color=orange>[PlayerSpawnApart]</color> Illegal slot range. Range (1-4).";
            return false;
        }
        if (!ChooseSlotValidate(SNet.LocalPlayer, slot))
        {
            msg = $"<color=orange>[PlayerSpawnApart]</color> Slot[{slot}] has already been assigned, please assign another one.";
            return false;
        }
        pPlayerSpawnApartSlot data = new(SNet.LocalPlayer, slot);
        SNetworkAPI.SetLocalCustomData(data);
        OnPlayerSpawnApartSlotChanged(SNet.LocalPlayer, data);
        return true;
    }

    public static void OnExpeditionUpdated(pActiveExpedition expData, ExpeditionInTierData expInTierData)
    {
        LevelLayoutID = expInTierData.LevelLayoutData;
        for (int i = 0; i < PlayerSpawnApartDataLookup.Count; i++)
        {
            var data = PlayerSpawnApartDataLookup[i];
            if (data.MainLevelLayoutID == LevelLayoutID && data.InternalEnabled)
            {
                ActivatedPlayerSpawnApartData = data;
                return;
            }
        }
        ActivatedPlayerSpawnApartData = null;
    }

    private static void OnPlayerSpawnApartSlotChanged(SNet_Player player, pPlayerSpawnApartSlot data)
    {
        if (!HasActivatedPlayerSpawnApartData) return;

        if (!player.IsLocal && SpawnApartSlotHasConflict(data.slot))
        {
            ResetLocalSpawnApartSlot();
            return;
        }
        if (data.slot == -1)
        {
            GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> {player.NickName}</color> reset slot.");
        }
        else
        {
            GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> {player.NickName}</color> assign slot[{data.slot}].");
        }
        if (IsEveryoneReady)
        {
            GameEventLogManager.AddLog($"<color=orange>[PlayerSpawnApart]</color> All players are ready:");
            for (int i = 0; i < SNet.Slots.SlottedPlayers.Count; i++)
            {
                var slottedPlayer = SNet.Slots.SlottedPlayers[i];
                if (slottedPlayer.IsBot) continue;
                GameEventLogManager.AddLog($"{player.NickName}</color>: Slot[{slottedPlayer.LoadCustomData<pPlayerSpawnApartSlot>().slot}]");
            }
        }
    }
}

public class PlayerSpawnApartData
{
    public Vector3 Slot1 { get; set; }
    public Vector3 Slot2 { get; set; }
    public Vector3 Slot3 { get; set; }
    public Vector3 Slot4 { get; set; }
    public uint MainLevelLayoutID { get; set; }

    public bool InternalEnabled { get; set; }
    public string DebugName { get; set; }
}

public struct pPlayerSpawnApartSlot : API.IReplicatedPlayerData
{
    public pPlayerSpawnApartSlot()
    {
    }

    public pPlayerSpawnApartSlot(SNet_Player player, int slot)
    {
        this.slot = slot;
        PlayerData.SetPlayer(player);
    }

    public SNetStructs.pPlayer PlayerData { get; set; } = new();

    public int slot { get; set; } = -1;
}
