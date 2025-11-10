using Microsoft.Xna.Framework;

namespace Celeste.Mod.Celeste_Multiworld.Locations
{
    internal class modHeartGem : modLocationBase
    {
        public override void Load()
        {
            On.Celeste.HeartGem.Collect += modHeartGem_Collect;
            On.Celeste.HeartGem.SkipFakeHeartCutscene += modHeartGem_SkipFakeHeartCutscene;
            On.Celeste.Level.RegisterAreaComplete += modLevel_RegisterAreaComplete;
            On.Celeste.SaveData.RegisterHeartGem += SaveData_RegisterHeartGem;
        }



        public override void Unload()
        {
            On.Celeste.HeartGem.Collect -= modHeartGem_Collect;
            On.Celeste.HeartGem.SkipFakeHeartCutscene -= modHeartGem_SkipFakeHeartCutscene;
            On.Celeste.Level.RegisterAreaComplete -= modLevel_RegisterAreaComplete;
            On.Celeste.SaveData.RegisterHeartGem -= SaveData_RegisterHeartGem;
        }

        private static void modHeartGem_Collect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player)
        {
            orig(self, player);

            if (SaveData.Instance.CurrentSession_Safe.Area.Mode == AreaMode.Normal)
            {
                string crystalHeartString = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_CrystalHeart";

                Celeste_MultiworldModule.SaveData.CrystalHeartLocations.Add(crystalHeartString);
            }
        }

        private static void modHeartGem_SkipFakeHeartCutscene(On.Celeste.HeartGem.orig_SkipFakeHeartCutscene orig, HeartGem self, Level level)
        {
            if (ArchipelagoManager.Instance.ActiveLevels.Contains("10b"))
            {
                orig(self, level);
            }
            else
            {
                Monocle.Engine.TimeRate = 1f;
                Glitch.Value = 0f;
                if (self.sfx != null)
                {
                    self.sfx.Source.Stop(true);
                }
                level.Session.SetFlag("fake_heart", true);
                level.Frozen = false;
                level.FormationBackdrop.Display = false;
                level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/intermission_heartgroove";
                level.Session.Audio.Apply(false);
                Player entity = self.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    entity.Sprite.Play("idle", false, false);
                    entity.Active = true;
                    entity.StateMachine.State = 0;
                    entity.Dashes = 1;
                    entity.Speed = Vector2.Zero;
                    entity.MoveV(200f, null, null);
                    entity.Depth = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        entity.UpdateHair(true);
                    }
                }
                foreach (AbsorbOrb absorbOrb in self.Scene.Entities.FindAll<AbsorbOrb>())
                {
                    absorbOrb.RemoveSelf();
                }
                if (self.poem != null)
                {
                    self.poem.RemoveSelf();
                }
                if (self.bird != null)
                {
                    self.bird.RemoveSelf();
                }

                self.RemoveSelf();
            }
        }

        private static void modLevel_RegisterAreaComplete(On.Celeste.Level.orig_RegisterAreaComplete orig, Level self)
        {
            orig(self);

            if (SaveData.Instance.CurrentSession_Safe.Area.Mode != AreaMode.Normal)
            {
                string AP_ID = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_Clear";
                Celeste_MultiworldModule.SaveData.LevelClearLocations.Add(AP_ID);
            }
        }

        private void SaveData_RegisterHeartGem(On.Celeste.SaveData.orig_RegisterHeartGem orig, SaveData self, AreaKey area)
        {
            orig(self, area);

            Session session = self.CurrentSession_Safe;
            session.HeartGem = false;
            self.Areas_Safe[session.Area.ID].Modes[(int)session.Area.Mode].HeartGem = false;
        }
    }
}
