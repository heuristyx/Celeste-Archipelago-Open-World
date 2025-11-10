using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.Celeste_Multiworld.Locations
{
    internal class modSummitGem : modLocationBase
    {
        static Dictionary<int, int> indexToGemItem = new Dictionary<int, int>()
        {
            // The Summit
            { 0, 0xCA16A00 },
            { 1, 0xCA16A01 },
            { 2, 0xCA16A02 },
            { 3, 0xCA16A03 },
            { 4, 0xCA16A04 },
            { 5, 0xCA16A05 },
        };

        public override void Load()
        {
            On.Celeste.SummitGem.OnPlayer += modSummitGem_OnPlayer;
            On.Celeste.SummitGemManager.Routine += modSummitGemManager_Routine;
            On.Celeste.SummitGem.SmashRoutine += SummitGem_SmashRoutine;
        }

        public override void Unload()
        {
            On.Celeste.SummitGem.OnPlayer -= modSummitGem_OnPlayer;
            On.Celeste.SummitGemManager.Routine -= modSummitGemManager_Routine;
            On.Celeste.SummitGem.SmashRoutine -= SummitGem_SmashRoutine;
        }

        private static void modSummitGem_OnPlayer(On.Celeste.SummitGem.orig_OnPlayer orig, SummitGem self, Player player)
        {
            if (player.DashAttacking)
            {
                string AP_ID = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{SaveData.Instance.CurrentSession_Safe.Level}";
                Celeste_MultiworldModule.SaveData.GemLocations.Add(AP_ID);
            }

            orig(self, player);
        }

        private static System.Collections.IEnumerator modSummitGemManager_Routine(On.Celeste.SummitGemManager.orig_Routine orig, SummitGemManager self)
        {
            Level level = self.Scene as Level;
            if (level.Session.HeartGem)
            {
                foreach (SummitGemManager.Gem gem2 in self.gems)
                {
                    gem2.Sprite.RemoveSelf();
                }
                self.gems.Clear();
                yield break;
            }
            for (; ; )
            {
                Player entity = self.Scene.Tracker.GetEntity<Player>();
                if (entity != null && (entity.Position - self.Position).Length() < 64f)
                {
                    break;
                }
                yield return null;
            }
            yield return 0.5f;
            bool alreadyHasHeart = level.Session.OldStats.Modes[0].HeartGem;
            int broken = 0;
            int index = 0;
            foreach (SummitGemManager.Gem gem in self.gems)
            {
                int gemSaveID = indexToGemItem[index];
                bool flag = Celeste_MultiworldModule.SaveData.GemItems.ContainsKey(gemSaveID) && Celeste_MultiworldModule.SaveData.GemItems[gemSaveID];
                int num;
                if (flag)
                {
                    if (index == 0)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_1", gem.Position);
                    }
                    else if (index == 1)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_2", gem.Position);
                    }
                    else if (index == 2)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_3", gem.Position);
                    }
                    else if (index == 3)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_4", gem.Position);
                    }
                    else if (index == 4)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_5", gem.Position);
                    }
                    else if (index == 5)
                    {
                        Audio.Play("event:/game/07_summit/gem_unlock_6", gem.Position);
                    }
                    gem.Sprite.Play("spin", false, false);
                    while (gem.Sprite.CurrentAnimationID == "spin")
                    {
                        gem.Bloom.Alpha = Calc.Approach(gem.Bloom.Alpha, 1f, Engine.DeltaTime * 3f);
                        if (gem.Bloom.Alpha > 0.5f)
                        {
                            gem.Shake = Calc.Random.ShakeVector();
                        }
                        gem.Sprite.Y -= Engine.DeltaTime * 8f;
                        gem.Sprite.Scale = Vector2.One * (1f + gem.Bloom.Alpha * 0.1f);
                        yield return null;
                    }
                    yield return 0.2f;
                    level.Shake(0.3f);
                    Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                    for (int i = 0; i < 20; i++)
                    {
                        level.ParticlesFG.Emit(SummitGem.P_Shatter, gem.Position + new Vector2((float)Calc.Random.Range(-8, 8), (float)Calc.Random.Range(-8, 8)), SummitGem.GemColors[index], Calc.Random.NextFloat(6.2831855f));
                    }
                    num = broken;
                    broken = num + 1;
                    gem.Bloom.RemoveSelf();
                    gem.Sprite.RemoveSelf();
                    yield return 0.25f;
                }
                num = index;
                index = num + 1;
                //gem = null;
            }

            if (broken >= 6)
            {
                HeartGem heart = self.Scene.Entities.FindFirst<HeartGem>();
                if (heart != null)
                {
                    Audio.Play("event:/game/07_summit/gem_unlock_complete", heart.Position);
                    yield return 0.1f;
                    Vector2 from = heart.Position;
                    float p = 0f;
                    while (p < 1f && heart.Scene != null)
                    {
                        heart.Position = Vector2.Lerp(from, self.Position + new Vector2(0f, -16f), Ease.CubeOut(p));
                        yield return null;
                        p += Engine.DeltaTime;
                    }
                    from = default(Vector2);
                }
                heart = null;
            }
            yield break;
        }

        private IEnumerator SummitGem_SmashRoutine(On.Celeste.SummitGem.orig_SmashRoutine orig, SummitGem self, Player player, Level level)
        {
            IEnumerator origIEnumerator = orig(self, player, level);

            while (origIEnumerator.MoveNext())
            {
                yield return origIEnumerator.Current;
            }

            Session session = SaveData.Instance.CurrentSession_Safe;
            session.SummitGems[self.GemID] = false;
            session.DoNotLoad.Remove(self.GID);
            SaveData.Instance.SummitGems[self.GemID] = false;
        }
    }
}
