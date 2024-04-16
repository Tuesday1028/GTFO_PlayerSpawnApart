using SNetwork;

namespace Hikaria.PlayerSpawnApart.API;

public interface IReplicatedPlayerData
{
    public SNetStructs.pPlayer PlayerData { get; set; }
}
