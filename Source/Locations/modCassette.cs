namespace Celeste.Mod.Celeste_Multiworld.Locations
{
    internal class modCassette : modLocationBase
    {
        public override void Load()
        {
            On.Celeste.Cassette.OnPlayer += modCassette_OnPlayer;
            On.Celeste.SaveData.RegisterCassette += SaveData_RegisterCassette;
        }

        public override void Unload()
        {
            On.Celeste.Cassette.OnPlayer -= modCassette_OnPlayer;
            On.Celeste.SaveData.RegisterCassette -= SaveData_RegisterCassette;
        }

        private static void modCassette_OnPlayer(On.Celeste.Cassette.orig_OnPlayer orig, Cassette self, Player player)
        {
            orig(self, player);

            string cassetteString = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_Cassette";

            Celeste_MultiworldModule.SaveData.CassetteLocations.Add(cassetteString);
        }

        private void SaveData_RegisterCassette(On.Celeste.SaveData.orig_RegisterCassette orig, SaveData self, AreaKey area)
        {
            orig(self, area);

            Session session = self.CurrentSession_Safe;
            session.Cassette = false;
            self.Areas_Safe[session.Area.ID].Cassette = false;
        }
    }
}
