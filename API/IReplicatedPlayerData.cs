using SNetwork;

namespace Hikaria.PlayerSpawnApart.API;

public interface IReplicatedPlayerData
{
    SNetStructs.pPlayer PlayerData { get; set; }
}
