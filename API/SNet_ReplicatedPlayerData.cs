﻿namespace Hikaria.PlayerSpawnApart.SNetworkExt;

public class SNet_ReplicatedPlayerData<A> where A : struct
{
    public static void Setup(string eventName, Action<SNetwork.SNet_Player, A> callback)
    {
        if (s_singleton == null)
        {
            s_singleton = new();
            s_singleton.m_syncPacket = SNet_Packet<A>.Create(eventName, OnRecieveData);
        }
        s_singleton.m_onChangeCallback = callback;
    }

    private static void OnRecieveData(ulong sender, A wrappedData)
    {
        SNetwork.SNet_Player snet_Player;
        if (((IReplicatedPlayerData)(object)wrappedData).PlayerData.TryGetPlayer(out snet_Player) && !snet_Player.IsLocal)
        {
            snet_Player.StoreCustomData(wrappedData);
            Action<SNetwork.SNet_Player, A> onChangeCallback = s_singleton.m_onChangeCallback;
            if (onChangeCallback == null)
            {
                return;
            }
            onChangeCallback(snet_Player, wrappedData);
        }
    }

    public static void SendData(SNetwork.SNet_Player player, A data, SNetwork.SNet_Player toPlayer = null)
    {
        if (toPlayer != null && toPlayer.IsBot)
        {
            return;
        }
        if (player.IsLocal || SNetwork.SNet.IsMaster)
        {
            SNetwork.SNetStructs.pPlayer pPlayer = new();
            pPlayer.SetPlayer(player);
            IReplicatedPlayerData replicatedPlayerData = (IReplicatedPlayerData)((object)data);
            replicatedPlayerData.PlayerData = pPlayer;
            data = (A)(object)replicatedPlayerData;
            if (toPlayer != null)
            {
                s_singleton.m_syncPacket.Send(data, SNetwork.SNet_ChannelType.SessionOrderCritical, toPlayer);
                return;
            }
            s_singleton.m_syncPacket.Send(data, SNetwork.SNet_ChannelType.SessionOrderCritical);
        }
    }

    public static bool Compare(delegateComparisonAction comparisonAction, A comparisonValue, SNetwork.eComparisonGroup group, bool includeBots = false)
    {
        Il2CppSystem.Collections.Generic.List<SNetwork.SNet_Player> list;
        if (group != SNetwork.eComparisonGroup.PlayersInSession)
        {
            if (group != SNetwork.eComparisonGroup.PlayersSynchedWithGame)
            {
                return false;
            }
            list = SNetwork.SNet.Slots.PlayersSynchedWithGame;
        }
        else
        {
            list = SNetwork.SNet.Slots.SlottedPlayers;
        }
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            SNetwork.SNet_Player snet_Player = list[i];
            if (includeBots || !snet_Player.IsBot)
            {
                A a = snet_Player.LoadCustomData<A>();
                if (!comparisonAction(a, snet_Player, comparisonValue))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static SNet_ReplicatedPlayerData<A> s_singleton;

    private SNet_Packet<A> m_syncPacket;

    private Action<SNetwork.SNet_Player, A> m_onChangeCallback;

    public delegate bool delegateComparisonAction(A playerData, SNetwork.SNet_Player player, A comparisonData);
}