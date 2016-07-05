using System;
using System.Linq;
using System.Collections.Generic;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX;

using Ensage.Common.Objects;



namespace Tinker_Air13
{
    class Tinker_Air13
    {
        private static Ability Laser, Rocket, Refresh, March;
        private static Item blink, dagon, sheep, soulring, ethereal, shiva, ghost, cyclone, forcestaff, glimmer, bottle, travel, veil, aether, atos;
        private static Hero me, target;
        private static List<Hero> Alies;
		private static readonly Dictionary<Unit, ParticleEffect> VisibleUnit = new Dictionary<Unit, ParticleEffect>();
		private static readonly Dictionary<Unit, ParticleEffect> VisibleUnit2 = new Dictionary<Unit, ParticleEffect>();
		private static readonly Dictionary<Unit, ParticleEffect> VisibleUnit3 = new Dictionary<Unit, ParticleEffect>();
		private static readonly Dictionary<Unit, ParticleEffect> VisibleUnit4 = new Dictionary<Unit, ParticleEffect>();


		
        private static readonly Menu Menu = new Menu("Tinker Air13", "Tinker Air13", true, "npc_dota_hero_tinker", true);
        private static readonly Menu _skills = new Menu("Skills", "Skills");
        private static readonly Menu _items = new Menu("Items", "Items");
        private static readonly Menu _ranges = new Menu("Drawing", "Drawing");

        private static readonly Dictionary<string, bool> Skills = new Dictionary<string, bool>
            {
				{"tinker_rearm",true},
                {"tinker_march_of_the_machines",true},
                {"tinker_heat_seeking_missile",true},
                {"tinker_laser",true}
            };
        private static readonly Dictionary<string, bool> Items = new Dictionary<string, bool>
            {
                {"item_blink",true},
                {"item_force_staff",true},
                {"item_glimmer_cape",true},
                {"item_cyclone",true},
                {"item_shivas_guard",true},
                {"item_bottle",true},
                {"item_soul_ring",true},
                {"item_veil_of_discord",true},
                {"item_sheepstick",true},
                {"item_ghost",true},
                {"item_ethereal_blade",true},
                {"item_dagon",true}
            };

        private static readonly string[] SoulringSpells = 
			{
            "tinker_heat_seeking_missile",
            "tinker_rearm",
            "tinker_march_of_the_machines"
			};			
			
        private static int[] laser_damage = new int[4] { 80, 160, 240, 320 };
		private static int[] rocket_damage = new int[4] { 125, 200, 275, 350 };	
		
        private static int[] laser_mana = new int[4] { 95, 120, 145, 170 };
		private static int[] rocket_mana = new int[4] { 120, 140, 160, 180 };	
		private static int[] rearm_mana = new int[3] { 100, 200, 300 };	
		private static int dagon_mana = 180, veil_mana = 50, sheep_mana = 100, ethereal_mana = 100, shiva_mana = 100;
		
		private static int aetherrange = 0;
        private static double allmult = 1;
        private static int alldamage = 0, procastdamage = 0;
        private static double angle, targetangle;
		private static double etherealmult = 1, veilmult = 1, lensmult = 1, spellamplymult = 1;
		
        private static ParticleEffect linedisplay, rangedisplay_dagger, rangedisplay_dagger_inc, rangedisplay_rocket;
		private static	ParticleEffect effect2, effect3, effect4;

        private static int linerange, range_dagger, range_rocket;
			

        static void Main(string[] args)
        {
			/*
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
			*/
		
            // Menu Options
            Menu.AddItem(new MenuItem("Combo Key", "Combo Key").SetValue(new KeyBind('D', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("TargetLock", "Target Lock"))
                .SetValue(new StringList(new[] { "Free", "Lock" }));
			
            Menu.AddItem(new MenuItem("Rocket Spam Key", "Rocket Spam Key").SetValue(new KeyBind('F', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("March Spam Key", "March Spam Key").SetValue(new KeyBind('E', KeyBindType.Press)));

			Menu.AddItem(new MenuItem("autoDisable", "Auto disable/counter enemy").SetValue(true));
			Menu.AddItem(new MenuItem("autoKillsteal", "Auto killsteal enemy").SetValue(true));
			Menu.AddItem(new MenuItem("autoSoulring", "Auto SoulRing by manual spell usage").SetValue(true).SetTooltip("Disable it if you have some bugs with rearming or use other auto soulring/items assemblies"));

            Menu.AddSubMenu(_skills);
            Menu.AddSubMenu(_items);
            Menu.AddSubMenu(_ranges);

            _skills.AddItem(new MenuItem("Skills: ", "Skills:").SetValue(new AbilityToggler(Skills)));
            _items.AddItem(new MenuItem("Items: ", "Items:").SetValue(new AbilityToggler(Items)));
            _ranges.AddItem(new MenuItem("Blink Range", "Show Blink Dagger Range").SetValue(true));
            _ranges.AddItem(new MenuItem("Blink Range Incoming TP", "Show incoming TP Blink Range").SetValue(true));
            _ranges.AddItem(new MenuItem("Rocket Range", "Show Rocket Range").SetValue(true));
            _ranges.AddItem(new MenuItem("Show Direction", "Show Direction Vector on Rearming").SetValue(true));
            _ranges.AddItem(new MenuItem("Show Target Effect", "Show Target Effect").SetValue(true));


			var _settings = new Menu("Settings", "Settings UI");
            Menu.AddSubMenu(_settings);
			_settings.AddItem(new MenuItem("HitCounter", "Enable Hit counter").SetValue(true));
			_settings.AddItem(new MenuItem("RocketCounter", "Enable Rocket counter").SetValue(true));
			_settings.AddItem(new MenuItem("TargetCalculator", "Enable target dmg calculator").SetValue(true));
			_settings.AddItem(new MenuItem("Calculator", "Enable UI calculator").SetValue(true));
            _settings.AddItem(new MenuItem("BarPosX", "Position X").SetValue(new Slider(600, -1500, 1500)));
            _settings.AddItem(new MenuItem("BarPosY", "Position Y").SetValue(new Slider(0, -1500, 1500)));
			
            Menu.AddToMainMenu();
			
			Orbwalking.Load();

            //Game.OnWndProc += ComboEngine;
            Game.OnUpdate += ComboEngine;
			Game.OnUpdate += AD;

			
            Player.OnExecuteOrder += Player_OnExecuteAction;
			
            Drawing.OnDraw += Information;
			Drawing.OnDraw += DrawRanges;

        }
		
		
        private static void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args) 
		{
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
		
            switch (args.Order) {

                case Order.AbilityTarget:
                case Order.AbilityLocation:
                case Order.Ability:
                case Order.ToggleAbility:
                    if (!Game.IsKeyDown(16))
                        CastSpell(args);
                    break;
                case Order.MoveLocation:
                case Order.MoveTarget:
                default:
                    break;
            }
        }	

		private static void CastSpell(ExecuteOrderEventArgs args) 
		{
            var spell = args.Ability;
            if (!SoulringSpells.Any(spell.StoredName().Equals))
                return;			
			
            var soulRing = me.FindItem("item_soul_ring");
            var bottle = me.FindItem("item_bottle");
            if (soulRing == null && bottle == null)
                return;

            if (!Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name) && !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(bottle.Name))
                return;
				
			if (!Menu.Item("autoSoulring").GetValue<bool>())
				return;

            args.Process = false;
			


            switch (args.Order) 
			{
				
                case Order.AbilityTarget: 
				{
                    var target = args.Target as Unit;
                    if (target != null && target.IsAlive) {
                        spell.UseAbility(target);
                    }
                    break;
                }
                case Order.AbilityLocation: 
				{
			
					if (soulRing != null && soulRing.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulRing.Name)) 
						soulRing.UseAbility();		
					if (bottle != null && bottle.CanBeCasted() && !me.Modifiers.Any(x => x.Name == "modifier_bottle_regeneration") && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(bottle.Name) )
						bottle.UseAbility();
                    spell.UseAbility(Game.MousePosition);
                    break;
                }
                case Order.Ability: 
				{
					if (soulRing != null && soulRing.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulRing.Name)) 
						 soulRing.UseAbility();				
                    spell.UseAbility();
                    break;
                }
                case Order.ToggleAbility: 
				{
                    spell.ToggleAbility();
                    break;
                }
            }
        }

		
		
		
        public static void ComboEngine(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
				

				
				
			if (Game.IsKeyDown(Menu.Item("Rocket Spam Key").GetValue<KeyBind>().Key) && Utils.SleepCheck("RocketSpam") && !Game.IsChatOpen)
            {

				FindItems();

				if (blink != null && blink.CanBeCasted() && !me.IsChanneling()  && Utils.SleepCheck("Rearms") && (me.Distance2D(Game.MousePosition) > 700))
				{
					var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
					var p = Game.MousePosition;
					
					if (me.Distance2D(Game.MousePosition) > safeRange)
					{
						var tpos = me.Position;
						var a = tpos.ToVector2().FindAngleBetween(Game.MousePosition.ToVector2(), true);
						
						safeRange -= (int)me.HullRadius;
						p = new Vector3(
							tpos.X + safeRange * (float)Math.Cos(a),
							tpos.Y + safeRange * (float)Math.Sin(a),
							100);
					}
					else p = Game.MousePosition;				
				
					blink.UseAbility(p);
                    Utils.Sleep(250, "Blinks");

				}
						
				
				
				/*
				if (ghost != null && ghost.CanBeCasted() && !me.IsChanneling() && Menu.Item("Items2: ").GetValue<AbilityToggler>().IsEnabled(ghost.Name) && Utils.SleepCheck("Rearms"))
				{
					ghost.UseAbility(false);
				}
				*/
				if (soulring != null && soulring.CanBeCasted() && !me.IsChanneling() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name) && Utils.SleepCheck("Rearms"))
				{
					soulring.UseAbility();
				}
				if (bottle != null && bottle.CanBeCasted() && !me.IsChanneling() && !me.Modifiers.Any(x => x.Name == "modifier_bottle_regeneration") && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(bottle.Name) && Utils.SleepCheck("Rearms"))
				{
					bottle.UseAbility();
				}
				var enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.IsVisible && x.IsAlive && x.Team == me.GetEnemyTeam() && !x.IsIllusion);
				foreach (var e in enemies)
				{
					if (Rocket != null && Rocket.CanBeCasted() && (e != null && me.Distance2D(e) < 2500) && (blink == null || !blink.CanBeCasted() || me.Distance2D(Game.MousePosition) <= 700 ) && !me.IsChanneling() && !me.Spellbook.Spells.Any(x => x.IsInAbilityPhase) && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Rocket.Name) && Utils.SleepCheck("Rearms")) //&& me.Mana >= Rocket.ManaCost + 75 
					{
						Rocket.UseAbility();
					}
				
				
					if ((soulring == null || !soulring.CanBeCasted() || !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name)) && (!Rocket.CanBeCasted()  || Rocket.Level <= 0 || !Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Rocket.Name) || e == null || me.Distance2D(e) >= 2500) && (blink == null || !blink.CanBeCasted() || me.Distance2D(Game.MousePosition) <= 700 ) && (Refresh.Level >= 0 && Refresh.CanBeCasted()) && !me.IsChanneling() && !me.Spellbook.Spells.Any(x => x.IsInAbilityPhase) &&  Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Refresh.Name)  && Utils.SleepCheck("Rearms") && Utils.SleepCheck("Blinks"))
					{
						Refresh.UseAbility();
						if (Refresh.Level == 1)
							Utils.Sleep(3010, "Rearms");
						if (Refresh.Level == 2)
							Utils.Sleep(1510, "Rearms");
						if (Refresh.Level == 3)
							Utils.Sleep(760, "Rearms");

					}
                }
                if ((soulring == null || !soulring.CanBeCasted() || !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name)) && (blink == null || !blink.CanBeCasted() ) && (Refresh.Level >= 0 && Refresh.CanBeCasted()) && !me.IsChanneling() && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Refresh.Name) && Utils.SleepCheck("Rearms") && Utils.SleepCheck("Blinks"))
				{
					Refresh.UseAbility();
					if (Refresh.Level == 1)
						Utils.Sleep(3010, "Rearms");
					if (Refresh.Level == 2)
						Utils.Sleep(1510, "Rearms");
					if (Refresh.Level == 3)
						Utils.Sleep(760, "Rearms");

				}
				
				Utils.Sleep(120, "RocketSpam");
			}
			
			
			if (Game.IsKeyDown(Menu.Item("March Spam Key").GetValue<KeyBind>().Key) && Utils.SleepCheck("MarchSpam") && !Game.IsChatOpen)
            {
				FindItems();
				
				
				
				if (blink != null && blink.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(blink.Name) && !me.IsChanneling()  && Utils.SleepCheck("Rearms") && (me.Distance2D(Game.MousePosition) > 700))
				{
					var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
					var p = Game.MousePosition;
					
					if (me.Distance2D(Game.MousePosition) > safeRange)
					{
						var tpos = me.Position;
						var a = tpos.ToVector2().FindAngleBetween(Game.MousePosition.ToVector2(), true);
						
						safeRange -= (int)me.HullRadius;
						p = new Vector3(
							tpos.X + safeRange * (float)Math.Cos(a),
							tpos.Y + safeRange * (float)Math.Sin(a),
							100);
					}
					else p = Game.MousePosition;				
				
					blink.UseAbility(p);
                    Utils.Sleep(250, "Blinks");

				}
				
				
				/*
				if (ghost != null && ghost.CanBeCasted() && !me.IsChanneling() && Menu.Item("Items2: ").GetValue<AbilityToggler>().IsEnabled(ghost.Name) && Utils.SleepCheck("Rearms"))
				{
					ghost.UseAbility(false);
				}
				*/
				if (soulring != null && soulring.CanBeCasted() && !me.IsChanneling() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name) && Utils.SleepCheck("Rearms"))
				{
					soulring.UseAbility();
				}
				if (bottle != null && bottle.CanBeCasted() && !me.IsChanneling() && !me.Modifiers.Any(x => x.Name == "modifier_bottle_regeneration") && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(bottle.Name) && Utils.SleepCheck("Rearms"))
				{
					bottle.UseAbility();
				}
				if (March != null && March.CanBeCasted() && (blink == null || !blink.CanBeCasted() || me.Distance2D(Game.MousePosition) <= 700 || !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_blink")) && !me.IsChanneling() && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(March.Name) && Utils.SleepCheck("Rearms")) //&& me.Mana >= March.ManaCost + 75 
				{
					March.UseAbility(Game.MousePosition);
				}
			
				if ((soulring == null || !soulring.CanBeCasted() || !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name)) && (blink == null || !blink.CanBeCasted() || me.Distance2D(Game.MousePosition) <= 700 || !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_blink")) && (!March.CanBeCasted()  || March.Level <= 0 || !Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(March.Name)) && (Refresh.Level >= 0 && Refresh.CanBeCasted()) && !me.IsChanneling()&& Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Refresh.Name)  && Utils.SleepCheck("Rearms"))
				{
					Refresh.UseAbility();
					if (Refresh.Level == 1)
						Utils.Sleep(3010, "Rearms");
					if (Refresh.Level == 2)
						Utils.Sleep(1510, "Rearms");
					if (Refresh.Level == 3)
						Utils.Sleep(760, "Rearms");

				}

				
				Utils.Sleep(150, "MarchSpam");
			}
			
            

     

			aether = me.FindItem("item_aether_lens");
			if (aether == null)
				aetherrange = 0;
			else
				aetherrange = 200;
				
			if (!Game.IsKeyDown(Menu.Item("Combo Key").GetValue<KeyBind>().Key))
                target = null;
           
				
            if ((Game.IsKeyDown(Menu.Item("Combo Key").GetValue<KeyBind>().Key)) && !Game.IsChatOpen)
            {
                //target = me.ClosestToMouseTarget(2000);
				
				var targetLock =
					Menu.Item("TargetLock").GetValue<StringList>().SelectedIndex;
				
                if (Utils.SleepCheck("UpdateTarget")
                    && (target == null || !target.IsValid || !target.IsAlive || !target.IsVisible || (target.IsVisible && targetLock == 0)))
                {
                    target = TargetSelector.ClosestToMouse(me, 2000);
                    Utils.Sleep(250, "UpdateTarget");
                }
				
				
				
				
				
                if (target != null && target.IsAlive && !target.IsIllusion && !me.IsChanneling() && !me.Spellbook.Spells.Any(x => x.IsInAbilityPhase) && !CanReflectDamage(target))
                {
                    FindItems();
					

					
					if (Utils.SleepCheck("FASTCOMBO") && !me.IsChanneling() )
					{
						uint elsecount = 0;
						bool EzkillCheck = EZkill(target);
						bool magicimune = (!target.IsMagicImmune() && !target.Modifiers.Any(x => x.Name == "modifier_eul_cyclone"));
						uint[] dagondamage = new uint[5] { 400, 500, 600, 700, 800 };
						// soulring -> glimmer -> sheep -> veil-> ghost ->  ->   -> ethereal -> dagon ->  laser -> rocket -> shivas 

						
						
						if (soulring != null && soulring.CanBeCasted() 
							&& target.NetworkPosition.Distance2D(me) <= 2500 
							&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name)  )
						{
							soulring.UseAbility();
						}
						else
							elsecount += 1;							
						if (glimmer != null && glimmer.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(glimmer.Name) )
						{
							glimmer.UseAbility(me);
						}
						else
							elsecount += 1;
							
						/*
                        if (blink != null && blink.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(blink.Name) && !me.IsChanneling())
                        {
                            blink.UseAbility(Game.MousePosition);
                        }*/
						if (blink != null && blink.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(blink.Name) && !me.IsChanneling()  && (me.Distance2D(Game.MousePosition) > 600+aetherrange)  && (target.NetworkPosition.Distance2D(me) <= 1200 + 600 +aetherrange*2))// && Utils.SleepCheck("Rearms"))
						{
							var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
							var p13 = Game.MousePosition;
							
							if (me.Distance2D(Game.MousePosition) > safeRange)
							{
								var tpos = me.Position;
								var a = tpos.ToVector2().FindAngleBetween(Game.MousePosition.ToVector2(), true);
								
								safeRange -= (int)me.HullRadius;
								p13 = new Vector3(
									tpos.X + safeRange * (float)Math.Cos(a),
									tpos.Y + safeRange * (float)Math.Sin(a),
									100);
							}
							else p13 = Game.MousePosition;				
						
							blink.UseAbility(p13);
							//Utils.Sleep(250, "Blinks");

						}
						/*
						if (blink != null && blink.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(blink.Name) && !me.IsChanneling()  &&  me.NetworkPosition.Distance2D(target.NetworkPosition) > 600+aetherrange)// && Utils.SleepCheck("Rearms"))
						{
							var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
							var closeRange = me.FindItem("item_aether_lens") == null ? 600 : 800;
							var p13 = Game.MousePosition;
							
							if (me.NetworkPosition.Distance2D(target.NetworkPosition) > safeRange + closeRange)
							{
								var tpos = me.NetworkPosition;
								var a = tpos.ToVector2().FindAngleBetween(target.NetworkPosition.ToVector2(), true);
								
								safeRange -= (int)me.HullRadius;
								p13 = new Vector3(
									tpos.X + (safeRange + closeRange)  * (float)Math.Cos(a),
									tpos.Y + (safeRange + closeRange) * (float)Math.Sin(a),
									100);
							}
							else 
							{
								var tpos = me.NetworkPosition;
								var a = tpos.ToVector2().FindAngleBetween(target.NetworkPosition.ToVector2(), true);
								var uncloseRange = me.NetworkPosition.Distance2D(target.NetworkPosition) - closeRange;
								
								safeRange -= (int)me.HullRadius;
								p13 = new Vector3(
									tpos.X + uncloseRange * (float)Math.Cos(a),
									tpos.Y + uncloseRange * (float)Math.Sin(a),
									100);							
							}
							blink.UseAbility(p13);
							//Utils.Sleep(250, "Blinks");

						}*/						
						else
							elsecount += 1;
							
							
						if (target.IsLinkensProtected() && Utils.SleepCheck("combo2"))
						{
							if (forcestaff != null && forcestaff.CanBeCasted() )
								forcestaff.UseAbility(target);	
							else if (cyclone != null && cyclone.CanBeCasted() )
								cyclone.UseAbility(target);
							else if (Laser.Level >= 1 && Laser.CanBeCasted() )
								Laser.UseAbility(target);
								
							Utils.Sleep(200, "combo2");

						}
						else
						{
							
							
							/*
							if (sheep != null && sheep.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(sheep.Name) && magicimune   )
							{
								sheep.UseAbility(target);
							}
							else
								elsecount += 1;
							*/
							
							if (sheep != null && sheep.CanBeCasted() 
								//&& !target.UnitState.HasFlag(UnitState.Hexed) 
								//&& !target.UnitState.HasFlag(UnitState.Stunned) 
								&& magicimune 
								&& (!EzkillCheck || (target.FindItem("item_manta") != null && target.FindItem("item_manta").CanBeCasted()) || (target.FindItem("item_black_king_bar") != null && target.FindItem("item_black_king_bar").CanBeCasted()) )
								&& target.NetworkPosition.Distance2D(me) <= 800+aetherrange
								&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(sheep.Name)
								)
								sheep.UseAbility(target);
							else
								elsecount += 1;								/*
							if(veil != null && veil.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name) )
							{
								veil.UseAbility(target.Position);
							}
							else
								elsecount += 1;		*/
							if (veil != null && veil.CanBeCasted() 
								&& magicimune
								&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)
								&& target.NetworkPosition.Distance2D(me) <= 1500+aetherrange
								&& !OneHitLeft(target) 
								&& !(target.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
								&& !target.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff")
								)
								{
									if (me.Distance2D(target) > 1000 + aetherrange)
									{
										var a = me.Position.ToVector2().FindAngleBetween(target.Position.ToVector2(), true);
										var p1 = new Vector3(
											me.Position.X + (me.Distance2D(target) - 500) * (float)Math.Cos(a),
											me.Position.Y + (me.Distance2D(target) - 500) * (float)Math.Sin(a),
											100);
										veil.UseAbility(p1);
									}
									else if (me.Distance2D(target) <= 1000 + aetherrange)
										veil.UseAbility(target.NetworkPosition);
								}

							else
								elsecount += 1;		
								
							if (ghost != null && ethereal == null && ghost.CanBeCasted() 
								&& target.NetworkPosition.Distance2D(me) <= 800+aetherrange
								&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ghost.Name) )
							{
								ghost.UseAbility();
							}
							else
								elsecount += 1;

						

							/*
							if (ethereal != null && ethereal.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name) && magicimune && me.Distance2D(target) <= ethereal.CastRange && target.Health >= target.DamageTaken(dagondamage[dagon.Level - 1],DamageType.Magical,me,false,0,0,0))
							{
								ethereal.UseAbility(target);
							}
							else
								elsecount += 1;
							if (dagon != null && dagon.CanBeCasted() && (!ethereal.CanBeCasted() || target.Health <= target.DamageTaken(dagondamage[dagon.Level - 1], DamageType.Magical, me, false, 0, 0, 0)) && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon") && magicimune )
							{
								dagon.UseAbility(target);
							}
							else
								elsecount += 1;*/

							if (ethereal != null && ethereal.CanBeCasted() 
								&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)
								&& (!veil.CanBeCasted() || target.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff") || veil == null | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) 
								//&& (!silence.CanBeCasted() || target.Ishexed())
								&& magicimune
								&& !OneHitLeft(target) 
								&& !CanReflectDamage(target)
								&& target.NetworkPosition.Distance2D(me) <= 800+aetherrange
								&& !(target.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
								&& !target.Modifiers.Any(y => y.Name == "modifier_item_blade_mail_reflect")
								)
							{
								ethereal.UseAbility(target);
								//if (Utils.SleepCheck("etherealDelay") && me.Distance2D(target) <= ethereal.CastRange)
								//	Utils.Sleep(((me.NetworkPosition.Distance2D(target.NetworkPosition) / 1200) * 1000), "etherealDelay");
							}
							else
								elsecount += 1;
							if (dagon != null && dagon.CanBeCasted() 
								&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon")
								&& (!veil.CanBeCasted() || target.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff") || veil == null | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) 
								&& (ethereal == null || (ethereal!=null && !IsCasted(ethereal) && !ethereal.CanBeCasted()) || target.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal") | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)) 
								//&& (!silence.CanBeCasted() || target.Ishexed())
								&& magicimune
								&& !CanReflectDamage(target)
								&& !OneHitLeft(target) 
								&& target.NetworkPosition.Distance2D(me) <= 800+aetherrange
								&& !(target.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
								&& !target.Modifiers.Any(y => y.Name == "modifier_item_blade_mail_reflect")
								)
								dagon.UseAbility(target);
							else
								elsecount += 1;								
							
							
							/*
							if (Laser != null && Laser.CanBeCasted() && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Laser.Name) && magicimune )
							{
								Laser.UseAbility(target);
							}
							else
								elsecount += 1;							
							if (Rocket != null && Rocket.CanBeCasted() && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Rocket.Name) && magicimune  && me.Distance2D(target) <= Rocket.CastRange)
							{
								Rocket.UseAbility();

							}
							else
								elsecount += 1;
							*/
							if (Rocket.Level > 0 && Rocket.CanBeCasted() 
								&& target.NetworkPosition.Distance2D(me) <= 2500 
								&& (!EzkillCheck || target.NetworkPosition.Distance2D(me) >= 800+aetherrange)
								&& (!OneHitLeft(target) || target.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
								&& magicimune  && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Rocket.Name)
								&& (((veil == null || !veil.CanBeCasted() || target.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff") | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) && target.NetworkPosition.Distance2D(me) <= 1500 + aetherrange) || target.NetworkPosition.Distance2D(me) > 1500 + aetherrange)
								&& (((ethereal == null || (ethereal!=null && !ethereal.CanBeCasted()) || IsCasted(ethereal) /*|| target.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal")*/ | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))&& target.NetworkPosition.Distance2D(me) <= 800+aetherrange)|| target.NetworkPosition.Distance2D(me) > 800+aetherrange)
								&& !(target.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
							)
							{
								Rocket.UseAbility();
							}
							else
								elsecount += 1; 
								
							if (Laser.Level > 0 && Laser.CanBeCasted() 
								&& Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Laser.Name)
								&& !EzkillCheck 
								&& !OneHitLeft(target)
								&& magicimune 
								&& target.NetworkPosition.Distance2D(me) <= 650+aetherrange
								&& !(target.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
								)
								Laser.UseAbility(target);
							else
								elsecount += 1;					
								
							/*
							if (shiva != null && shiva.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name) && magicimune )
							{
								shiva.UseAbility();
							}
							else
								elsecount += 1;*/
							if (shiva != null && shiva.CanBeCasted() 
								&& !EzkillCheck 
								&& magicimune
								&& !OneHitLeft(target)
								&& target.NetworkPosition.Distance2D(me) <= 900
								&& !(target.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
								&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name)
								)
								shiva.UseAbility();
							else
								elsecount += 1;

							/*
							if (elsecount == 11)
							{
								if (me.Distance2D(target) > me.GetAttackRange()-150)
									Orbwalking.Orbwalk(target);
								else if (!target.IsAttackImmune())
									me.Attack(target);
								else
									me.Move(Game.MousePosition, false);
							}*/
							
							if (elsecount == 11 
								&& Refresh != null && Refresh.CanBeCasted() 
								&& Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Refresh.Name) 
								&& !me.IsChanneling() 
								&& (!OneHitLeft(target) || target.NetworkPosition.Distance2D(me) > 800+aetherrange)
								&& Utils.SleepCheck("Rearm") 
								&& Ready_for_refresh())
							{
								Refresh.UseAbility();
								if (Refresh.Level == 1)
									Utils.Sleep(3010, "Rearm");
								if (Refresh.Level == 2)
									Utils.Sleep(1510, "Rearm");
								if (Refresh.Level == 3)
									Utils.Sleep(760, "Rearm");
							}
							else
							{
								if (!me.IsChanneling() && me.CanAttack() && !target.IsAttackImmune() && Utils.SleepCheck("Rearm"))
									{
										if (me.Distance2D(target) > me.GetAttackRange()-150)
											Orbwalking.Orbwalk(target);
										else 
											me.Attack(target);
									}
								else
									me.Move(Game.MousePosition, false);
							}
							
							
							Utils.Sleep(150, "FASTCOMBO");
						}
                    }
                }
                else
                {
                    if (!me.IsChanneling() && !me.Spellbook.Spells.Any(x => x.IsInAbilityPhase))
                        me.Move(Game.MousePosition);
                }
            }

        }
		
		
		
		
		
		
		public static void AD(EventArgs args)
		{
			if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
		
			aether = me.FindItem("item_aether_lens");
			//cyclone = me.FindItem("item_cyclone");
			//ghost = me.FindItem("item_ghost");
            //sheep = me.FindItem("item_sheepstick");
            //atos = me.FindItem("item_rod_of_atos");
            FindItems();



			
			if (aether == null)
				aetherrange = 0;
			else
				aetherrange = 200;

				
				
			if (bottle != null && !me.IsInvisible() && !me.IsChanneling() && !me.Spellbook.Spells.Any(x => x.IsInAbilityPhase) && !March.IsInAbilityPhase && me.Modifiers.Any(x => x.Name == "modifier_fountain_aura_buff") && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(bottle.Name) && Utils.SleepCheck("bottle1"))
			{
				if(!me.Modifiers.Any(x => x.Name == "modifier_bottle_regeneration") && (me.Health < me.MaximumHealth || me.Mana < me.MaximumMana))
					bottle.UseAbility();
				Alies = ObjectMgr.GetEntities<Hero>().Where(x => x.Team == me.Team && x != me && (x.Health < x.MaximumHealth || x.Mana < x.MaximumMana) && !x.Modifiers.Any(y => y.Name == "modifier_bottle_regeneration") && x.IsAlive && !x.IsIllusion && x.Distance2D(me) <= bottle.CastRange).ToList();
				foreach (Hero v in Alies)
					if (v != null)
						bottle.UseAbility(v);
				Utils.Sleep(255, "bottle1");
			}
				

			var enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.IsVisible && x.IsAlive && x.Team == me.GetEnemyTeam() && !x.IsIllusion);
			
				
			foreach (var e in enemies)
			{
				if (e == null)
					return;
				//distance = me.Distance2D(e);
				angle = Math.Abs(e.FindAngleR() - Utils.DegreeToRadian(e.FindAngleForTurnTime(me.NetworkPosition)));

				if (Menu.Item("autoDisable").GetValue<bool>() && me.IsAlive && me.IsVisibleToEnemies)
				{		
					
					
					
					//break linken if tp
					if (!me.IsChanneling()
						&& me.Distance2D(e) <= 800 + 50 + aetherrange
						&& me.Distance2D(e) >= 300
						&& e.Modifiers.Any(y => y.Name == "modifier_teleporting")
						&& e.IsLinkensProtected()
						&& Utils.SleepCheck("tplink")
						)
					{
						if ((cyclone != null && cyclone.CanBeCasted()) || (sheep != null && sheep.CanBeCasted()))
						{ 
							if (atos != null && atos.CanBeCasted())
								atos.UseAbility(e);
							else if (me.Spellbook.SpellQ != null && me.Spellbook.SpellQ.CanBeCasted())
								me.Spellbook.SpellQ.UseAbility(e);
							else if (ethereal != null && ethereal.CanBeCasted())
								ethereal.UseAbility(e);
							else if (dagon != null && dagon.CanBeCasted())
								dagon.UseAbility(e);
							else if ((sheep != null && sheep.CanBeCasted()) && (cyclone != null && cyclone.CanBeCasted()))
								sheep.UseAbility(e);
							//else if (cyclone != null && cyclone.CanBeCasted())
							//    cyclone.UseAbility(e);
						}

						Utils.Sleep(150, "tplink");
					}
					

					
					
						//break TP 
						if (!me.IsChanneling()
							&& me.Distance2D(e) <= 800 + 50 + aetherrange
							&& e.Modifiers.Any(y => y.Name == "modifier_teleporting")
							//&& e.IsChanneling()
							&& !e.IsHexed()
							&& !e.Modifiers.Any(y => y.Name == "modifier_eul_cyclone")
							&& !e.IsLinkensProtected()
							&& Utils.SleepCheck("tplink1")
							)
						{
							if (sheep != null && sheep.CanBeCasted())
								sheep.UseAbility(e);
							else if (cyclone != null && cyclone.CanBeCasted())
								cyclone.UseAbility(e);
								
							Utils.Sleep(150, "tplink1");
						}




					//break channel by Hex
					if (!me.IsChanneling()
						&& sheep != null && sheep.CanBeCasted()
						&& me.Distance2D(e) <= 800 + 50 + aetherrange
						&& !e.Modifiers.Any(y => y.Name == "modifier_eul_cyclone")
						&& !e.IsSilenced()
						&& !e.IsMagicImmune()
						&& !e.IsLinkensProtected()
						&& !e.Modifiers.Any(y => y.Name == "modifier_teleporting")
						&& Utils.SleepCheck(e.Handle.ToString())
						&& (e.IsChanneling()
							|| (e.FindItem("item_blink") != null && IsCasted(e.FindItem("item_blink")))
						//break escape spells (1 hex, 2 seal) no need cyclone
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_QueenOfPain && e.FindSpell("queenofpain_blink").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_AntiMage && e.FindSpell("antimage_blink").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_StormSpirit && e.FindSpell("storm_spirit_ball_lightning").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Shredder && e.FindSpell("shredder_timber_chain").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Weaver && e.FindSpell("weaver_time_lapse").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_FacelessVoid && e.FindSpell("faceless_void_time_walk").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Phoenix && e.FindSpell("phoenix_icarus_dive").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Magnataur && e.FindSpell("magnataur_skewer").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Morphling && e.FindSpell("morphling_waveform").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomAssassin && e.FindSpell("phantom_assassin_phantom_strike").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && e.FindSpell("riki_blink_strike").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Spectre && e.FindSpell("spectre_haunt").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Furion && e.FindSpell("furion_sprout").IsInAbilityPhase
							
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomLancer && e.FindSpell("phantom_lancer_doppelwalk").IsInAbilityPhase



							//break special (1 hex, 2 cyclone)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen")
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_SpiritBreaker && e.Modifiers.Any(y => y.Name == "modifier_spirit_breaker_charge_of_darkness")
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Phoenix && e.Modifiers.Any(y => y.Name == "modifier_phoenix_icarus_dive")
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Magnataur && e.Modifiers.Any(y => y.Name == "modifier_magnataur_skewer_movement")


							
							//break rats shadow blades and invis (1 hex, 2 seal, 3 cyclone)
							|| e.IsMelee && me.Distance2D(e) <= 350 //test
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Legion_Commander && e.FindSpell("legion_commander_duel").Cooldown < 2 && me.Distance2D(e) < 480 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Tiny && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Pudge && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nyx_Assassin && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_BountyHunter && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nevermore && me.Distance2D(e) <= 350 
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Weaver && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Clinkz && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Broodmother && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Slark && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Ursa && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Earthshaker && (e.Spellbook.SpellQ.Cooldown<=1 || e.Spellbook.SpellR.Cooldown<=1) 
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Alchemist && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_TrollWarlord && me.Distance2D(e) <= 350 && !me.IsAttackImmune()

							//break rats blinkers (1 hex, 2 seal, 3 cyclone)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Ursa && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomAssassin && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Spectre && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_AntiMage && me.Distance2D(e) <= 350 && !me.IsAttackImmune()

							|| e.ClassID == ClassID.CDOTA_Unit_Hero_TemplarAssassin && me.Distance2D(e) <= e.GetAttackRange()+50 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Morphling && me.Distance2D(e) <= e.GetAttackRange()+50 && !me.IsAttackImmune()

							|| e.ClassID == ClassID.CDOTA_Unit_Hero_QueenOfPain && me.Distance2D(e) <= 800+50+aetherrange 
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Puck && me.Distance2D(e) <= 800+50+aetherrange 
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_StormSpirit && me.Distance2D(e) <= 800+50+aetherrange
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Phoenix && me.Distance2D(e) <= 800+50+aetherrange
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Magnataur && me.Distance2D(e) <= 800+50+aetherrange
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_FacelessVoid && me.Distance2D(e) <= 800+50+aetherrange


							//break mass dangerous spells (1 hex, 2 seal, 3 cyclone)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Necrolyte && e.FindSpell("necrolyte_reapers_scythe").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_FacelessVoid && e.FindSpell("faceless_void_chronosphere").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Magnataur && e.FindSpell("magnataur_reverse_polarity").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DoomBringer && e.FindSpell("doom_bringer_doom").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter && e.FindSpell("tidehunter_ravage").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Enigma && e.FindSpell("enigma_black_hole").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Rattletrap && e.FindSpell("rattletrap_power_cogs").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Luna && e.FindSpell("luna_eclipse").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nevermore && e.FindSpell("nevermore_requiem").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_SpiritBreaker && e.FindSpell("spirit_breaker_nether_strike").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Naga_Siren && e.FindSpell("naga_siren_song_of_the_siren").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Medusa && e.FindSpell("medusa_stone_gaze").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Treant && e.FindSpell("treant_overgrowth").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_AntiMage && e.FindSpell("antimage_mana_void").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Warlock && e.FindSpell("warlock_rain_of_chaos").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Terrorblade && e.FindSpell("terrorblade_sunder").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DarkSeer && e.FindSpell("dark_seer_wall_of_replica").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DarkSeer && e.FindSpell("dark_seer_surge").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Dazzle && e.FindSpell("dazzle_shallow_grave").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Omniknight && e.FindSpell("omniknight_guardian_angel").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Omniknight && e.FindSpell("omniknight_repel").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Beastmaster && e.FindSpell("beastmaster_primal_roar").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && e.FindSpell("chaos_knight_reality_rift").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && e.FindSpell("chaos_knight_phantasm").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Life_Stealer && e.FindSpell("life_stealer_infest").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Sven && e.FindSpell("sven_gods_strength").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DrowRanger && e.FindSpell("drow_ranger_wave_of_silence").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nyx_Assassin && e.FindSpell("nyx_assassin_mana_burn").IsInAbilityPhase
														
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Mirana && e.Spellbook.SpellW.IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_BountyHunter && e.Spellbook.SpellR.IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Phoenix && e.FindSpell("phoenix_icarus_dive").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_EarthSpirit && e.FindSpell("earth_spirit_magnetize").IsInAbilityPhase


							//break stun spells (1 hex, 2 seal, 3 cyclone)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Ogre_Magi && e.FindSpell("ogre_magi_fireblast").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Axe && e.FindSpell("axe_berserkers_call").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Lion && e.FindSpell("lion_impale").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nyx_Assassin && e.FindSpell("nyx_assassin_impale").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Rubick && e.FindSpell("rubick_telekinesis").IsInAbilityPhase
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Rubick && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 50)
							//|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Alchemist && e.FindSpell("alchemist_unstable_concoction_throw").IsInAbilityPhase)


							//break flying stun spells if enemy close (1 hex, 2 seal, 3 cyclone)  have cyclone
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sniper && e.Spellbook.SpellR.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)//e.FindSpell("sniper_assassinate").Cooldown > 0 && me.Modifiers.Any(y => y.Name == "modifier_sniper_assassinate"))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.1 && me.Distance2D(e) <= 400)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sven && e.Spellbook.SpellQ.IsInAbilityPhase && me.Distance2D(e) <= 300)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_SkeletonKing && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_VengefulSpirit && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)


							//break flying stun spells if enemy close (1 hex, 2 seal, 3 cyclone)  no cyclone
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sniper && e.Spellbook.SpellR.IsInAbilityPhase && angle <= 0.03 && (cyclone == null || !cyclone.CanBeCasted()))//e.FindSpell("sniper_assassinate").Cooldown > 0 && me.Modifiers.Any(y => y.Name == "modifier_sniper_assassinate"))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.1 && (cyclone == null || !cyclone.CanBeCasted()))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sven && e.Spellbook.SpellQ.IsInAbilityPhase && (cyclone == null || !cyclone.CanBeCasted()))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_SkeletonKing && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && (cyclone == null || !cyclone.CanBeCasted()))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && (cyclone == null || !cyclone.CanBeCasted()))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_VengefulSpirit && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && (cyclone == null || !cyclone.CanBeCasted()))
						   



							//break common dangerous spell (1 hex, 2 seal) //no need cyclone
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Bloodseeker && e.FindSpell("bloodseeker_rupture").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Mirana && e.FindSpell("mirana_invis").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && e.FindSpell("riki_smoke_screen").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && e.FindSpell("riki_tricks_of_the_trade").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Viper && e.FindSpell("viper_viper_strike").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Chen && e.FindSpell("chen_hand_of_god").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DeathProphet && e.FindSpell("death_prophet_silence").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DeathProphet && e.FindSpell("death_prophet_exorcism").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Invoker // =)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_EmberSpirit // =)


							
							//break hex spell
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Lion && e.Spellbook.SpellW.Level > 0 && e.Spellbook.SpellW.Cooldown < 1 && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 50)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_ShadowShaman && e.Spellbook.SpellW.Level > 0 && e.Spellbook.SpellW.Cooldown < 1 && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 50)
							|| (e.FindItem("item_sheepstick") != null && e.FindItem("item_sheepstick").Cooldown < 1 && me.Distance2D(e) < e.FindItem("item_sheepstick").GetCastRange() + 50)



							
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Omniknight && e.FindSpell("omniknight_purification").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Ursa && e.FindSpell("ursa_overpower").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Silencer && e.FindSpell("silencer_last_word").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Silencer && e.FindSpell("silencer_global_silence").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_ShadowShaman && e.FindSpell("shadow_shaman_mass_serpent_ward").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_QueenOfPain && e.FindSpell("queenofpain_sonic_wave").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer && e.FindSpell("obsidian_destroyer_astral_imprisonment").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer && e.FindSpell("obsidian_destroyer_sanity_eclipse").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Pugna && e.FindSpell("pugna_nether_ward").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Lich && e.FindSpell("lich_chain_frost").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_StormSpirit && e.FindSpell("storm_spirit_electric_vortex").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Zuus && e.FindSpell("zuus_thundergods_wrath").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Brewmaster && e.FindSpell("brewmaster_primal_split").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Bane && e.FindSpell("bane_fiends_grip").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Bane && e.FindSpell("bane_nightmare").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Undying && e.FindSpell("undying_tombstone").IsInAbilityPhase

							)
						)
					{
						sheep.UseAbility(e);
						Utils.Sleep(200, e.Handle.ToString());
					}

					
				
					
					
					//break channel by cyclone if not hex
					if (!me.IsChanneling()
						&& cyclone != null 
						&& cyclone.CanBeCasted()
						&& (sheep == null || !sheep.CanBeCasted() || e.IsLinkensProtected())
						&& me.Distance2D(e) <= 575+50+aetherrange
						&& !e.IsHexed()
						&& !e.IsMagicImmune()
						&& !e.IsSilenced()
						&& !e.Modifiers.Any(y => y.Name == "modifier_skywrath_mystic_flare_aura_effect")

						&& !e.Modifiers.Any(y => y.Name == "modifier_teleporting")
						&& Utils.SleepCheck(e.Handle.ToString())
						&& (e.IsChanneling()
							|| (e.FindItem("item_blink") != null && IsCasted(e.FindItem("item_blink")))

							//break rats shadow blades and invis if they appear close(1 hex, 2 seal, 3 cyclone)
							|| (e.IsMelee && me.Distance2D(e) <= 350 && (me.Spellbook.SpellR == null || !me.Spellbook.SpellR.CanBeCasted() )) //test
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Legion_Commander && e.FindSpell("legion_commander_duel").Cooldown < 2 && me.Distance2D(e) < 480 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Tiny && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Pudge && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nyx_Assassin && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_BountyHunter && me.Distance2D(e) <= 350
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Weaver && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Clinkz && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Broodmother && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Slark && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Earthshaker && (e.Spellbook.SpellQ.Cooldown <= 1 || e.Spellbook.SpellR.Cooldown <= 1) 
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Alchemist && me.Distance2D(e) <= 350 && !me.IsAttackImmune()
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_TrollWarlord && me.Distance2D(e) <= 350 && !me.IsAttackImmune()

							
							//break rats blinkers (1 hex, 2 seal, 3 cyclone)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_QueenOfPain && me.Distance2D(e) <= 575+50+aetherrange 
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Puck && me.Distance2D(e) <= 575+50+aetherrange 
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_StormSpirit && me.Distance2D(e) <= 575+50+aetherrange
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_FacelessVoid && me.Distance2D(e) <= 575+50+aetherrange
							
							
							//break mass dangerous spells (1 hex, 2 seal, 3 cyclone)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Necrolyte && e.FindSpell("necrolyte_reapers_scythe").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_FacelessVoid && e.FindSpell("faceless_void_chronosphere").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Magnataur && e.FindSpell("magnataur_reverse_polarity").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DoomBringer && e.FindSpell("doom_bringer_doom").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter && e.FindSpell("tidehunter_ravage").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Enigma && e.FindSpell("enigma_black_hole").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Rattletrap && e.FindSpell("rattletrap_power_cogs").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Luna && e.FindSpell("luna_eclipse").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nevermore && e.FindSpell("nevermore_requiem").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_SpiritBreaker && e.FindSpell("spirit_breaker_nether_strike").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Naga_Siren && e.FindSpell("naga_siren_song_of_the_siren").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Medusa && e.FindSpell("medusa_stone_gaze").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Treant && e.FindSpell("treant_overgrowth").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_AntiMage && e.FindSpell("antimage_mana_void").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Warlock && e.FindSpell("warlock_rain_of_chaos").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Terrorblade && e.FindSpell("terrorblade_sunder").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DarkSeer && e.FindSpell("dark_seer_wall_of_replica").IsInAbilityPhase
							//|| e.ClassID == ClassID.CDOTA_Unit_Hero_DarkSeer && e.FindSpell("dark_seer_surge").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Dazzle && e.FindSpell("dazzle_shallow_grave").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Omniknight && e.FindSpell("omniknight_guardian_angel").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Omniknight && e.FindSpell("omniknight_repel").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Beastmaster && e.FindSpell("beastmaster_primal_roar").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && e.FindSpell("chaos_knight_reality_rift").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && e.FindSpell("chaos_knight_phantasm").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Life_Stealer && e.FindSpell("life_stealer_infest").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Sven && e.FindSpell("sven_gods_strength").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_DrowRanger && e.FindSpell("drow_ranger_wave_of_silence").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nyx_Assassin && e.FindSpell("nyx_assassin_mana_burn").IsInAbilityPhase

							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Phoenix && e.FindSpell("phoenix_icarus_dive").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_EarthSpirit && e.FindSpell("earth_spirit_magnetize").IsInAbilityPhase
							
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Furion && e.FindSpell("furion_sprout").IsInAbilityPhase


							//break stun spells (1 hex, 2 seal, 3 cyclone)
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Ogre_Magi && e.FindSpell("ogre_magi_fireblast").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Axe && e.FindSpell("axe_berserkers_call").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Lion && e.FindSpell("lion_impale").IsInAbilityPhase
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nyx_Assassin && e.FindSpell("nyx_assassin_impale").IsInAbilityPhase      
							|| e.ClassID == ClassID.CDOTA_Unit_Hero_Rubick && e.FindSpell("rubick_telekinesis").IsInAbilityPhase
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Rubick && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 50)

							
							//break hex spell
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Lion && e.Spellbook.SpellW.Level > 0 && e.Spellbook.SpellW.Cooldown < 1 && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 50)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_ShadowShaman && e.Spellbook.SpellW.Level > 0 && e.Spellbook.SpellW.Cooldown < 1 && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 50)
							|| (e.FindItem("item_sheepstick") != null && e.FindItem("item_sheepstick").Cooldown < 1 && me.Distance2D(e) < e.FindItem("item_sheepstick").GetCastRange() + 50)

							
							//break flying stun spells if enemy close (1 hex, 2 seal, 3 cyclone)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sniper && e.Spellbook.SpellR.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)//e.FindSpell("sniper_assassinate").Cooldown > 0 && me.Modifiers.Any(y => y.Name == "modifier_sniper_assassinate"))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.1 && me.Distance2D(e) <= 400)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sven && e.Spellbook.SpellQ.IsInAbilityPhase && me.Distance2D(e) <= 300)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_SkeletonKing && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_VengefulSpirit && e.Spellbook.SpellQ.IsInAbilityPhase && angle <= 0.03 && me.Distance2D(e) <= 300)
							
							)
						)
					{
						cyclone.UseAbility(e);
						Utils.Sleep(50, e.Handle.ToString());
					}
					
							
					
					
					//cyclone dodge	
					if (Utils.SleepCheck("item_cyclone") && cyclone != null && cyclone.CanBeCasted())
					{
						//use on me
						var mod =
							me.Modifiers.FirstOrDefault(
								x =>
									x.Name == "modifier_lina_laguna_blade" ||
								   
									//x.Name == "modifier_orchid_malevolence_debuff" || 
									//x.Name == "modifier_skywrath_mage_ancient_seal" ||

									x.Name == "modifier_lion_finger_of_death");
							
						if (cyclone != null && cyclone.CanBeCasted() && 
							(mod != null
							|| (me.IsRooted() && !me.Modifiers.Any(y => y.Name == "modifier_razor_unstablecurrent_slow")  )
							//|| e.ClassID == ClassID.CDOTA_Unit_Hero_Zuus && e.FindSpell("zuus_thundergods_wrath").IsInAbilityPhase  //zuus can cancel
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Huskar && IsCasted(e.Spellbook.SpellR) && angle <= 0.15 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 250) //( (e.FindSpell("huskar_life_break").Cooldown >= 3 && e.AghanimState()) || (e.FindSpell("huskar_life_break").Cooldown >= 11 && !e.AghanimState())) && me.Distance2D(e) <= 400)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Juggernaut && e.Modifiers.Any(y => y.Name == "modifier_juggernaut_omnislash") && me.Distance2D(e) <= 300  && !me.IsAttackImmune())// && (ghost == null || !ghost.CanBeCasted()) && (ghost == null || !ghost.CanBeCasted())&& !me.Modifiers.Any(y => y.Name == "modifier_item_ghost_scepter")


							//dodge flying stuns
							|| (e.FindItem("item_ethereal_blade")!=null && IsCasted(e.FindItem("item_ethereal_blade")) && angle <= 0.1 && me.Distance2D(e) < e.FindItem("item_ethereal_blade").GetCastRange() + 250)

							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sniper && IsCasted(e.Spellbook.SpellR) && me.Distance2D(e) > 300 && me.Modifiers.Any(y => y.Name == "modifier_sniper_assassinate"))//e.FindSpell("sniper_assassinate").Cooldown > 0 && me.Modifiers.Any(y => y.Name == "modifier_sniper_assassinate"))
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Tusk && angle <= 0.35 && e.Modifiers.Any(y => y.Name == "modifier_tusk_snowball_movement") && me.Distance2D(e) <= 575)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && IsCasted(e.Spellbook.SpellQ) && angle <= 0.12 && me.Distance2D(e) > 400 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 550)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sven && IsCasted(e.Spellbook.SpellQ) && angle <= 0.3 && me.Distance2D(e) > 300 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 500)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_SkeletonKing && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) > 300 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) > 300 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_VengefulSpirit && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) > 300 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Alchemist && e.FindSpell("alchemist_unstable_concoction_throw").IsInAbilityPhase && angle <= 0.3 && me.Distance2D(e) < e.FindSpell("alchemist_unstable_concoction_throw").GetCastRange() + 500)

							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Viper && IsCasted(e.Spellbook.SpellR) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellR.GetCastRange() + 350)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomLancer && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) > 300 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Morphling && IsCasted(e.Spellbook.SpellW) && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 350)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) > 300  && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 150)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Visage && IsCasted(e.Spellbook.SpellW) && angle <= 0.1 && me.Distance2D(e) > 300  && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 250)
							|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Lich && IsCasted(e.Spellbook.SpellR) && angle <= 0.5  && me.Distance2D(e) < e.Spellbook.SpellR.GetCastRange() + 350)

							
							//free silence
							|| (me.IsSilenced() && !me.IsHexed() && !me.Modifiers.Any(y => y.Name == "modifier_doom_bringer_doom") && !me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen")&& !me.Modifiers.Any(y => y.Name == "modifier_disruptor_static_storm")))
							
							//free debuff
							|| me.Modifiers.Any(y => y.Name == "modifier_oracle_fortunes_end_purge") 
							|| me.Modifiers.Any(y => y.Name == "modifier_life_stealer_open_wounds") 
							)
						{
							cyclone.UseAbility(me);
							Utils.Sleep(150, "item_cyclone");
							return;
						}

						/*
						//use on enemy cyclone
						else if (cyclone != null 
								&& cyclone.CanBeCasted() 
								&& me.Distance2D(e) <= 575 + 50 + aetherrange 
								&& (e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen")
									|| e.ClassID == ClassID.CDOTA_Unit_Hero_SpiritBreaker && e.Modifiers.Any(y => y.Name == "modifier_spirit_breaker_charge_of_darkness")
									)
								)
						{
							cyclone.UseAbility(e);
							Utils.Sleep(150, "item_cyclone");
							return;

						}
						 //use on enemy sheep
						else if ((cyclone ==null || !cyclone.CanBeCasted() || me.Distance2D(e) > 575 + 50 + aetherrange) 
								&& sheep != null && sheep.CanBeCasted()
								&& me.Distance2D(e) <= 800 + 50 + aetherrange
								&& (e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen")
									|| e.ClassID == ClassID.CDOTA_Unit_Hero_SpiritBreaker && e.Modifiers.Any(y => y.Name == "modifier_spirit_breaker_charge_of_darkness")
									)
								)
						{
							sheep.UseAbility(e);
							Utils.Sleep(150, "item_cyclone");
							return;

						}
						*/
						

					}
					
					
					
					
					//Laser dodge close enemy
					if (
										Laser != null
										&& Laser.CanBeCasted()
										&& (sheep == null || !sheep.CanBeCasted())
										&& !me.IsAttackImmune()
										&& !e.IsHexed()
										&& !e.IsMagicImmune()
										&& angle <= 0.03
										&& ( (e.IsMelee && me.Position.Distance2D(e) < 250)
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_TemplarAssassin && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_TrollWarlord && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Clinkz && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Weaver && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Huskar && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Nevermore && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && me.Distance2D(e) <= e.GetAttackRange()+50 && IsCasted(e.Spellbook.SpellR)// && e.Modifiers.Any(y => y.Name == "modifier_windrunner_focusfire"))
											)
										&& e.IsAttacking() 
										&& Utils.SleepCheck("Ghost"))
									{
										Laser.UseAbility(e);
										Utils.Sleep(150, "Ghost");
									}
									/*
									else if (Laser != null
											&& Laser.CanBeCasted()
											&& !me.IsAttackImmune()
											&& e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && IsCasted(e.Spellbook.SpellR)//&& e.Modifiers.Any(y => y.Name == "modifier_windrunner_focusfire")
											//&& e.IsAttacking() 
										   && angle <= 0.03
											&& Utils.SleepCheck("Ghost")
											)
									{
										ghost.UseAbility();
										Utils.Sleep(150, "Ghost");
									}*/
					
					
					
					
					
					

					//ghost dodge close enemy
					if (
										ghost != null
										&& ghost.CanBeCasted()
										&& (sheep == null || !sheep.CanBeCasted())
										&& (Laser == null || !Laser.CanBeCasted() || e.Modifiers.Any(y => y.Name == "modifier_juggernaut_omnislash"))
										&& !me.IsAttackImmune()
										&& !e.IsHexed()
										&& (!e.Modifiers.Any(y => y.Name == "modifier_tinker_laser_blind") || e.Modifiers.Any(y => y.Name == "modifier_juggernaut_omnislash"))
										&& angle <= 0.03
										&& ((e.IsMelee && me.Position.Distance2D(e) < 350)
											&& e.ClassID != ClassID.CDOTA_Unit_Hero_Tiny
											&& e.ClassID != ClassID.CDOTA_Unit_Hero_Shredder
											&& e.ClassID != ClassID.CDOTA_Unit_Hero_Nyx_Assassin
											&& e.ClassID != ClassID.CDOTA_Unit_Hero_Meepo
											&& e.ClassID != ClassID.CDOTA_Unit_Hero_Earthshaker
											&& e.ClassID != ClassID.CDOTA_Unit_Hero_Centaur
											
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_TemplarAssassin && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_TrollWarlord && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Clinkz && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Weaver && me.Distance2D(e) <= e.GetAttackRange()+50
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Huskar && me.Distance2D(e) <= e.GetAttackRange()+50
											//|| e.Modifiers.Any(y => y.Name == "modifier_juggernaut_omnislash")
											|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && IsCasted(e.Spellbook.SpellR))// && e.Modifiers.Any(y => y.Name == "modifier_windrunner_focusfire"))

											)
										&& e.IsAttacking() 
										&& Utils.SleepCheck("Ghost"))
									{
										ghost.UseAbility();
										Utils.Sleep(150, "Ghost");
									}
									/*
									else if (ghost != null
											&& ghost.CanBeCasted()
											&& !me.IsAttackImmune()
											&& e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && IsCasted(e.Spellbook.SpellR)//&& e.Modifiers.Any(y => y.Name == "modifier_windrunner_focusfire")
											//&& e.IsAttacking() 
										   && angle <= 0.03
											&& Utils.SleepCheck("Ghost")
											)
									{
										ghost.UseAbility();
										Utils.Sleep(150, "Ghost");
									}*/
									
								
					//cyclone dodge attacking close enemy		
					if (
										(ghost == null || !ghost.CanBeCasted())
										&& (sheep == null || !sheep.CanBeCasted())
										&& (Laser == null || !Laser.CanBeCasted())
										//&& (me.Spellbook.SpellE == null || !me.Spellbook.SpellE.CanBeCasted())

										&& cyclone != null 
										&& cyclone.CanBeCasted()
										&& me.Distance2D(e) <= 575 + 50 + aetherrange
										&& !me.IsAttackImmune()
										&& !e.IsHexed()
										&& !e.Modifiers.Any(y => y.Name == "modifier_tinker_laser_blind")
										&& !e.Modifiers.Any(y => y.Name == "modifier_skywrath_mystic_flare_aura_effect")

										&& angle <= 0.03
										&& (e.ClassID == ClassID.CDOTA_Unit_Hero_Ursa 
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomAssassin 
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki
											
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Sven
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Spectre 
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_AntiMage 

											|| e.ClassID == ClassID.CDOTA_Unit_Hero_TemplarAssassin 
											|| e.ClassID == ClassID.CDOTA_Unit_Hero_Morphling 
											
											)
										
										&& e.IsAttacking() 
										&& Utils.SleepCheck("Ghost"))
									{
										cyclone.UseAbility(e);
										Utils.Sleep(150, "Ghost");
									}
					else if ( //если цель под ультой ская
									(ghost == null || !ghost.CanBeCasted())
									&& (sheep == null || !sheep.CanBeCasted())
									&& (Laser == null || !Laser.CanBeCasted())
						//&& (me.Spellbook.SpellE == null || !me.Spellbook.SpellE.CanBeCasted())
									&& cyclone != null
									&& cyclone.CanBeCasted()
									&& me.Distance2D(e) <= 575 + 50 + aetherrange
									&& !me.IsAttackImmune()
									&& !e.IsHexed()
									&& e.Modifiers.Any(y => y.Name == "modifier_skywrath_mystic_flare_aura_effect") ////!!!!!!!!

									&& angle <= 0.03
									&& (e.ClassID == ClassID.CDOTA_Unit_Hero_Ursa
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomAssassin
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_Riki
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_Spectre
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_AntiMage

										|| e.ClassID == ClassID.CDOTA_Unit_Hero_TemplarAssassin
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_Morphling
										)

									&& e.IsAttacking()
									&& Utils.SleepCheck("Ghost"))
					{
						cyclone.UseAbility(me);
						Utils.Sleep(150, "Ghost");
					}

					
					

					

					else if ( //break special (1 hex, 2 cyclone)
									!me.IsChanneling()
						//&& (me.Spellbook.SpellE == null || !me.Spellbook.SpellE.CanBeCasted())
									&& cyclone != null
									&& cyclone.CanBeCasted()
									&& (sheep == null || !sheep.CanBeCasted())
									&& me.Distance2D(e) <= 575 + 50 + aetherrange
									&& !e.IsHexed()
									&& !e.Modifiers.Any(y => y.Name == "modifier_skywrath_mystic_flare_aura_effect") ////!!!!!!!!

									&& (
										//break special (1 hex, 2 cyclone)
										e.ClassID == ClassID.CDOTA_Unit_Hero_Riki && me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen")
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_SpiritBreaker && e.Modifiers.Any(y => y.Name == "modifier_spirit_breaker_charge_of_darkness")
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_Phoenix && e.Modifiers.Any(y => y.Name == "modifier_phoenix_icarus_dive")
										|| e.ClassID == ClassID.CDOTA_Unit_Hero_Magnataur && e.Modifiers.Any(y => y.Name == "modifier_magnataur_skewer_movement")

										)
									&& Utils.SleepCheck("Ghost"))
					{
						cyclone.UseAbility(e);
						Utils.Sleep(150, "Ghost");
					}
					


									else if ( // Если ВРка
											(ghost == null || !ghost.CanBeCasted())
											&& (Laser == null || !Laser.CanBeCasted())
											&& cyclone != null 
											&& cyclone.CanBeCasted()
											&& !me.IsAttackImmune()
											&& !e.Modifiers.Any(y => y.Name == "modifier_skywrath_mystic_flare_aura_effect")
											&& e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && IsCasted(e.Spellbook.SpellR)//&& e.Modifiers.Any(y => y.Name == "modifier_windrunner_focusfire")
											//&& e.IsAttacking() 
										   && angle <= 0.03
											&& Utils.SleepCheck("Ghost")
											)
									{
										cyclone.UseAbility(e);
										Utils.Sleep(150, "Ghost");
									}

					
				}
				
			
				if (Menu.Item("autoKillsteal").GetValue<bool>() && me.IsAlive && me.IsVisible)
				{
				
					if (e.Health < (factdamage(e))
						&& !CanReflectDamage(e)
						//&& (!e.FindSpell("abaddon_borrowed_time").CanBeCasted() && !e.Modifiers.Any(y => y.Name == "modifier_abaddon_borrowed_time_damage_redirect"))
						&& !e.Modifiers.Any(y => y.Name == "modifier_abaddon_borrowed_time_damage_redirect")
						&& !e.Modifiers.Any(y => y.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison")
						&& !e.Modifiers.Any(y => y.Name == "modifier_puck_phase_shift")
						&& !e.Modifiers.Any(y => y.Name == "modifier_eul_cyclone")
						&& !e.Modifiers.Any(y => y.Name == "modifier_dazzle_shallow_grave")
						&& !e.Modifiers.Any(y => y.Name == "modifier_brewmaster_storm_cyclone")
						&& !e.Modifiers.Any(y => y.Name == "modifier_shadow_demon_disruption")
						&& !e.Modifiers.Any(y => y.Name == "modifier_tusk_snowball_movement")
						&& !me.Modifiers.Any(y => y.Name == "modifier_pugna_nether_ward_aura")
						)
						{
							if (Utils.SleepCheck("AUTOCOMBO") && !me.IsChanneling())
							{

								bool EzkillCheck = EZkill(e);
								bool magicimune = (!e.IsMagicImmune() && !e.Modifiers.Any(x => x.Name == "modifier_eul_cyclone"));
								
								if (soulring != null && soulring.CanBeCasted()
									&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name)  
									&& e.NetworkPosition.Distance2D(me) < 2500
									&& magicimune  
									&& (!OneHitLeft(e) || e.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
									&& (((veil == null || !veil.CanBeCasted() || e.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff")  | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) && e.NetworkPosition.Distance2D(me) <= 1500+aetherrange) || ((e.NetworkPosition.Distance2D(me) > 1500+aetherrange) && (e.Health < (int)(e.DamageTaken(rocket_damage[Rocket.Level - 1], DamageType.Magical, me, false, 0, 0, 0)*spellamplymult*lensmult)))   )
									&& (((ethereal == null || (ethereal!=null && !ethereal.CanBeCasted()) || IsCasted(ethereal) /*|| e.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal")*/ | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))&& e.NetworkPosition.Distance2D(me) <= 800+aetherrange)|| ((e.NetworkPosition.Distance2D(me) > 800+aetherrange) && (e.Health < (int)(e.DamageTaken(rocket_damage[Rocket.Level - 1], DamageType.Magical, me, false, 0, 0, 0)*spellamplymult*lensmult)))   )
																		
									)
									soulring.UseAbility();
									
								
								if (veil != null && veil.CanBeCasted() 
									&& magicimune
									&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)
									&& e.NetworkPosition.Distance2D(me) <= 1500+aetherrange
									&& (!OneHitLeft(e) || e.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
									&& !(e.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
									&& !e.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff")
									)
								{
									if (me.Distance2D(e)>1000 + aetherrange)
									{
										var a = me.Position.ToVector2().FindAngleBetween(e.Position.ToVector2(), true);
										var p1 = new Vector3(
											me.Position.X + (me.Distance2D(e)-500) * (float)Math.Cos(a),
											me.Position.Y + (me.Distance2D(e)-500) * (float)Math.Sin(a),
											100);
										veil.UseAbility(p1);
									}
									else if (me.Distance2D(e)<=1000 + aetherrange)
										veil.UseAbility(e.NetworkPosition);

								}
		

									

								if (ethereal != null && ethereal.CanBeCasted() 
									&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)
									&& (!veil.CanBeCasted() || e.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff") || veil == null | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) 
									&& (!OneHitLeft(e) || e.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
									&& magicimune
									&& e.NetworkPosition.Distance2D(me) <= 800+aetherrange
									&& !(e.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
									)
									ethereal.UseAbility(e);

								if (dagon != null && dagon.CanBeCasted() 
									&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon")
									&& (!veil.CanBeCasted() || e.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff") || veil == null | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) 
									&& (ethereal == null || (ethereal!=null && !IsCasted(ethereal) && !ethereal.CanBeCasted()) || e.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal") | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)) 
									&& (!OneHitLeft(e) || e.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
									&& magicimune
									&& e.NetworkPosition.Distance2D(me) <= 800+aetherrange
									&& !(e.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
									)
									dagon.UseAbility(e);
							
								
								
								if (Rocket.Level > 0 && Rocket.CanBeCasted() 
									&& e.NetworkPosition.Distance2D(me) <= 2500 
									&& (!EzkillCheck || e.NetworkPosition.Distance2D(me) >= 800+aetherrange)
									&& (!OneHitLeft(e) || e.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
									&& magicimune  
									&& Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Rocket.Name) 
									//&& (((veil == null || !veil.CanBeCasted() || e.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff")  | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) && e.NetworkPosition.Distance2D(me) <= 1500+aetherrange)|| ((e.NetworkPosition.Distance2D(me) > 1500+aetherrange) && (e.Health < (int)(e.DamageTaken(rocket_damage[Rocket.Level - 1], DamageType.Magical, me, false, 0, 0, 0)*spellamplymult*lensmult)))   )
									//&& (((ethereal == null || (ethereal!=null && !ethereal.CanBeCasted()) || IsCasted(ethereal) /*|| e.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal")*/ | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))&& e.NetworkPosition.Distance2D(me) <= 800+aetherrange)|| ((e.NetworkPosition.Distance2D(me) > 800+aetherrange) && (e.Health < (int)(e.DamageTaken(rocket_damage[Rocket.Level - 1], DamageType.Magical, me, false, 0, 0, 0)*spellamplymult*lensmult)))   )
									&& (((veil == null || !veil.CanBeCasted() || e.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff")  | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) && e.NetworkPosition.Distance2D(me) <= 1500+aetherrange)|| (e.NetworkPosition.Distance2D(me) > 1500+aetherrange)    )
									&& (((ethereal == null || (ethereal!=null && !ethereal.CanBeCasted()) || IsCasted(ethereal) /*|| e.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal")*/ | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))&& e.NetworkPosition.Distance2D(me) <= 800+aetherrange) || (e.NetworkPosition.Distance2D(me) > 800+aetherrange)   )
									&& !(e.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
									)
									Rocket.UseAbility();

									
								if (Laser.Level > 0 && Laser.CanBeCasted() 
									&& Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Laser.Name)
									&& !EzkillCheck 
									&& (!OneHitLeft(e) || e.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
									&& magicimune 
									&& e.NetworkPosition.Distance2D(me) <= 650+aetherrange
									&& !(e.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
									)
									Laser.UseAbility(e);
									
								if (shiva != null && shiva.CanBeCasted() 
									&& Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name)
									&& (!veil.CanBeCasted() || e.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff") || veil == null | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name)) 
									&& (ethereal == null || (ethereal!=null && !IsCasted(ethereal) && !ethereal.CanBeCasted()) || e.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal") | !Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)) 
									&& !EzkillCheck 
									&& (!OneHitLeft(e) || e.NetworkPosition.Distance2D(me) > me.GetAttackRange()+50)
									&& magicimune
									&& e.NetworkPosition.Distance2D(me) <= 900
									&& !(e.Modifiers.Any(y => y.Name == "modifier_teleporting") && IsEulhexFind())
									)
									shiva.UseAbility();

								if (!me.IsChanneling() 
									&& me.CanAttack() 
									&& !e.IsAttackImmune() 
									&& !me.Spellbook.Spells.Any(x => x.IsInAbilityPhase)
									&& OneHitLeft(e) 
									&& e.NetworkPosition.Distance2D(me) <= me.GetAttackRange()+50
									)
									me.Attack(e);
									
									
									
								Utils.Sleep(150, "AUTOCOMBO");

							}
						}
			
				}				
			}
				
			
		}		
		
		
        public static void DrawRanges(EventArgs args)
        {
			if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame || !Utils.SleepCheck("VisibilitySleep"))
				return;
			//Utils.Sleep(150, "VisibilitySleep");
				
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
		
			aether = me.FindItem("item_aether_lens");
			
			if (aether == null)
				aetherrange = 0;
			else
				aetherrange = 200;
		
			

			if (Menu.Item("Show Direction").GetValue<bool>())
			{
				/*
				ParticleEffect effect3;
						
				if (me.IsChanneling() && !Prediction.IsTurning(me))
				{
					if (VisibleUnit3.TryGetValue(me, out effect3)) return;
					effect3 = me.AddParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf");
					effect3.SetControlPoint(1, me.Position);
					effect3.SetControlPoint(2, FindVector(me.Position, me.Rotation, 1200+aetherrange));
					VisibleUnit3.Add(me, effect3);
				}
				else if (!me.IsChanneling())
				{
					if (!VisibleUnit3.TryGetValue(me, out effect3)) return;
					effect3.Dispose();
					VisibleUnit3.Remove(me);
				}*/
				
				
				if (me.IsChanneling())// && !Prediction.IsTurning(me))
				{
					if (effect3 == null)
					{
						effect3 = new ParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf", me);     
						effect3.SetControlPoint(1, me.Position);
						effect3.SetControlPoint(2, FindVector(me.Position, me.Rotation, 1200+aetherrange));
					}
					else 
					{
						effect3.SetControlPoint(1, me.Position);
						effect3.SetControlPoint(2, FindVector(me.Position, me.Rotation, 1200+aetherrange));
					} 
				}
				else if (effect3 != null)
				{
				   effect3.Dispose();
				   effect3 = null;
				}  
				
			}
			

			
			
			if (Menu.Item("Show Target Effect").GetValue<bool>())
			{
				if (target != null && target.IsValid && !target.IsIllusion && target.IsAlive && target.IsVisible && me.Distance2D(target.Position) < 2000)
				{
					if (effect4 == null)
					{
						effect4 = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target);     
						effect4.SetControlPoint(2, me.Position);
						effect4.SetControlPoint(6, new Vector3(1, 0, 0));
						effect4.SetControlPoint(7, target.Position);
					}
					else 
					{
						effect4.SetControlPoint(2, me.Position);
						effect4.SetControlPoint(6, new Vector3(1, 0, 0));
						effect4.SetControlPoint(7, target.Position);
					} 
				}
				else if (effect4 != null)
				{
				   effect4.Dispose();
				   effect4 = null;
				}  
			}
			
			/*
			{
				if (linedisplay == null)
				{
					linedisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf");
					linedisplay.SetControlPoint(1, me.Position);
					linedisplay.SetControlPoint(2, FindVector(me.Position, me.Rotation, 1200+aetherrange));
				}
				if (!me.IsChanneling() || Prediction.IsTurning(me)) 
				{
					linedisplay.Dispose();
					linedisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf");
					linedisplay.SetControlPoint(1, me.Position);
					linedisplay.SetControlPoint(2, FindVector(me.Position, me.Rotation, 1200+aetherrange));
				}
			}
			else if (linedisplay!=null)
				{
				linedisplay.Dispose();
				linedisplay = null;
				}*/
			

			
			
			if (Menu.Item("Blink Range").GetValue<bool>())
			{
				if (me.FindItem("item_blink")!=null)
				{	
					if(rangedisplay_dagger == null)
					{
						rangedisplay_dagger = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
						range_dagger = 1200 + 130 + aetherrange;
						rangedisplay_dagger.SetControlPoint(1, new Vector3(0, 255, 255));
						rangedisplay_dagger.SetControlPoint(2, new Vector3(range_dagger, 255, 0));
					}
					if (range_dagger != 1200 + 130 + aetherrange)
					{
						range_dagger = 1200 + 130 + aetherrange;
						if(rangedisplay_dagger != null)
							rangedisplay_dagger.Dispose();
						rangedisplay_dagger = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
						rangedisplay_dagger.SetControlPoint(1, new Vector3(0, 255, 255));
						rangedisplay_dagger.SetControlPoint(2, new Vector3(range_dagger, 255, 0));
					}
				}
				
                else
				{
					if(rangedisplay_dagger != null)
						rangedisplay_dagger.Dispose();
					rangedisplay_dagger = null;
				}

			}
			else if (rangedisplay_dagger!=null)
				{
				rangedisplay_dagger.Dispose();
				rangedisplay_dagger = null;
				}
			
			
			
			if (Menu.Item("Blink Range Incoming TP").GetValue<bool>())
			{
				if (me.FindItem("item_blink")!=null )
				{	

					var units = ObjectMgr.GetEntities<Unit>().Where
					(x =>
					(x is Hero && x.Team == me.Team)
					||(x is Creep && x.Team == me.Team)
					|| (x is Building && x.Team == me.Team)
					|| (!(x is Hero) && !(x is Building) && !(x is Creep) 
						&& x.ClassID != ClassID.CDOTA_NPC_TechiesMines && x.ClassID != ClassID.CDOTA_NPC_Observer_Ward
						&& x.ClassID != ClassID.CDOTA_NPC_Observer_Ward_TrueSight && x.Team == me.Team)
					).ToList();
					
					foreach (var unit in units)
						{
						HandleEffectR(unit);
						HandleEffectD(unit);
						}

				}
			}
			
			
			

			
			
			
			
			
			if (Menu.Item("Rocket Range").GetValue<bool>())
			{
				if(rangedisplay_rocket == null)
				{
				rangedisplay_rocket = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
				range_rocket = 2800;
				rangedisplay_rocket.SetControlPoint(1, new Vector3(255, 255, 0));
				rangedisplay_rocket.SetControlPoint(2, new Vector3(range_rocket, 255, 0));
				}
			}
			else if (rangedisplay_rocket!=null)
				{
				rangedisplay_rocket.Dispose();
				rangedisplay_rocket = null;
				}
			
		}
		
		
		
        private static void HandleEffectR(Unit unit)
        {
            if (unit == null) return;
            ParticleEffect effect;
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
			
            if (unit.Modifiers.Any(y => y.Name == "modifier_boots_of_travel_incoming") && me.HasModifier("modifier_teleporting"))
            {
                if (VisibleUnit.TryGetValue(unit, out effect)) return;
                effect = unit.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
				range_dagger = 1200 + 130 + aetherrange;
				effect.SetControlPoint(1, new Vector3(0, 255, 255));
				effect.SetControlPoint(2, new Vector3(range_dagger, 255, 0));
                VisibleUnit.Add(unit, effect);
            }
            else
            {
                if (!VisibleUnit.TryGetValue(unit, out effect)) return;
                effect.Dispose();
                VisibleUnit.Remove(unit);
            }
        }
		
        private static void HandleEffectD(Unit unit)
        {
			/*
            if (unit == null) return;
            //ParticleEffect effect2;

            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
			var upos = unit.Position;
					
            if (unit.Modifiers.Any(y => y.Name == "modifier_boots_of_travel_incoming") && me.HasModifier("modifier_teleporting"))// && !Prediction.IsTurning(me))
            {
                if (VisibleUnit2.TryGetValue(unit, out effect2)) return;
                //effect2 = unit.AddParticleEffect(@"particles\ui_mouseactions\range_finder_d_glow.vpcf");
				effect2 = unit.AddParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf");

				effect2.SetControlPoint(1, upos);
				effect2.SetControlPoint(2, FindVector(upos, me.Rotation, 1200+aetherrange));
                VisibleUnit2.Add(unit, effect2);
            }
            else if (!unit.Modifiers.Any(y => y.Name == "modifier_boots_of_travel_incoming") || !me.HasModifier("modifier_teleporting") || !me.IsChanneling())// || Prediction.IsTurning(me))
            {
                if (!VisibleUnit2.TryGetValue(unit, out effect2)) return;
                effect2.Dispose();
                VisibleUnit2.Remove(unit);
            }
			*/
			
            if (unit == null) return;
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
			
			
            if (unit != null && unit.IsValid && unit.IsAlive && unit.Modifiers.Any(y => y.Name == "modifier_boots_of_travel_incoming") && me.HasModifier("modifier_teleporting"))// && !Prediction.IsTurning(me))
			{
				if (effect2 == null)
				{
					effect2 = new ParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf", unit);     
					effect2.SetControlPoint(1, unit.Position);
					effect2.SetControlPoint(2, FindVector(unit.Position, me.Rotation, 1200+aetherrange));
				}
				else 
				{
					effect2.SetControlPoint(1, unit.Position);
					effect2.SetControlPoint(2, FindVector(unit.Position, me.Rotation, 1200+aetherrange));
				} 
			}
			//else if (effect2 != null)
			if (!me.HasModifier("modifier_teleporting") && effect2 != null)
			{
			   effect2.Dispose();
			   effect2 = null;
			}
			
				
        }

		
		

		
		
       public static Vector3 FindVector(Vector3 first, double ret, float distance)
        {
            var retVector = new Vector3(first.X + (float) Math.Cos(Utils.DegreeToRadian(ret)) * distance,
                first.Y + (float) Math.Sin(Utils.DegreeToRadian(ret)) * distance, 100);

            return retVector;
        }		
		
		
		
		
		


        static void FindItems()
        {
            //Skils
            Laser = me.Spellbook.SpellQ;
            Rocket = me.Spellbook.SpellW;
            Refresh = me.Spellbook.SpellR;
            March = me.Spellbook.SpellE;
            //Items
            blink = me.FindItem("item_blink");
            dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
            sheep = me.FindItem("item_sheepstick");
            soulring = me.FindItem("item_soul_ring");
            ethereal = me.FindItem("item_ethereal_blade");
            shiva = me.FindItem("item_shivas_guard");
            ghost = me.FindItem("item_ghost");
            cyclone = me.FindItem("item_cyclone");
            forcestaff = me.FindItem("item_force_staff");
            glimmer = me.FindItem("item_glimmer_cape");
            bottle = me.FindItem("item_bottle");
            veil = me.FindItem("item_veil_of_discord");
            travel = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_travel_boots"));
        }
        static Vector2 HeroPositionOnScreen(Hero x)
        {
            Vector2 PicPosition;
            PicPosition = new Vector2(HUDInfo.GetHPbarPosition(x).X - 1, HUDInfo.GetHPbarPosition(x).Y - 40);
            return PicPosition;
        }


        static bool Ready_for_refresh()
        {
            if ((ghost != null && ghost.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ghost.Name))
                || (soulring != null && soulring.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name))
                || (sheep != null && sheep.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(sheep.Name))
                || (Laser != null && Laser.CanBeCasted() && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Laser.Name))
                || (ethereal != null && ethereal.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
                || (dagon != null && dagon.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
                || (Rocket != null && Rocket.CanBeCasted() && Menu.Item("Skills: ").GetValue<AbilityToggler>().IsEnabled(Rocket.Name))
                || (shiva != null && shiva.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name))
                || (glimmer != null && glimmer.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(glimmer.Name)))
                return false;
            else
                return true;
        }
		
		

        static bool CanReflectDamage(Hero x)
        {
            if (x.Modifiers.Any(m => (m.Name == "modifier_item_blade_mail_reflect" ) || (m.Name == "modifier_item_lotus_orb_active")))
                return true;
            else
                return false;
        }
		
		static bool IsEulhexFind()
        {
            if ((me.FindItem("item_cyclone") != null && me.FindItem("item_cyclone").CanBeCasted()) || (me.FindItem("item_sheepstick") != null && me.FindItem("item_sheepstick").CanBeCasted()) )  
			  return true;
            else
              return false;
        } 	

        static bool IsLinkensProtected(Hero x)
        {
            if (x.Modifiers.Any(m => m.Name == "modifier_item_sphere_target") || x.FindItem("item_sphere") != null && x.FindItem("item_sphere").Cooldown <= 0)
                return true;
            else
                return false;
        } 
		
		
        private static bool IsCasted(Ability ability)
        {
            return ability.Level > 0 && ability.CooldownLength > 0 && Math.Ceiling(ability.CooldownLength).Equals(Math.Ceiling(ability.Cooldown));
        }
		
		

		
		static bool EZkill(Hero en)
        {
            if (en != null && en.IsAlive && en.IsValid)
            {
                int[] dagondamage = new int[5] { 400, 500, 600, 700, 800 };			
                int alletherealdmg = 0, alldagondmg = 0, alllaserdmg = 0, allrocketdmg = 0, allshivadmg = 0, allphysdmg = 0;
                int etherealdamage = (int)(((me.TotalIntelligence * 2) + 75));

					
				spellamplymult = 1 + (me.TotalIntelligence/16/100);

				
                if (((ethereal != null && ethereal.CanBeCasted()) || (ethereal != null && IsCasted(ethereal))) && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)&& !en.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal"))
					etherealmult = 1.4;
				else
					etherealmult = 1;
                if (veil != null && veil.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name) && !en.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff"))
					veilmult = 1.25;
				else
					veilmult = 1;
				if (me.FindItem("item_aether_lens")!=null)
					lensmult = 1.05;
				else
					lensmult = 1;
					
				

                allmult = etherealmult * veilmult * lensmult * spellamplymult;

                if (dagon != null && dagon.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
                    alldagondmg = (int)(en.DamageTaken(dagondamage[dagon.Level - 1], DamageType.Magical, me, false, 0, 0, 0)*allmult);
                else 
					alldagondmg = 0;
				
				if (((ethereal != null && ethereal.CanBeCasted()) || (ethereal != null && IsCasted(ethereal)))  && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
                    alletherealdmg = (int)(en.DamageTaken(etherealdamage, DamageType.Magical, me, false, 0, 0, 0) *allmult);
				else
					alletherealdmg = 0;
					
					
				if (Laser!=null && Laser.Level>0 && Laser.CanBeCasted())
					alllaserdmg = (int)(en.DamageTaken((int)(laser_damage[Laser.Level - 1]), DamageType.Pure, me, false, 0, 0, 0)* lensmult * spellamplymult);
				else
					alllaserdmg = 0;
				
                if ((Rocket != null && Rocket.Level>0 && Rocket.CanBeCasted()) || (Rocket != null && Rocket.Level>0 && IsCasted(Rocket)))
					allrocketdmg = (int)(en.DamageTaken((int)(rocket_damage[Rocket.Level - 1]), DamageType.Magical, me, false, 0, 0, 0)* allmult);
				else
					allrocketdmg = 0;
					
				if (((shiva != null && shiva.CanBeCasted()) || (shiva != null && IsCasted(shiva)))  && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name))
                    allshivadmg = (int)(en.DamageTaken(200, DamageType.Magical, me, false, 0, 0, 0) *allmult);
                else 
					allshivadmg = 0;		

				if (me.CanAttack() && !en.IsAttackImmune())
					allphysdmg = (int)(en.DamageTaken(me.BonusDamage + me.DamageAverage, DamageType.Physical, me));
				else
					allphysdmg = 0;
					
				//factdamage = ((me.Distance2D(en)<650+aetherrange)? alllaserdmg : 0 )+ ((me.Distance2D(en)<2500)? allrocketdmg : 0) + ((me.Distance2D(en)<800+aetherrange)? (alletherealdmg + alldagondmg): 0);  //factical damage in current range
				procastdamage = alldagondmg + alletherealdmg + allrocketdmg + alllaserdmg + allshivadmg + allphysdmg;
				alldamage = alldagondmg + alletherealdmg ;
                if (en.Health < alldamage)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
		
		
		
		
		static int manaprocast()
        {
            int manalaser = 0, manarocket = 0, manarearm = 0, manadagon = 0, manaveil = 0, manasheep = 0, manaethereal = 0, manashiva = 0, manasoulring = 0;
			int manacounter = 0; 
				
			if (Laser!=null && Laser.Level>0)
				manalaser = (int)(laser_mana[Laser.Level - 1]);
			else
				manalaser = 0;
			
			if (Rocket != null && Rocket.Level>0)
				manarocket = (int)(rocket_mana[Rocket.Level - 1]);
			else
				manarocket = 0;
				
			if (Refresh != null && Refresh.Level>0)
				manarearm = (int)(rearm_mana[Refresh.Level - 1]);
			else
				manarearm = 0;
				
			if (dagon != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
				manadagon = 180;
			else
				manadagon = 0;		
				
			if (ethereal != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
				manaethereal = 100;
			else
				manaethereal = 0;
				
			if (veil != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name))
				manaveil = 50;
			else
				manaveil = 0;
				
			if (sheep != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(sheep.Name))
				manasheep = 100;
			else
				manasheep = 0;
				
			if (shiva != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name))
				manashiva = 100;
			else
				manashiva = 0;

			if (soulring != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(soulring.Name))
				manasoulring = 150;
			else
				manasoulring = 0;

				
			manacounter = manalaser+manarocket+manadagon+manaethereal+manaveil+manasheep+manashiva-manasoulring;			
			return manacounter;
              
        }		
		
		static int averagedamage()
        {
      
			int[] dagondamage1 = new int[5] { 400, 500, 600, 700, 800 };			
			int etherealdamage1 = (int)(((me.TotalIntelligence * 2) + 75));
            int alletherealdmg1 = 0, alldagondmg1 = 0, alllaserdmg1 = 0, allrocketdmg1 = 0, allshivadmg1 = 0;
			int averagedamage1 = 0; 
			double etherealmult1 = 1, veilmult1 = 1, lensmult1 = 1, spellamplymult1 = 1;
				
			spellamplymult1 = 1 + (me.TotalIntelligence/16/100);

			if (ethereal != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
				etherealmult1 = 1.4;
			else
				etherealmult1 = 1;
			if (veil != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name))
				veilmult1 = 1.25;
			else
				veilmult1 = 1;
			if (me.FindItem("item_aether_lens")!=null)
				lensmult1 = 1.05;
			else
				lensmult1 = 1;
				
			var allmultavg = etherealmult1 * veilmult1 * lensmult1 * spellamplymult1;
			
			if (dagon != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
				alldagondmg1 = (int)(dagondamage1[dagon.Level - 1]*0.75*allmultavg);
			else 
				alldagondmg1 = 0;
			
			if (ethereal != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
				alletherealdmg1 = (int)(etherealdamage1*0.75*allmultavg);
			else
				alletherealdmg1 = 0;
				
			if (shiva != null && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name))
				allshivadmg1 = (int)(200*0.75*allmultavg);
			else
				allshivadmg1 = 0;
					
				
			if (Laser!=null && Laser.Level>0)
				alllaserdmg1 = (int)(laser_damage[Laser.Level - 1]*lensmult1*spellamplymult1);
			else
				alllaserdmg1 = 0;
			
			if (Rocket != null && Rocket.Level>0)
				allrocketdmg1 = (int)(rocket_damage[Rocket.Level - 1]*0.75*allmultavg);
			else
				allrocketdmg1 = 0;
			
			
			averagedamage1 = alletherealdmg1+alldagondmg1+allshivadmg1+allrocketdmg1+alllaserdmg1;			
			
			return averagedamage1;
              
        }	 

		static int factdamage(Hero en)
        {
            if (en != null && en.IsAlive && en.IsValid)
            {
                int[] dagondamage = new int[5] { 400, 500, 600, 700, 800 };			
                int alletherealdmg = 0, alldagondmg = 0, alllaserdmg = 0, allrocketdmg = 0, allshivadmg = 0, allphysdmg = 0;
                int etherealdamage = (int)(((me.TotalIntelligence * 2) + 75));
				int factdamage1 = 0;

					
				spellamplymult = 1 + (me.TotalIntelligence/16/100);

				

				if (((ethereal != null && ethereal.CanBeCasted()) || (ethereal != null && IsCasted(ethereal))) && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name)&& !en.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_ethereal"))
					etherealmult = 1.4;
				else
					etherealmult = 1;
				if (veil != null && veil.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(veil.Name) && !en.Modifiers.Any(y => y.Name == "modifier_item_veil_of_discord_debuff"))
					veilmult = 1.25;
				else
					veilmult = 1;
				if (me.FindItem("item_aether_lens")!=null)
					lensmult = 1.05;
				else
					lensmult = 1;					
					
                allmult = etherealmult * veilmult * lensmult * spellamplymult;

                if (dagon != null && dagon.CanBeCasted() && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
                    alldagondmg = (int)(en.DamageTaken(dagondamage[dagon.Level - 1], DamageType.Magical, me, false, 0, 0, 0)*allmult);
                else 
					alldagondmg = 0;
				
				if (((ethereal != null && ethereal.CanBeCasted()) || (ethereal != null && IsCasted(ethereal)))  && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
                    alletherealdmg = (int)(en.DamageTaken(etherealdamage, DamageType.Magical, me, false, 0, 0, 0) *allmult);
				else
					alletherealdmg = 0;
					
				if (((shiva != null && shiva.CanBeCasted()) || (shiva != null && IsCasted(shiva)))  && Menu.Item("Items: ").GetValue<AbilityToggler>().IsEnabled(shiva.Name))
                    allshivadmg = (int)(en.DamageTaken(200, DamageType.Magical, me, false, 0, 0, 0) *allmult);
				else
					allshivadmg = 0;					
					
					
				if (Laser!=null && Laser.Level>0 && Laser.CanBeCasted())
					alllaserdmg = (int)(en.DamageTaken((int)(laser_damage[Laser.Level - 1]), DamageType.Pure, me, false, 0, 0, 0)* lensmult * spellamplymult);
				else
					alllaserdmg = 0;
				
                if ((Rocket != null && Rocket.Level>0 && Rocket.CanBeCasted()) || (Rocket != null && Rocket.Level>0 && IsCasted(Rocket)))
					if (me.Distance2D(en) < 800 + aetherrange)
						allrocketdmg = (int)(en.DamageTaken((int)(rocket_damage[Rocket.Level - 1]), DamageType.Magical, me, false, 0, 0, 0)* allmult);
                    else if (me.Distance2D(en) >= 800 + aetherrange && me.Distance2D(en) < 1500 + aetherrange)
						allrocketdmg = (int)(en.DamageTaken((int)(rocket_damage[Rocket.Level - 1]), DamageType.Magical, me, false, 0, 0, 0)* veilmult * lensmult * spellamplymult);
                    else if (me.Distance2D(en) >= 1500 + aetherrange && me.Distance2D(en) < 2500)
						allrocketdmg = (int)(en.DamageTaken((int)(rocket_damage[Rocket.Level - 1]), DamageType.Magical, me, false, 0, 0, 0)* lensmult * spellamplymult);
				else
					allrocketdmg = 0;
					
				if (me.CanAttack() && !en.IsAttackImmune() && me.Distance2D(en)<me.GetAttackRange()+50)
					allphysdmg = (int)(en.DamageTaken(me.BonusDamage + me.DamageAverage, DamageType.Physical, me));
				else
					allphysdmg = 0;
					
				factdamage1 = ((me.Distance2D(en)<650+aetherrange)? alllaserdmg : 0 )+ ((me.Distance2D(en)<2500)? allrocketdmg : 0) + ((me.Distance2D(en)<800+aetherrange)? (alletherealdmg + alldagondmg): 0) + ((me.Distance2D(en)<900)? allshivadmg : 0) + allphysdmg;  //factical damage in current range
                return factdamage1;
              
            }
            else
                return 0;
        }


		
		static int HitCount(Hero en)
		{
			var cleardmg = me.BonusDamage + me.DamageAverage;
			var hitDmg = en.DamageTaken(cleardmg, DamageType.Physical, me);
			return ((int)Math.Ceiling((en.Health - procastdamage)/hitDmg));
		}
		
		static bool OneHitLeft(Hero en)
		{
			var cleardmg = me.BonusDamage + me.DamageAverage;
			var hitDmg = en.DamageTaken(cleardmg, DamageType.Physical, me);
			return (Math.Ceiling(en.Health/hitDmg)<=1);
		}
		
		
        static void Information(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
            if (me.ClassID != ClassID.CDOTA_Unit_Hero_Tinker)
                return;
				
            var targetInf = me.ClosestToMouseTarget(2000);
            FindItems();
            if (targetInf != null && targetInf.IsValid && !targetInf.IsIllusion && targetInf.IsAlive && targetInf.IsVisible)
            {
				if (Menu.Item("TargetCalculator").GetValue<bool>())
				{	
					var start = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(0, HUDInfo.GetHpBarSizeY(targetInf) - 50);
					var starts = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(1, HUDInfo.GetHpBarSizeY(targetInf) - 49);
					var start2 = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(0, HUDInfo.GetHpBarSizeY(targetInf) - 70);
					var start2s = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(1, HUDInfo.GetHpBarSizeY(targetInf) - 69);
					var start3 = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(0, HUDInfo.GetHpBarSizeY(targetInf) - 90);
					var start3s = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(1, HUDInfo.GetHpBarSizeY(targetInf) - 89);
					Drawing.DrawText(EZkill(targetInf) ? alldamage.ToString()+" ez" : alldamage.ToString(), starts, new Vector2(21, 21), Color.Black, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
					Drawing.DrawText(EZkill(targetInf) ? alldamage.ToString()+" ez" : alldamage.ToString(), start, new Vector2(21, 21), EZkill(targetInf) ? Color.Lime : Color.Red, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
					Drawing.DrawText(procastdamage.ToString(), start2s, new Vector2(21, 21), Color.Black, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
					Drawing.DrawText(procastdamage.ToString(), start2, new Vector2(21, 21), (targetInf.Health < procastdamage) ? Color.Lime : Color.Red, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
					Drawing.DrawText(factdamage(targetInf).ToString(), start3s, new Vector2(21, 21), Color.Black, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
					Drawing.DrawText(factdamage(targetInf).ToString(), start3, new Vector2(21, 21), (targetInf.Health < factdamage(targetInf)) ? Color.Lime : Color.Red, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
				}
				if (Menu.Item("HitCounter").GetValue<bool>())
				{	
					var hitcounter = HitCount(targetInf);
					var starthit = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(107, HUDInfo.GetHpBarSizeY(targetInf) - 13);
					var starthits = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(108, HUDInfo.GetHpBarSizeY(targetInf) - 12);
					Drawing.DrawText(hitcounter.ToString()+" hits", starthits, new Vector2(21, 21), Color.Black, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
					Drawing.DrawText(hitcounter.ToString()+" hits", starthit, new Vector2(21, 21), (hitcounter<=1)?Color.Lime:Color.White, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
				}
				if (Menu.Item("RocketCounter").GetValue<bool>() && Rocket.Level>0)
				{	
					var rocketDmg = targetInf.DamageTaken((int)(rocket_damage[Rocket.Level - 1]), DamageType.Magical, me, false, 0, 0, 0);
					var rocketcounter = Math.Ceiling((targetInf.Health - procastdamage)/rocketDmg);
					var startrocket = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(107, HUDInfo.GetHpBarSizeY(targetInf) + 6);
					var startrockets = HUDInfo.GetHPbarPosition(targetInf) + new Vector2(108, HUDInfo.GetHpBarSizeY(targetInf) + 7);
                    Drawing.DrawText(rocketcounter.ToString() + " rckts", startrockets, new Vector2(21, 21), Color.Black, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
                    Drawing.DrawText(rocketcounter.ToString() + " rckts", startrocket, new Vector2(21, 21), (rocketcounter<=1)?Color.Lime:Color.Yellow, FontFlags.AntiAlias | FontFlags.Additive | FontFlags.DropShadow);
				}
				
			}  
			
			if (Menu.Item("Calculator").GetValue<bool>())
			{
				var coordX = Menu.Item("BarPosX").GetValue<Slider>().Value;
				var coordY = Menu.Item("BarPosY").GetValue<Slider>().Value;
				
				Drawing.DrawText("x1", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -240 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText("x1", new Vector2(HUDInfo.ScreenSizeX() / 2-240 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + coordY), new Vector2(30, 200), Color.White, FontFlags.AntiAlias);			
				Drawing.DrawText("x2", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2-240 + coordX, HUDInfo.ScreenSizeY() / 2 + 285 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText("x2", new Vector2(HUDInfo.ScreenSizeX() / 2-240 + coordX, HUDInfo.ScreenSizeY() / 2 + 285 + coordY), new Vector2(30, 200), Color.White, FontFlags.AntiAlias);			
				Drawing.DrawText("x3", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2-240 + coordX, HUDInfo.ScreenSizeY() / 2 + 310 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText("x3", new Vector2(HUDInfo.ScreenSizeX() / 2-240 + coordX, HUDInfo.ScreenSizeY() / 2 + 310 + coordY), new Vector2(30, 200), Color.White, FontFlags.AntiAlias);			
						

				Drawing.DrawText("dmg", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -200 + coordX, HUDInfo.ScreenSizeY() / 2 + 232 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText("dmg", new Vector2(HUDInfo.ScreenSizeX() / 2-200 + coordX, HUDInfo.ScreenSizeY() / 2 + 232 + coordY), new Vector2(30, 200), Color.White, FontFlags.AntiAlias);
				
				Drawing.DrawText(averagedamage().ToString(), new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -200 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText(averagedamage().ToString(), new Vector2(HUDInfo.ScreenSizeX() / 2-200 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + coordY), new Vector2(30, 200), Color.LimeGreen, FontFlags.AntiAlias);			
				Drawing.DrawText((2*averagedamage()).ToString(), new Vector2(HUDInfo.ScreenSizeX() / 2 + 2-200 + coordX, HUDInfo.ScreenSizeY() / 2 + 285 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText((2*averagedamage()).ToString(), new Vector2(HUDInfo.ScreenSizeX() / 2-200 + coordX, HUDInfo.ScreenSizeY() / 2 + 285 + coordY), new Vector2(30, 200), Color.LimeGreen, FontFlags.AntiAlias);			
				Drawing.DrawText((3*averagedamage()).ToString(), new Vector2(HUDInfo.ScreenSizeX() / 2 + 2-200 + coordX, HUDInfo.ScreenSizeY() / 2 + 310 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText((3*averagedamage()).ToString(), new Vector2(HUDInfo.ScreenSizeX() / 2-200 + coordX, HUDInfo.ScreenSizeY() / 2 + 310 + coordY), new Vector2(30, 200), Color.LimeGreen, FontFlags.AntiAlias);			
							
							
				Drawing.DrawText("mana", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 232 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
				Drawing.DrawText("mana", new Vector2(HUDInfo.ScreenSizeX() / 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 232 + coordY), new Vector2(30, 200), Color.White, FontFlags.AntiAlias);			
				if (Refresh != null && Refresh.Level>0)
				{
					Drawing.DrawText(manaprocast().ToString()+" ("+(-manaprocast()+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
					Drawing.DrawText(manaprocast().ToString()+" ("+(-manaprocast()+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + coordY), new Vector2(30, 200),(me.Mana>manaprocast())? Color.LimeGreen : Color.Red, FontFlags.AntiAlias);			
					Drawing.DrawText((2*manaprocast()+rearm_mana[Refresh.Level - 1]).ToString()+" ("+(-(2*manaprocast()+rearm_mana[Refresh.Level - 1])+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 285 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
					Drawing.DrawText((2*manaprocast()+rearm_mana[Refresh.Level - 1]).ToString()+" ("+(-(2*manaprocast()+rearm_mana[Refresh.Level - 1])+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 -120 + coordX , HUDInfo.ScreenSizeY() / 2 + 285 + coordY), new Vector2(30, 200), (me.Mana>(2*manaprocast()+rearm_mana[Refresh.Level - 1]))? Color.LimeGreen : Color.Red, FontFlags.AntiAlias);			
					Drawing.DrawText((3*manaprocast()+2*rearm_mana[Refresh.Level - 1]).ToString()+" ("+(-(3*manaprocast()+2*rearm_mana[Refresh.Level - 1])+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 310 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
					Drawing.DrawText((3*manaprocast()+2*rearm_mana[Refresh.Level - 1]).ToString()+" ("+(-(3*manaprocast()+2*rearm_mana[Refresh.Level - 1])+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 310 + coordY), new Vector2(30, 200), (me.Mana>(3*manaprocast()+2*rearm_mana[Refresh.Level - 1]))? Color.LimeGreen : Color.Red, FontFlags.AntiAlias);			
				}
				else
				{
					Drawing.DrawText(manaprocast().ToString()+" ("+(-manaprocast()+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 + 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + 2 + coordY), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
					Drawing.DrawText(manaprocast().ToString()+" ("+(-manaprocast()+(int)me.Mana).ToString()+")", new Vector2(HUDInfo.ScreenSizeX() / 2 -120 + coordX, HUDInfo.ScreenSizeY() / 2 + 260 + coordY), new Vector2(30, 200), (me.Mana>manaprocast())? Color.LimeGreen : Color.Red, FontFlags.AntiAlias);			
				}
			}
			
        
		}
		
		
    }
}
