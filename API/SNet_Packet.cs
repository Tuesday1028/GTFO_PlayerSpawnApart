using GTFO.API;

namespace Hikaria.PlayerSpawnApart.API;

public class SNet_Packet<T> where T : struct
{
    public void Setup(string eventName)
    {
        EventName = eventName;
        NetworkAPI.RegisterEvent<T>(eventName, OnReceiveData);
    }

    public string EventName { get; set; }
    private Action<T> ValidateAction { get; set; }
    private Action<ulong, T> ReceiveAction { get; set; }

    public static SNet_Packet<T> Create(string eventName, Action<ulong, T> receiveAction, Action<T> validateAction = null)
    {
        var packet = new SNet_Packet<T>
        {
            EventName = eventName,
            ReceiveAction = receiveAction,
            ValidateAction = validateAction,
            m_hasValidateAction = validateAction != null
        };
        NetworkAPI.RegisterEvent<T>(eventName, packet.OnReceiveData);
        return packet;
    }

    public void Ask(T data, SNetwork.SNet_ChannelType channelType = SNetwork.SNet_ChannelType.SessionOrderCritical)
    {
        if (SNetwork.SNet.IsMaster)
        {
            ValidateAction(data);
            return;
        }
        if (SNetwork.SNet.HasMaster)
        {
            Send(data, channelType, SNetwork.SNet.Master);
        }
    }

    public void Send(T data, SNetwork.SNet_ChannelType type, SNetwork.SNet_Player player = null)
    {
        NetworkAPI.InvokeEvent(EventName, data, player, type);
    }

    public void OnReceiveData(ulong sender, T data)
    {
        if (m_hasValidateAction && SNetwork.SNet.IsMaster)
        {
            ValidateAction(m_data);
            return;
        }
        ReceiveAction(sender, m_data);
    }

    private T m_data = new T();

    private bool m_hasValidateAction;
}