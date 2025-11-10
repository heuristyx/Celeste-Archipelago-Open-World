using Celeste.Mod.Celeste_Multiworld.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.Celeste_Multiworld.General
{

    public class RoomDisplay : Monocle.Entity
    {
        public static string CurrentRoom = "";
        public static int RoomDisplayTimer = 0;

        public RoomDisplay()
        {
            base.Y = 196f;
            base.Depth = -101;
            base.Tag = Tags.HUD | Tags.Global | Tags.PauseUpdate | Tags.TransitionUpdate;
        }

        public override void Render()
        {
            if (!Celeste_MultiworldModule.Settings.RoomPopups)
            {
                return;
            }

            if (SaveData.Instance == null || SaveData.Instance.CurrentSession_Safe == null)
            {
                return;
            }

            if (SaveData.Instance.CurrentSession_Safe.Level != CurrentRoom && SaveData.Instance.CurrentSession_Safe.Level != "")
            {
                CurrentRoom = SaveData.Instance.CurrentSession_Safe.Level;

                RoomDisplayTimer = 180;
            }

            if (RoomDisplayTimer >= 0)
            {
                float alpha = 1.0f;

                if (RoomDisplayTimer > 150)
                {
                    alpha = (float)(180.0f - RoomDisplayTimer) / 30.0f;
                }
                else if (RoomDisplayTimer < 30)
                {
                    alpha = (float)(RoomDisplayTimer) / 30.0f;
                }

                Color TextColor = new Color(0.96078f, 0.25882f, 0.78431f, alpha);

                ActiveFont.Draw($"Room: {CurrentRoom}", new Vector2(50f, 1030f), new Vector2(0.0f, 0.5f), Vector2.One, TextColor, 5.0f, new Color(0.0f, 0.0f, 0.0f, alpha), 0.0f, new Color(1.0f, 0.0f, 0.0f, 0.0f));
                RoomDisplayTimer--;
            }
        }
    }

    internal class modPlayer
    {
        public static int HairLength = 4;

        public void Load()
        {
            On.Celeste.Player.Die += modPlayer_Die;
            On.Celeste.Player.Update += modPlayer_Update;
            On.Celeste.PlayerSeeker.Update += modPlayerSeeker_Update;
            On.Celeste.Level.LoadLevel += modLevel_LoadLevel;
            On.Celeste.Level.CompleteArea_bool_bool_bool += modLevel_CompleteArea_bool_bool_bool;
        }

        public void Unload()
        {
            On.Celeste.Player.Die -= modPlayer_Die;
            On.Celeste.Player.Update -= modPlayer_Update;
            On.Celeste.PlayerSeeker.Update -= modPlayerSeeker_Update;
            On.Celeste.Level.LoadLevel -= modLevel_LoadLevel;
            On.Celeste.Level.CompleteArea_bool_bool_bool -= modLevel_CompleteArea_bool_bool_bool;
        }

        private static PlayerDeadBody modPlayer_Die(On.Celeste.Player.orig_Die orig, Player self, Microsoft.Xna.Framework.Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            PlayerDeadBody result = orig(self, direction, evenIfInvincible, registerDeathInStats);

            if (registerDeathInStats && !SaveData.Instance.Assists.Invincible)
            {
                ArchipelagoManager.Instance.SendDeathLinkIfEnabled("couldn't climb the mountain");
            }

            Items.Traps.TrapManager.Instance.AddDeathToActiveTraps();

            return result;
        }

        private static void modPlayer_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (Items.Traps.TrapManager.Instance.IsTrapActive(Items.Traps.TrapType.Stun))
            {
                self.Speed = Vector2.Zero;
                self.StateMachine.state = 0;
            }
            else
            {
                orig(self);
            }

            HandleMessageQueue(self);

            if (Items.Traps.TrapManager.Instance.IsTrapActive(Items.Traps.TrapType.Bald) && self.Sprite != null)
            {
                self.Sprite.HairCount = 0;
            }
            else if (self.Sprite != null)
            {
                self.Sprite.HairCount = self.StateMachine.state == 19 ? (int)(modPlayer.HairLength * 1.75f) : modPlayer.HairLength;
            }

            if (ArchipelagoManager.Instance.DeathLinkData != null)
            {
                if (self.InControl && !self.Dead && !(self.Scene as Level).InCredits)
                {
                    self.Die(Vector2.Zero, true, false);

                    ArchipelagoManager.Instance.ClearDeathLink();
                }
            }

            if (self.InControl && !self.Dead)
            {
                string AP_ID = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{SaveData.Instance.CurrentSession_Safe.Level}";
                Items.Traps.TrapManager.Instance.AddScreenToActiveTraps(AP_ID);

                ArchipelagoManager.Instance.SetRoomStorage(AP_ID);

                if (ArchipelagoManager.Instance.Roomsanity)
                {
                    Celeste_MultiworldModule.SaveData.RoomLocations.Add(AP_ID);
                }
            }
            else if (!self.InControl)
            {
                if (SaveData.Instance.CurrentSession_Safe.Area.ID == 8 && (self.Scene as Level).Completed)
                {
                    ArchipelagoManager.Instance.UpdateGameStatus(Archipelago.MultiClient.Net.Enums.ArchipelagoClientState.ClientGoal);
                }
            }

            if ($"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{SaveData.Instance.CurrentSession_Safe.Level}" == "10_0_f-door")
            {
                if (self.Position.X < 18980)
                {
                    self.Position.X = 18980;
                }
            }
        }

        private static void modPlayerSeeker_Update(On.Celeste.PlayerSeeker.orig_Update orig, PlayerSeeker self)
        {
            orig(self);

            if (ArchipelagoManager.Instance.Roomsanity)
            {
                if (self.enabled)
                {
                    string AP_ID = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{SaveData.Instance.CurrentSession_Safe.Level}";
                    Celeste_MultiworldModule.SaveData.RoomLocations.Add(AP_ID);
                }
            }
        }

        private static void modLevel_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // Fake the B Side Crystal Hearts so that the Golden Strawberries are spawned
            Dictionary<int, bool> realBSideHearts = new Dictionary<int, bool>();
            foreach (AreaStats area in SaveData.Instance.Areas_Safe)
            {
                AreaData areaData = AreaData.Areas[area.ID];

                // TODO: This causes B Side Crystal Hearts to look as Ghosts in-level
                if (areaData.HasMode(AreaMode.BSide) && areaData.Mode[(int)AreaMode.BSide].MapData.DetectedHeartGem)
                {
                    realBSideHearts.Add(area.ID, area.Modes[(int)AreaMode.BSide].HeartGem);
                    area.Modes[(int)AreaMode.BSide].HeartGem = true;
                }
            }

            orig(self, playerIntro, isFromLoader);

            // Undo faked B Side Crystal Hearts
            foreach (AreaStats area in SaveData.Instance.Areas_Safe)
            {
                AreaData areaData = AreaData.Areas[area.ID];

                if (areaData.HasMode(AreaMode.BSide) && areaData.Mode[(int)AreaMode.BSide].MapData.DetectedHeartGem)
                {
                    area.Modes[(int)AreaMode.BSide].HeartGem = realBSideHearts[area.ID];
                }
            }


            if (self.Session.Area.ID == 2 && self.Session.Area.Mode == 0)
            {
                // Always start Old Site A with Mirror Pre-Broken, for Logic reasons
                self.Session.Inventory.DreamDash = true;
            }

            // Pause UI Entities
            if (ArchipelagoManager.Instance.DeathLinkActive && self.Entities.FindFirst<DeathDisplay>() == null)
            {
                self.Entities.Add(new DeathDisplay());
            }
            if (self.Entities.FindFirst<RoomDisplay>() == null)
            {
                self.Entities.Add(new RoomDisplay());
            }

            self.SaveQuitDisabled = true;
        }

        private static ScreenWipe modLevel_CompleteArea_bool_bool_bool(On.Celeste.Level.orig_CompleteArea_bool_bool_bool orig, Level self, bool spotlightWipe, bool skipScreenWipe, bool skipCompleteScreen)
        {
            if (SaveData.Instance != null)
            {
                string AP_ID = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_Clear";
                Celeste_MultiworldModule.SaveData.LevelClearLocations.Add(AP_ID);
            }

            return orig(self, spotlightWipe, skipScreenWipe, skipCompleteScreen);
        }

        private static bool ShouldShowMessage(ArchipelagoMessage message)
        {
            if (message.Type == ArchipelagoMessage.MessageType.Literature)
            {
                return true;
            }
            else if (message.Type == ArchipelagoMessage.MessageType.Chat)
            {
                return Celeste_MultiworldModule.Settings.ChatMessages;
            }
            else if (message.Type == ArchipelagoMessage.MessageType.Server)
            {
                return Celeste_MultiworldModule.Settings.ServerMessages;
            }
            else if (message.Type == ArchipelagoMessage.MessageType.ItemReceive)
            {
                Celeste_MultiworldModuleSettings.ItemReceiveStyle style = Celeste_MultiworldModule.Settings.ItemReceiveMessages;

                if (!message.Strawberry && (message.Flags & Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement) != 0 && style > Celeste_MultiworldModuleSettings.ItemReceiveStyle.None)
                {
                    return true;
                }
                else if ((message.Flags & Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement) != 0 && style > Celeste_MultiworldModuleSettings.ItemReceiveStyle.Non_Strawberry_Progression)
                {
                    return true;
                }
                else if (style > Celeste_MultiworldModuleSettings.ItemReceiveStyle.Progression)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (message.Type == ArchipelagoMessage.MessageType.ItemSend)
            {
                Celeste_MultiworldModuleSettings.ItemSendStyle style = Celeste_MultiworldModule.Settings.ItemSendMessages;

                if ((message.Flags & Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement) != 0 && style > Celeste_MultiworldModuleSettings.ItemSendStyle.None)
                {
                    return true;
                }
                else if (style > Celeste_MultiworldModuleSettings.ItemSendStyle.Progression)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private static void HandleMessageQueue(Player self)
        {
            if (ArchipelagoManager.Instance.MessageLog.Count > 0)
            {
                if (self.Scene.Tracker.GetEntity<modMiniTextbox>() == null)
                {
                    ArchipelagoMessage message = ArchipelagoManager.Instance.MessageLog[0];
                    ArchipelagoManager.Instance.MessageLog.RemoveAt(0);

                    if (ShouldShowMessage(message))
                    {
                        self.Scene.Add(new modMiniTextbox(message.Text, (message.Type == ArchipelagoMessage.MessageType.Literature)));
                        Logger.Verbose("AP", message.Text);
                    }
                }
            }
        }
    }
}
