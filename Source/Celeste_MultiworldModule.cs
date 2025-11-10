using Celeste.Mod.Celeste_Multiworld.Aesthetics;
using Celeste.Mod.Celeste_Multiworld.General;
using Celeste.Mod.Celeste_Multiworld.Items;
using Celeste.Mod.Celeste_Multiworld.Items.Traps;
using Celeste.Mod.Celeste_Multiworld.Locations;
using Celeste.Mod.Celeste_Multiworld.UI;
using System;

namespace Celeste.Mod.Celeste_Multiworld;

public class Celeste_MultiworldModule : EverestModule
{
    public static Celeste_MultiworldModule Instance { get; private set; }

    public override Type SettingsType => typeof(Celeste_MultiworldModuleSettings);
    public static Celeste_MultiworldModuleSettings Settings => (Celeste_MultiworldModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(Celeste_MultiworldModuleSession);
    public static Celeste_MultiworldModuleSession Session => (Celeste_MultiworldModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(Celeste_MultiworldModuleSaveData);
    public static Celeste_MultiworldModuleSaveData SaveData => (Celeste_MultiworldModuleSaveData)Instance._SaveData;

    #region Hooks
    modMainMenu menu = new modMainMenu();
    modChapterMenu chapterPanel = new modChapterMenu();
    modJournal journal = new modJournal();
    modAudio audio = new modAudio();
    modPlayer player = new modPlayer();
    modHeartGate heartGate = new modHeartGate();
    #endregion

    public Celeste_MultiworldModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel("AP", LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel("AP", LogLevel.Info);
#endif
    }

    public override void Load()
    {
        new ArchipelagoManager(Celeste.Instance);
        new TrapManager(Celeste.Instance);

        // TODO: apply any hooks that should always be active
        foreach (modItemBase item in APItemData.modItems)
        {
            item.Load();
        }

        foreach (modLocationBase location in APLocationData.modLocations)
        {
            location.Load();
        }

        menu.Load();
        chapterPanel.Load();
        journal.Load();
        audio.Load();
        player.Load();
        heartGate.Load();

        modMiniTextbox.Load();
    }

    public override void Unload()
    {
        // TODO: unapply any hooks applied in Load()
        foreach (modItemBase item in APItemData.modItems)
        {
            item.Unload();
        }

        foreach (modLocationBase location in APLocationData.modLocations)
        {
            location.Unload();
        }

        menu.Unload();
        chapterPanel.Unload();
        journal.Unload();
        audio.Unload();
        player.Unload();
        heartGate.Unload();

        modMiniTextbox.Unload();
    }
}
