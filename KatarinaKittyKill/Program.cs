﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;
using System.Net;

namespace KatarinaKittyKill
{
    class Program
    {
        public const string ChampionName = "Katarina";

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spellbook spellBook = ObjectManager.Player.Spellbook;
        public static SpellDataInst rSpell = spellBook.GetSpell(SpellSlot.R);

        public static SpellSlot IgniteSlot;

        public static Obj_AI_Hero selectedTarget = null;
        //Menu
        public static Menu menu;

        //items
        public static Items.Item DFG;

        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //check to see if correct champ
            if (Player.BaseSkinName != ChampionName) return;

            //intalize spell
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            Q.SetTargetted(400, 1400);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline || Utility.Map.GetMap()._MapType == Utility.Map.MapType.CrystalScar ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            menu = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            var orbwalkerMenu = new Menu("My Orbwalker", "my_Orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);
            
            //Target selector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //Key 
            menu.AddSubMenu(new Menu("Key", "Key"));
            menu.SubMenu("Key").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Key").AddItem(new MenuItem("lastHit", "Lasthit!").SetValue(new KeyBind(menu.Item("LastHit_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key").AddItem(new MenuItem("LaneClearActive", "Lane Clear").SetValue(new KeyBind(menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Key").AddItem(new MenuItem("jFarm", "Jungle Farm").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Key").AddItem(new MenuItem("Wardjump", "Escape/Ward jump").SetValue(new KeyBind(menu.Item("Flee_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Combo menu:
            menu.AddSubMenu(new Menu("Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("tsModes", "TS Modes").SetValue(new StringList(new[] { "Orbwalker/LessCast", "Low HP%", "NearMouse", "CurrentHP" }, 0)));
            menu.SubMenu("Combo").AddItem(new MenuItem("selected", "Focus Selected Target").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("eDis", "E only if >").SetValue(new Slider(0, 0, 700)));
            menu.SubMenu("Combo").AddItem(new MenuItem("smartE", "Smart E with R CD ").SetValue(false));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("comboMode", "Mode").SetValue(new StringList(new[] { "QEW", "EQW" }, 0)));

            //Harass menu:
            menu.AddSubMenu(new Menu("Harass", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("harassMode", "Mode").SetValue(new StringList(new[] { "QEW", "EQW", "QW" }, 2)));
            menu.SubMenu("Harass").AddItem(new MenuItem("harassUnderTower", "Harass under tower").SetValue(false));

            //Farming menu:
            menu.AddSubMenu(new Menu("LastHit", "LastHit"));
            menu.SubMenu("LastHit").AddItem(new MenuItem("UseQHit", "Use Q Last Hit").SetValue(false));
            menu.SubMenu("LastHit").AddItem(new MenuItem("UseWHit", "Use W Last Hit").SetValue(false));
            menu.SubMenu("LastHit").AddItem(new MenuItem("UseEHit", "Use E Last Hit").SetValue(false));

            menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(false));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(false));
            menu.SubMenu("LaneClear").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(false));


            //killsteal
            menu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("smartKS", "Use Smart KS System").SetValue(true));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("wardKs", "Use Jump KS").SetValue(true));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("rKS", "Use R for KS").SetValue(true));
            menu.SubMenu("KillSteal").AddItem(new MenuItem("rCancel", "NO R Cancel for KS").SetValue(false));

            //Misc Menu:
            menu.AddSubMenu(new Menu("Misc", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("dfg", "Use DFG").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("ignite", "Use Ignite").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("igniteMode", "Mode").SetValue(new StringList(new[] { "Combo", "KS" }, 0)));
            menu.SubMenu("Misc").AddItem(new MenuItem("autoWz", "Auto W Enemy").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("printTar", "Print Selected Target").SetValue(true));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            //Drawings menu:
            menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();

            using (var wb = new WebClient())
            {
                wb.DownloadString("http://thunderr.net/leaguesharp/update.php?script_name=KatarinaKittyKiller");
                var response = wb.DownloadString("http://thunderr.net/leaguesharp/uses.php?script_name=KatarinaKittyKiller");
                Game.PrintChat("This script has been used: " + response + " times");
            }

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            Game.OnGameSendPacket += Game_OnGameSendPacket;
            Game.PrintChat(ChampionName + " Loaded! --- by xSalice");
        }

        public static bool inTowerRange(Vector2 pos)
        {
            //  if (!YasuoSharp.Config.Item("djTur").GetValue<bool>())
            //      return false;
            foreach (Obj_AI_Turret tur in ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsEnemy && tur.Health > 0))
            {
                if (pos.Distance(tur.Position.To2D()) < (850 + Player.BoundingRadius))
                    return true;
            }
            return false;
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (DFG.IsReady())
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) + Player.GetSpellDamage(enemy, SpellSlot.Q, 1);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady() || (rSpell.State == SpellState.Surpressed && R.Level > 0))
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 8;

            if (DFG.IsReady())
                damage = damage * 1.2;

            return (float)damage;
        }

        private static void Combo()
        {
            combo(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>());
        }

        private static void Harass()
        {
            harass(menu.Item("UseQHarass").GetValue<bool>(), menu.Item("UseWHarass").GetValue<bool>(),
                menu.Item("UseEHarass").GetValue<bool>(), false);
        }

        public static void combo(bool useQ, bool useW, bool useE, bool useR)
        {
            var Target = getTarget();

            var mode = menu.Item("comboMode").GetValue<StringList>().SelectedIndex;
            var IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;

            var eDis = menu.Item("eDis").GetValue<Slider>().Value;

            if (!Target.HasBuffOfType(BuffType.Invulnerability) && Target.IsValidTarget(E.Range))
            {
                if (mode == 0)//qwe
                {
                    if (Target != null && DFG.IsReady() && E.IsReady() && menu.Item("dfg").GetValue<bool>())
                    {
                        DFG.Cast(Target);
                    }

                    if (useQ && Q.IsReady() && Player.Distance(Target) <= Q.Range && Target != null)
                    {
                        Q.Cast(Target, packets());
                    }

                    if (useE && Target != null && E.IsReady() && Player.Distance(Target) < E.Range && Player.Distance(Target) > eDis)
                    {
                        if (menu.Item("smartE").GetValue<bool>() && countEnemiesNearPosition(Target.ServerPosition, 500) > 2 && (!R.IsReady() || !(rSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        E.Cast(Target, packets());
                    }

                }
                else if (mode == 1)//eqw
                {
                    if (Target != null && DFG.IsReady() && E.IsReady() && menu.Item("dfg").GetValue<bool>())
                    {
                        DFG.Cast(Target);
                    }

                    if (useE && Target != null && E.IsReady() && Player.Distance(Target) < E.Range && Player.Distance(Target) > eDis)
                    {
                        if (menu.Item("smartE").GetValue<bool>() && countEnemiesNearPosition(Target.ServerPosition, 500) > 2 && (!R.IsReady() || !(rSpell.State == SpellState.Surpressed && R.Level > 0)))
                            return;

                        E.Cast(Target, packets());
                    }

                    if (useQ && Q.IsReady() && Player.Distance(Target) <= Q.Range && Target != null)
                    {
                        Q.Cast(Target, packets());
                    }
                }

                //Ignite
                if (Target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                    Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (IgniteMode == 0 && GetComboDamage(Target) > Target.Health)
                    {
                        Player.SummonerSpellbook.CastSpell(IgniteSlot, Target);
                    }
                }

                if (useW && Target != null && W.IsReady() && Player.Distance(Target) <= W.Range)
                {
                    W.Cast();
                }

                if (useR && Target != null && R.IsReady() && Player.Distance(Target) <= R.Range)
                {
                    if (!Q.IsReady() && !E.IsReady() && !W.IsReady())
                        R.Cast();
                    return;
                }
            }
        }

        public static Obj_AI_Hero getTarget()
        {
            var tsMode = menu.Item("tsModes").GetValue<StringList>().SelectedIndex;
            var focusSelected = menu.Item("selected").GetValue<bool>();

            if (focusSelected && selectedTarget != null)
            {
                if (Player.Distance(selectedTarget) < 1600 && !selectedTarget.IsDead && selectedTarget.IsVisible && selectedTarget.IsEnemy)
                {
                    //Game.PrintChat("focusing selected target");
                    LXOrbwalker.ForcedTarget = selectedTarget;
                    return selectedTarget;
                }
                else
                {
                    selectedTarget = null;
                }
            }


            var getTar = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            if (tsMode == 0)
                return getTar;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.Distance(x) < E.Range && x.IsValidTarget(E.Range) && !x.IsDead && x.IsEnemy && x.IsVisible))
            {
                if (tsMode == 1)
                {
                    var tar1hp = target.Health / target.MaxHealth * 100;
                    var tar2hp = getTar.Health / getTar.MaxHealth * 100;
                    if (tar1hp < tar2hp)
                        getTar = target;
                }

                if (tsMode == 2)
                {
                    if (target.Distance(Game.CursorPos) < getTar.Distance(Game.CursorPos))
                        getTar = target;
                }

                if (tsMode == 3)
                {
                    if (target.Health < getTar.Health)
                        getTar = target;
                }
            }

            if (getTar != null)
            {
                LXOrbwalker.ForcedTarget = getTar;
                //Game.PrintChat("Focus Mode on: " + getTar.BaseSkinName);
                return getTar;
            }

            return null;
        }

        public static void harass(bool useQ, bool useW, bool useE, bool useR)
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            var mode = menu.Item("harassMode").GetValue<StringList>().SelectedIndex;

            if (mode == 0)//QEW
            {
                if (menu.Item("harassUnderTower").GetValue<bool>() | !inTowerRange(Player.ServerPosition.To2D()))
                {
                      if (useQ) { castQ(qTarget); }
                    
                    if (menu.Item("harassUnderTower").GetValue<bool>() | !inTowerRange(eTarget.ServerPosition.To2D()))
                    {
                        if (useE) { castE(eTarget); }
                    }
                }
            }
            else if (mode == 1)//EQW
            {
                if (!menu.Item("harassUnderTower").GetValue<bool>()) {
                    if (menu.Item("harassUnderTower").GetValue<bool>() | !inTowerRange(eTarget.ServerPosition.To2D()))
                    {
                        if (useE) { castE(eTarget); }
                    }

                    if (menu.Item("harassUnderTower").GetValue<bool>() | !inTowerRange(Player.ServerPosition.To2D()))
                    {
                        if (useQ) { castQ(qTarget); }
                    }
                }
                else
                {
                    if (useE) { castE(eTarget); }
                    if (useQ) { castQ(qTarget); }
                }

            }
            else if(mode == 2)//QW
            {
                if (menu.Item("harassUnderTower").GetValue<bool>() | !inTowerRange(Player.ServerPosition.To2D()))
                {
                    if (useQ) { castQ(qTarget); }
                }
            }
            if (menu.Item("harassUnderTower").GetValue<bool>() | !inTowerRange(Player.ServerPosition.To2D())) {
                if (useW) { castW(wTarget); }
            }
        }

        public static void castQ(Obj_AI_Hero target)
        {
            if (Q.IsReady() && Player.Distance(target) <= Q.Range && target != null)
            {
                Q.Cast(target, packets());
            }
        }

        public static void castW(Obj_AI_Hero target)
        {
            if (W.IsReady() && Player.Distance(target) <= W.Range && target != null)
            {
                W.Cast();
            }
        }

        public static void castE(Obj_AI_Hero target)
        {
            if (E.IsReady() && Player.Distance(target) < E.Range && target != null)
            {
                E.Cast(target, packets());
            }
        }

        public static void smartKS()
        {
            if (!menu.Item("smartKS").GetValue<bool>())
                return;

            if (menu.Item("rCancel").GetValue<bool>() && countEnemiesNearPosition(Player.ServerPosition, 570) > 1)
                return;

            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) <= 1375 && champ.IsEnemy select champ).ToList();
            nearChamps.OrderBy(x => x.Health);

            foreach (var target in nearChamps)
            {
                if (target != null && !target.IsDead && !target.HasBuffOfType(BuffType.Invulnerability) && target.IsValidTarget(1375))
                {
                    //E
                    if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20)
                    {
                        if (E.IsReady())
                        {
                            cancelUlt(target);
                            E.Cast(target, packets());
                            //Game.PrintChat("ks 8");
                            return;
                        }
                    }

                    //dfg
                    if (DFG.IsReady() && Player.GetItemDamage(target, Damage.DamageItems.Dfg) > target.Health + 20 && Player.Distance(target.ServerPosition) <= 750)
                    {
                        DFG.Cast(target);
                        //Game.PrintChat("ks 1");
                        return;
                    }

                    //dfg + q
                    if (Player.Distance(target.ServerPosition) <= Q.Range &&
                        (Player.GetItemDamage(target, Damage.DamageItems.Dfg) + (Player.GetSpellDamage(target, SpellSlot.Q)) * 1.2) > target.Health + 20)
                    {
                        if (DFG.IsReady() && Q.IsReady())
                        {
                            DFG.Cast(target);
                            cancelUlt(target);
                            Q.Cast(target, packets());
                            //Game.PrintChat("ks 2");
                            return;
                        }
                    }

                    //dfg + e
                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        (Player.GetItemDamage(target, Damage.DamageItems.Dfg) + (Player.GetSpellDamage(target, SpellSlot.E)) * 1.2) > target.Health + 20)
                    {
                        if (DFG.IsReady() && E.IsReady())
                        {
                            DFG.Cast(target);
                            cancelUlt(target);
                            E.Cast(target, packets());
                            //Game.PrintChat("ks 3");
                            return;
                        }
                    }
                    
                    //QEW
                    if (Player.Distance(target.ServerPosition) <= E.Range && 
                        (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady() && W.IsReady())
                        {
                            cancelUlt(target);
                            Q.Cast(target, packets());
                            E.Cast(target, packets());
                            if(Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                            return;
                        }
                    }

                    //E + W
                    if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                    {
                        if (E.IsReady() && W.IsReady())
                        {
                            cancelUlt(target);
                            E.Cast(target, packets());
                            if (Player.Distance(target.ServerPosition) < W.Range)
                                W.Cast();
                            //Game.PrintChat("ks 5");
                            return;
                        }
                    }

                    //E + Q
                    if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.E) + Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                    {
                        if (E.IsReady() && Q.IsReady())
                        {
                            cancelUlt(target);
                            E.Cast(target, packets());
                            Q.Cast(target, packets());
                            //Game.PrintChat("ks 6");
                            return;
                        }
                    }

                    //Q
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                    {
                        if (Q.IsReady() && Player.Distance(target.ServerPosition) <= Q.Range)
                        {
                            cancelUlt(target);
                            Q.Cast(target, packets());
                            //Game.PrintChat("ks 7");
                            return;
                        }
                        else if (Q.IsReady() && E.IsReady() && Player.Distance(target.ServerPosition) <= 1375 && menu.Item("wardKs").GetValue<bool>() && countEnemiesNearPosition(target.ServerPosition, 500) < 3)
                        {
                            cancelUlt(target);
                            jumpKS(target);
                            //Game.PrintChat("wardKS!!!!!");
                            return;
                        }
                    }


                    //R
                    if (Player.Distance(target.ServerPosition) <= E.Range && (Player.GetSpellDamage(target, SpellSlot.R) * 5) > target.Health + 20 && menu.Item("rKS").GetValue<bool>())
                    {
                        if (R.IsReady())
                        {
                            R.Cast();
                            //Game.PrintChat("ks 8");
                            return;
                        }
                    }

                    if (target != null && menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                            Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && Player.Distance(target.ServerPosition) <= 600)
                    {
                        var IgniteMode = menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
                        if (IgniteMode == 1 && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                        {
                            Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                        }
                    }
                }
            }
        }

        public static void cancelUlt(Obj_AI_Hero target)
        {
            if (Player.IsChannelingImportantSpell())
            {
                if (countEnemiesNearPosition(Player.ServerPosition, 600) < 2)
                {
                    LXOrbwalker.Orbwalk(target.ServerPosition, null);
                    return;
                }
            }

        }

        public static void shouldCancel()
        {
            if (countEnemiesNearPosition(Player.ServerPosition, 600) < 1)
            {
                var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) <= 1375 && champ.IsEnemy select champ).ToList();
                nearChamps.OrderBy(x => x.Health);

                if (nearChamps.FirstOrDefault() != null && nearChamps.FirstOrDefault().IsValidTarget(1375))
                    LXOrbwalker.Orbwalk(nearChamps.FirstOrDefault().ServerPosition, null);
            }
        }

        public static void autoW()
        {
            foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (target != null && !target.IsDead && target.IsEnemy && Player.Distance(target.ServerPosition) <= W.Range && target.IsValidTarget(W.Range) && !inTowerRange(Player.ServerPosition.To2D()))
                {
                    if (Player.Distance(target.ServerPosition) < W.Range && W.IsReady())
                        W.Cast();
                }
            }
        }

        //wardjump
        //-------------------------------------------------
        private static int lastPlaced = 0;
        private static Vector3 lastWardPos = new Vector3();

        public static void wardWalk(Vector3 pos)
        {
            LXOrbwalker.Orbwalk(pos, null);
            return;
        }

        public static void jumpKS(Obj_AI_Hero target)
        {
            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward =>
                E.IsReady() && Q.IsReady() && ward.Name.ToLower().Contains("ward") && ward.Distance(target.ServerPosition) < Q.Range && ward.Distance(Player) < E.Range))
            {
                E.Cast(ward);
                return;
            }

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero =>
                E.IsReady() && Q.IsReady() && hero.Distance(target.ServerPosition) < Q.Range && hero.Distance(Player) < E.Range && hero.IsValidTarget(E.Range)))
            {
                E.Cast(hero);
                return;
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
                E.IsReady() && Q.IsReady() && minion.Distance(target.ServerPosition) < Q.Range && minion.Distance(Player) < E.Range && minion.IsValidTarget(E.Range)))
            {
                E.Cast(minion);
                return;
            }

            if (Player.Distance(target) < Q.Range)
            {
                Q.Cast(target, packets());
                return;
            }

            if (E.IsReady() && Q.IsReady())
            {
                Vector3 position = Player.ServerPosition + Vector3.Normalize(target.ServerPosition - Player.ServerPosition) * 590;

                if (target.Distance(position) < Q.Range)
                {
                    InventorySlot invSlot = FindBestWardItem();
                    if (invSlot == null) return;

                    invSlot.UseItem(position);
                    lastWardPos = position;
                    lastPlaced = Environment.TickCount;
                }
            }

            if (Player.Distance(target) < Q.Range)
            {
                Q.Cast(target, packets());
                return;
            }

        }

        public static void wardJump(){
            //wardWalk(Game.CursorPos);

            foreach (Obj_AI_Minion ward in ObjectManager.Get<Obj_AI_Minion>().Where(ward => 
                ward.Name.ToLower().Contains("ward") && ward.Distance(Game.CursorPos) < 250))
            {
                if (E.IsReady())
                {
                    E.CastOnUnit(ward);
                    return;
                }
            }

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Distance(Game.CursorPos) < 250))
            {
                if (E.IsReady())
                {
                    E.CastOnUnit(hero);
                    return;
                }
            }

            foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion =>
                minion.Distance(Game.CursorPos) < 250 ))
            {
                if (E.IsReady())
                {
                    E.CastOnUnit(minion);
                    return;
                }
            }

            if (Environment.TickCount <= lastPlaced + 3000 || !E.IsReady()) return;

            Vector3 cursorPos = Game.CursorPos;
            Vector3 myPos = Player.ServerPosition;

            Vector3 delta = cursorPos - myPos;
            delta.Normalize();

            Vector3 wardPosition = myPos + delta * (600 - 5);

            InventorySlot invSlot = FindBestWardItem();
            if (invSlot == null) return;

            invSlot.UseItem(wardPosition);
            lastWardPos = wardPosition;
            lastPlaced = Environment.TickCount;
        }

        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }
        private static InventorySlot FindBestWardItem()
        {
            var slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;

            var sdi = GetItemSpell(slot);

            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return slot;
            }
            return null;
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (Environment.TickCount < lastPlaced + 300)
            {
                Obj_AI_Minion ward = (Obj_AI_Minion)sender;
                if (ward.Name.ToLower().Contains("ward") && ward.Distance(lastWardPos) < 500 && E.IsReady())
                {
                    E.Cast(ward);
                }
            }
        }

        //end wardjump
        //-------------------------------------------------
        //-------------------------------------------------

        public static int countEnemiesNearPosition(Vector3 pos, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>().Count(
                    hero => hero.IsEnemy && !hero.IsDead && hero.IsValid && hero.Distance(pos) <= range);
        }

        public static void lastHit()
        {
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQHit").GetValue<bool>();
            var useW = menu.Item("UseWHit").GetValue<bool>();
            var useE = menu.Item("UseEHit").GetValue<bool>();

            if (Q.IsReady() && useQ)
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < Damage.GetSpellDamage(Player, minion, SpellSlot.Q) - 20)
                    {
                        Q.CastOnUnit(minion, packets());
                        return;
                    }
                }
            }

            if (W.IsReady() && useW)
            {
                foreach (var minion in allMinionsW)
                {
                    if (minion.IsValidTarget(W.Range) && minion.Health < Damage.GetSpellDamage(Player, minion, SpellSlot.W) - 20)
                    {
                        if (Player.Distance(minion.ServerPosition) < W.Range)
                        {
                            W.Cast();
                            return;
                        }
                    }
                }
            }
            if (E.IsReady() && useE)
            {
                foreach (var minion in allMinionsE)
                {
                    if (minion.IsValidTarget(E.Range) && minion.Health < Damage.GetSpellDamage(Player, minion, SpellSlot.E) - 20)
                    {
                        if (Player.Distance(minion.ServerPosition) < E.Range)
                        {
                            E.CastOnUnit(minion, packets());
                            return;
                        }
                    }
                }
            }
        }

        private static void Farm()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useW = menu.Item("UseWFarm").GetValue<bool>();
            var useE = menu.Item("UseEFarm").GetValue<bool>();

            if (useE && allMinionsE.Count > 0 && E.IsReady() && allMinionsE[0].IsValidTarget(E.Range))
            {
                E.Cast(allMinionsE[0], packets());
            }

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady() && allMinionsQ[0].IsValidTarget(Q.Range))
            {
                Q.Cast(allMinionsQ[0], packets());
            }

            if (useW && W.IsReady())
            {
                var wPos = E.GetCircularFarmLocation(allMinionsW);
                if (wPos.MinionsHit >= 2)
                    W.Cast();
            }
        }

        private static void JungleFarm()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useW = menu.Item("UseWFarm").GetValue<bool>();

            if (useQ && allMinionsQ.Count > 0 && Q.IsReady() && allMinionsQ[0].IsValidTarget(Q.Range))
            {
                Q.Cast(allMinionsQ[0], packets());
            }

            if (useW && W.IsReady())
            {
                var wPos = E.GetCircularFarmLocation(allMinionsW);
                if (wPos.MinionsHit >= 1)
                    W.Cast();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            smartKS();

            if (Player.IsChannelingImportantSpell())
            {
                shouldCancel();
                return;
            }
           
            if (menu.Item("Wardjump").GetValue<KeyBind>().Active)
            {
                wardJump();
            }
            else if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {

                if (menu.Item("lastHit").GetValue<KeyBind>().Active)
                    lastHit();
                
                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("jFarm").GetValue<KeyBind>().Active)
                    JungleFarm();

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

            }

            if (menu.Item("autoWz").GetValue<bool>())
                autoW();

        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, (spell.IsReady()) ? Color.Cyan : Color.DarkRed);
            }

        }

        private static void Game_OnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] != Packet.C2S.SetTarget.Header)
            {
                return;
            }

            var decoded = Packet.C2S.SetTarget.Decoded(args.PacketData);

            if (decoded.NetworkId != 0 && decoded.Unit.IsValid && !decoded.Unit.IsMe)
            {
                selectedTarget = (Obj_AI_Hero)decoded.Unit;
                if (menu.Item("printTar").GetValue<bool>())
                    Game.PrintChat("Selected Target: " + decoded.Unit.BaseSkinName);
            }
        }
    }
}
