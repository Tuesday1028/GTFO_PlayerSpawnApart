using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hikaria.PlayerSpawnApart.Managers;
using MTFO.Managers;

namespace Hikaria.PlayerSpawnApart;

[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(MTFO.MTFO.GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
public class EntryPoint : BasePlugin
{
    public override void Load()
    {
        Instance = this;

        Directory.CreateDirectory(Path.Combine(ConfigManager.CustomPath, PluginInfo.NAME));

        PlayerSpawnApartManager.Setup();

        m_Harmony = new(PluginInfo.GUID);
        m_Harmony.PatchAll();

        Logs.LogMessage("OK");
    }

    public override bool Unload()
    {
        if (m_Harmony != null)
        {
            m_Harmony.UnpatchSelf();
        }

        return base.Unload();
    }

    private static Harmony m_Harmony;

    public static EntryPoint Instance { get; private set; }
}