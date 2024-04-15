using SNetwork;

namespace Hikaria.PlayerSpawnApart.SNetworkExt;

public interface IReplicatedPlayerData
{
    SNetStructs.pPlayer PlayerData { get; set; }
}
