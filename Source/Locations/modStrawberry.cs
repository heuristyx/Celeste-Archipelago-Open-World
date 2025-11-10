namespace Celeste.Mod.Celeste_Multiworld.Locations
{
    public class modStrawberry : modLocationBase
    {
        public override void Load()
        {
            On.Celeste.Strawberry.ctor += modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect += modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool += modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.SaveData.CheckStrawberry_EntityID += modSaveData_CheckStrawberry_EntityID;
            On.Celeste.TotalStrawberriesDisplay.Update += modTotalStrawberriesDisplay_Update;
        }

        public override void Unload()
        {
            On.Celeste.Strawberry.ctor -= modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect -= modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool -= modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.SaveData.CheckStrawberry_EntityID -= modSaveData_CheckStrawberry_EntityID;
            On.Celeste.TotalStrawberriesDisplay.Update -= modTotalStrawberriesDisplay_Update;
        }

        private static void modStrawberry_ctor(On.Celeste.Strawberry.orig_ctor orig, Strawberry self, EntityData data, Microsoft.Xna.Framework.Vector2 offset, EntityID gid)
        {
            orig(self, data, offset, gid);

            if (self.Golden)
            {
                if (SaveData.Instance.CurrentSession_Safe.Area.ID == 10)
                {
                    if (ArchipelagoManager.Instance.GoalLevel != "10c")
                    {
                        self.Active = false;
                        self.Visible = false;
                        self.Collidable = false;
                    }
                }
                else if (!ArchipelagoManager.Instance.IncludeGoldens)
                {
                    self.Active = false;
                    self.Visible = false;
                    self.Collidable = false;
                }
            }
        }

        private static void modStrawberry_OnCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self)
        {
            orig(self);
            string strawberryString = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{self.ID}";

            //Celeste_MultiworldModule.SaveData.StrawberryLocations.Add(strawberryString);
            Logger.Verbose("AP", strawberryString);

            // Undo any saving/data storage related to the berries
            // Safest way to do it... probably
            Session session = SaveData.Instance.CurrentSession_Safe;
            session.DoNotLoad.Remove(self.ID);
            session.Strawberries.Remove(self.ID);

            AreaModeStats stats = SaveData.Instance.Areas_Safe[session.Area.ID].Modes[(int)session.Area.Mode];
            stats.Strawberries.Remove(self.ID);
        }

        private static void modSaveData_AddStrawberry_AreaKey_EntityID_bool(On.Celeste.SaveData.orig_AddStrawberry_AreaKey_EntityID_bool orig, SaveData self, AreaKey area, EntityID strawberry, bool golden)
        {
            AreaModeStats areaModeStats = self.Areas_Safe[area.ID].Modes[(int)area.Mode];
            if (!areaModeStats.Strawberries.Contains(strawberry))
            {
                areaModeStats.Strawberries.Add(strawberry);
                areaModeStats.TotalStrawberries += 1;
            }
            Stats.Increment(golden ? Stat.GOLDBERRIES : Stat.BERRIES, 1);
        }

        private static bool modSaveData_CheckStrawberry_EntityID(On.Celeste.SaveData.orig_CheckStrawberry_EntityID orig, SaveData self, EntityID strawberry)
        {
            string AP_ID = $"{self.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{strawberry}";
            return Celeste_MultiworldModule.SaveData.StrawberryLocations.Contains(AP_ID);
        }

        private static void modTotalStrawberriesDisplay_Update(On.Celeste.TotalStrawberriesDisplay.orig_Update orig, TotalStrawberriesDisplay self)
        {
            self.strawberries.showOutOf = true;
            self.strawberries.OutOf = ArchipelagoManager.Instance.StrawberriesRequired;
            orig(self);
        }
    }
}
