using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Celeste.Mod.Celeste_Multiworld;

public class Celeste_MultiworldModuleSettings : EverestModuleSettings
{
    public static Celeste_MultiworldModuleSettings Instance { get; private set; }

    public Celeste_MultiworldModuleSettings()
    {
        Instance = this;
    }

    #region Connection Info
    [SettingIgnore]
    public string Address { get; set; } = "archipelago.gg";
    [SettingIgnore]
    [SettingMinLength(1)]
    [SettingMaxLength(16)]
    public string SlotName { get; set; } = "Maddy";
    [SettingIgnore]
    public string Password { get; set; } = "";
    #endregion

    #region Send/Receive Messages
    public enum ItemReceiveStyle
    {
        None,
        Non_Strawberry_Progression,
        Progression,
        All
    }
    public ItemReceiveStyle ItemReceiveMessages { get; set; } = ItemReceiveStyle.All;

    public enum ItemSendStyle
    {
        None,
        Progression,
        All
    }

    public ItemSendStyle ItemSendMessages { get; set; } = ItemSendStyle.All;

    public bool ChatMessages { get; set; } = true;
    public bool ServerMessages { get; set; } = true;
    public bool RoomPopups { get; set; } = true;
    #endregion

    #region Debug Item Toggles
    [YamlIgnore]
    public ItemTogglesMenu ItemToggles { get; set; } = new();

    [SettingSubMenu]
    public class ItemTogglesMenu
    {
        public bool Dummy { get; set; }

        public Dictionary<string, TextMenu.OnOff> ItemToggleSettingItems = new();

        public void CreateDummyEntry(TextMenuExt.SubMenu menu, bool inGame)
        {
            foreach ((long id, string itemName) in Items.APItemData.ItemIDToString.Where(i => i.Key >= 0xCA12000 && i.Key < 0xCA13000)) // Interactables
            {
                TextMenu.OnOff settingEntry = new(
                    label: itemName,
                    on: IsItemActive(id)
                );

                settingEntry.Change(newValue => Celeste_MultiworldModule.SaveData.Interactables[id] = newValue);

                ItemToggleSettingItems[itemName] = settingEntry;
                menu.Add(settingEntry);
            }
        }

        private bool IsItemActive(long itemID)
        {
            if (Celeste_MultiworldModule.SaveData == null) return false;
            if (!Celeste_MultiworldModule.SaveData.Interactables.TryGetValue(itemID, out bool active)) return false;
            return active;
        }
    }
    #endregion
}