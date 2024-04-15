namespace Hikaria.PlayerSpawnApart.Managers;

internal static class GameEventLogManager
{
    public static void AddLog(string log)
    {
        MainMenuGuiLayer.Current.PageLoadout.m_gameEventLog.AddLogItem(log);
        GuiManager.PlayerLayer.m_gameEventLog.AddLogItem(log);
    }
}