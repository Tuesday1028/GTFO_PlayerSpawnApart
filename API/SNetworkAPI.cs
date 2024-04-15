namespace Hikaria.PlayerSpawnApart.SNetworkExt;

public static class SNetworkAPI
{
    private static Dictionary<ulong, Dictionary<Type, DataWrapper>> DataWrappersLookup = new();

    public static void SetupCustomData<A>(string eventName, Action<SNetwork.SNet_Player, A> callback) where A : struct, IReplicatedPlayerData
    {
        SNet_ReplicatedPlayerData<A>.Setup(eventName, callback);
    }

    public static void SetLocalCustomData<A>(A data) where A : struct
    {
        if (SNetwork.SNet.LocalPlayer != null)
        {
            StoreCustomData(SNetwork.SNet.LocalPlayer, data);
        }
    }

    public static void SendCustomData<A>(this SNetwork.SNet_Player player, SNetwork.SNet_Player toPlayer = null) where A : struct
    {
        if (toPlayer != null && toPlayer.IsBot)
        {
            return;
        }
        if (SNetwork.SNet.LocalPlayer != null)
        {
            SNet_ReplicatedPlayerData<A>.SendData(SNetwork.SNet.LocalPlayer, SNetwork.SNet.GetLocalCustomData<A>(), toPlayer);
        }
        if (SNetwork.SNet.IsMaster && toPlayer != null && !toPlayer.IsBot)
        {
            var allBots = SNetwork.SNet.Core.GetAllBots(true);
            for (int i = 0; i < allBots.Count; i++)
            {
                SNetwork.SNet_Player snet_Player = allBots[i];
                if (snet_Player != null && snet_Player.IsBot)
                {
                    SNet_ReplicatedPlayerData<A>.SendData(snet_Player, LoadCustomData<A>(snet_Player), toPlayer);
                }
            }
        }
    }

    public static A GetLocalCustomData<A>() where A : struct
    {
        if (SNetwork.SNet.LocalPlayer != null)
        {
            return LoadCustomData<A>(SNetwork.SNet.LocalPlayer);
        }
        return default(A);
    }

    public static A LoadCustomData<A>(this SNetwork.SNet_Player player) where A : struct
    {
        Type typeFromHandle = typeof(A);
        DataWrapper dataWrapper;
        DataWrapper<A> dataWrapper2;
        if (!DataWrappersLookup.TryGetValue(player.Lookup, out var dic))
        {
            DataWrappersLookup[player.Lookup] = new();
            dic = DataWrappersLookup[player.Lookup];
        }
        if (!dic.TryGetValue(typeFromHandle, out dataWrapper))
        {
            dataWrapper2 = new DataWrapper<A>();
            dic.Add(typeFromHandle, dataWrapper2);
        }
        else
        {
            dataWrapper2 = (DataWrapper<A>)dataWrapper;
        }
        return dataWrapper2.Load();
    }

    public static void StoreCustomData<A>(this SNetwork.SNet_Player player, A data) where A : struct
    {
        Type typeFromHandle = typeof(A);
        DataWrapper dataWrapper;
        DataWrapper<A> dataWrapper2;
        if (!DataWrappersLookup.TryGetValue(player.Lookup, out var dic))
        {
            DataWrappersLookup[player.Lookup] = new();
            dic = DataWrappersLookup[player.Lookup];
        }
        if (!dic.TryGetValue(typeFromHandle, out dataWrapper))
        {
            dataWrapper2 = new DataWrapper<A>();
            dic.Add(typeFromHandle, dataWrapper2);
        }
        else
        {
            dataWrapper2 = (DataWrapper<A>)dataWrapper;
        }
        dataWrapper2.Store(player, data);
    }

    public abstract class DataWrapper
    {
        public abstract void Send(SNetwork.SNet_Player fromPlayer, SNetwork.SNet_Player toPlayer = null);
    }

    public class DataWrapper<A> : DataWrapper where A : struct
    {
        public A Load()
        {
            return m_data;
        }

        public void Store(SNetwork.SNet_Player player, A data)
        {
            m_data = data;
            SNet_ReplicatedPlayerData<A>.SendData(player, m_data);
        }

        public override void Send(SNetwork.SNet_Player fromPlayer, SNetwork.SNet_Player toPlayer = null)
        {
            SNet_ReplicatedPlayerData<A>.SendData(fromPlayer, m_data, toPlayer);
        }

        private A m_data;
    }
}
