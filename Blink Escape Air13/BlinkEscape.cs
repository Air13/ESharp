using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ensage.Common;
using SharpDX.Direct3D9;


namespace BlinkEscape
{
    internal static class Program
    {
        private static readonly Menu Menu=new Menu("Blink Escape Air13","Blink Escape Air13",true,"item_blink",true);
        private static readonly Menu _ranges = new Menu("Ranges", "Ranges");

		private static double angle;
        private static ParticleEffect linedisplay = null, rangedisplay_dagger = null;
        private static int range_dagger = 1200 + 130;



		
        private static void Main()
        {
			

            Menu.AddItem(new MenuItem("Blink Key", "Blink Key").SetValue(new KeyBind('D', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Auto Escape", "Auto Escape")).SetValue(true);
            Menu.AddSubMenu(_ranges);
            _ranges.AddItem(new MenuItem("Blink Range", "Show Blink Dagger Range").SetValue(true));
            _ranges.AddItem(new MenuItem("Show Direction", "Show Direction Vector on Channeling/Astral").SetValue(true));
			
            Menu.AddToMainMenu();
			Game.OnUpdate += ComboEngine;
			Game.OnUpdate += AD;
			Game.OnUpdate += DrawRanges;

            //Drawing.OnDraw += Information;
        }



		
		
		public static void ComboEngine(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            var me = ObjectMgr.LocalHero;

            if (me == null)
                return;

			
            if (Game.IsKeyDown(Menu.Item("Blink Key").GetValue<KeyBind>().Key) && !Game.IsChatOpen && (!me.IsChanneling() || me.Modifiers.Any(y => y.Name == "modifier_puck_phase_shift")))

            {
                if (me.FindItem("item_blink") == null)
					return;
				if (me.FindItem("item_blink") != null && me.FindItem("item_blink").CanBeCasted() && Utils.SleepCheck("blink"))
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
					 
						
					me.FindItem("item_blink").UseAbility(p);	
					Utils.Sleep(100, "blink");
				}
				
				
				
			}
		}
		
		
		
		public static void AD(EventArgs args)
		{
			if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            var me = ObjectMgr.LocalHero;
            if (me == null || me.FindItem("item_blink") == null)
                return;

			var enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.IsVisible && x.IsAlive && x.Team == me.GetEnemyTeam() && !x.IsIllusion);
			if (Menu.Item("Auto Escape").GetValue<bool>() && me.IsAlive && me.IsVisibleToEnemies && Utils.SleepCheck("blinkM"))
			{
				
				foreach (var e in enemies)
				{
					if (e == null)
						return;
                    angle = Math.Abs(e.FindAngleR() - Utils.DegreeToRadian(e.FindAngleForTurnTime(me.NetworkPosition)));

                    if (
						(e.ClassID == ClassID.CDOTA_Unit_Hero_Windrunner && IsCasted(e.Spellbook.SpellQ) && angle <= 0.12  && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 550)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Sven && IsCasted(e.Spellbook.SpellQ) && angle <= 0.3  && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 500)
						//|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Oracle && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						)
					{
						if (me.FindItem("item_blink") != null && me.FindItem("item_blink").Cooldown==0 && me.FindItem("item_blink").CanBeCasted() && Utils.SleepCheck("blink1a"))
						{
							var p1a = new Vector3(
										me.Position.X + 500 * (float)Math.Cos(me.RotationRad),
										me.Position.Y + 500 * (float)Math.Sin(me.RotationRad),
										100);
						
							me.FindItem("item_blink").UseAbility(p1a);
							Game.ExecuteCommand("+dota_camera_follow");
							Game.ExecuteCommand("dota_camera_center");
							//Utils.Sleep(150, "blink1a");
						}
					}
					
					
					
					
                    else if (
						(e.FindItem("item_ethereal_blade")!=null && IsCasted(e.FindItem("item_ethereal_blade")) && angle <= 0.1 && me.Distance2D(e) < e.FindItem("item_ethereal_blade").GetCastRange() + 250)
                        || (e.ClassID == ClassID.CDOTA_Unit_Hero_Sniper && IsCasted(e.Spellbook.SpellR)  && me.Modifiers.Any(y => y.Name == "modifier_sniper_assassinate"))//e.FindSpell("sniper_assassinate").Cooldown > 0 && me.Modifiers.Any(y => y.Name == "modifier_sniper_assassinate"))
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_SkeletonKing && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_ChaosKnight && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_VengefulSpirit && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Chen && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 250)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomAssassin && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_QueenOfPain && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Dazzle && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Viper && IsCasted(e.Spellbook.SpellR) && angle <= 0.1 && me.Distance2D(e) < e.Spellbook.SpellR.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_PhantomLancer && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Morphling && IsCasted(e.Spellbook.SpellW) && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter && IsCasted(e.Spellbook.SpellQ) && angle <= 0.1   && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 150)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Naga_Siren && IsCasted(e.Spellbook.SpellW) && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellW.GetCastRange() + 350)
						
						)
					{
						if (me.FindItem("item_blink") != null && me.FindItem("item_blink").Cooldown==0 && me.FindItem("item_blink").CanBeCasted() && Utils.SleepCheck("blink1"))
						{
							var p1 = new Vector3(
										me.Position.X + 150 * (float)Math.Cos(me.RotationRad),
										me.Position.Y + 150 * (float)Math.Sin(me.RotationRad),
										100);
						
							me.FindItem("item_blink").UseAbility(p1);
							Game.ExecuteCommand("+dota_camera_follow");
							Game.ExecuteCommand("dota_camera_center");
							//Utils.Sleep(150, "blink1");
						}
					}
					
					
					
					else if (
                        (e.ClassID == ClassID.CDOTA_Unit_Hero_Huskar && IsCasted(e.Spellbook.SpellR) && angle <= 0.15 && me.Distance2D(e) < e.Spellbook.SpellQ.GetCastRange() + 250) //( (e.FindSpell("huskar_life_break").Cooldown >= 3 && e.AghanimState()) || (e.FindSpell("huskar_life_break").Cooldown >= 11 && !e.AghanimState())) && me.Distance2D(e) <= 400)
						|| e.ClassID == ClassID.CDOTA_Unit_Hero_Tidehunter && e.Spellbook.SpellR.IsInAbilityPhase && me.Distance2D(e) <= 1200
						|| e.ClassID == ClassID.CDOTA_Unit_Hero_Necrolyte && e.FindSpell("necrolyte_reapers_scythe").IsInAbilityPhase && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellR.GetCastRange() + 200
						|| e.ClassID == ClassID.CDOTA_Unit_Hero_DoomBringer && e.FindSpell("doom_bringer_doom").IsInAbilityPhase && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellR.GetCastRange() + 200
						)
					{
						if (me.FindItem("item_blink") != null && me.FindItem("item_blink").CanBeCasted() && Utils.SleepCheck("blink2"))
						{
							var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
							var a2 = me.Position.ToVector2().FindAngleBetween(e.Position.ToVector2(), true);
							
							safeRange -= (int)me.HullRadius;
							var p2 = new Vector3(
								(me.Position.X - safeRange * (float)Math.Cos(a2)),
								(me.Position.Y - safeRange * (float)Math.Sin(a2)),
								100);
							
							me.FindItem("item_blink").UseAbility(p2);
							Game.ExecuteCommand("+dota_camera_follow");
							Game.ExecuteCommand("dota_camera_center");
							//Utils.Sleep(150, "blink2");
						}

					}



                    else if (
						e.FindItem("item_blink") != null && me.Distance2D(e) <= 600 && e.FindItem("item_blink").Cooldown > 11 && IsCasted(e.FindItem("item_blink"))// && e.FindItem("item_blink").Cooldown > 11
						|| e.ClassID == ClassID.CDOTA_Unit_Hero_Enigma && e.Spellbook.SpellR.IsInAbilityPhase && me.Distance2D(e) <= 700 
						|| e.ClassID == ClassID.CDOTA_Unit_Hero_FacelessVoid && e.FindSpell("faceless_void_chronosphere").IsInAbilityPhase && me.Distance2D(e) <= 1050
						|| e.ClassID == ClassID.CDOTA_Unit_Hero_Magnataur && e.FindSpell("magnataur_reverse_polarity").IsInAbilityPhase && me.Distance2D(e) <= 450
                        || (e.ClassID == ClassID.CDOTA_Unit_Hero_Tusk && angle <= 0.35 && e.Modifiers.Any(y => y.Name == "modifier_tusk_snowball_movement") && me.Distance2D(e) <= 575)
                        || (e.ClassID == ClassID.CDOTA_Unit_Hero_Lich && IsCasted(e.Spellbook.SpellR) && angle <= 0.5  && me.Distance2D(e) < e.Spellbook.SpellR.GetCastRange() + 350)
						|| (e.ClassID == ClassID.CDOTA_Unit_Hero_Life_Stealer && me.Modifiers.Any(y => y.Name == "modifier_life_stealer_open_wounds"))
                        || e.ClassID == ClassID.CDOTA_Unit_Hero_Beastmaster && e.FindSpell("beastmaster_primal_roar").IsInAbilityPhase && angle <= 0.1  && me.Distance2D(e) < e.Spellbook.SpellR.GetCastRange() + 200
						|| e.ClassID == ClassID.CDOTA_Unit_Hero_Pudge && e.Modifiers.Any(y => y.Name == "modifier_pudge_rot")  && me.Distance2D(e) < 300
						)
					{
						if (me.FindItem("item_blink") != null && me.FindItem("item_blink").CanBeCasted() && Utils.SleepCheck("blink3"))
						{
							var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
							safeRange -= (int)me.HullRadius;

							var p13 = new Vector3(
										me.Position.X + safeRange * (float)Math.Cos(me.RotationRad),
										me.Position.Y + safeRange * (float)Math.Sin(me.RotationRad),
										100);
							
							
							me.FindItem("item_blink").UseAbility(p13);
							Game.ExecuteCommand("+dota_camera_follow");
							Game.ExecuteCommand("dota_camera_center");
							//Utils.Sleep(150, "blink3");
						}
					}	
					
					

				} 
				
				//не зависит от наличия врагов на карте
				
				/*
                if (me.FindItem("item_blink") != null && me.FindItem("item_blink").CanBeCasted() && Utils.SleepCheck("blink4") 
						&& !me.IsHexed() 
						&& !me.HasModifier("modifier_rattletrap_hookshot")
						&& !me.Modifiers.Any(y => y.Name == "modifier_doom_bringer_doom") 
						&& !me.Modifiers.Any(y => y.Name == "modifier_legion_commander_duel")
						&& (me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen")
						//|| me.Modifiers.Any(y => y.Name == "modifier_disruptor_static_storm")
						|| me.IsSilenced()
						|| me.Modifiers.Any(y => y.Name == "modifier_life_stealer_open_wounds")
						|| me.Modifiers.Any(y => y.Name == "modifier_oracle_fortunes_end_purge")
						||(me.IsRooted() && !me.HasModifier("modifier_phoenix_sun_ray") && !me.Modifiers.Any(y => y.Name == "modifier_razor_unstablecurrent_slow"))
						//|| me.MovementSpeed <= 180
						|| me.Modifiers.Any(y => y.Name == "modifier_item_rod_of_atos")
						))
					{
						var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
						safeRange -= (int)me.HullRadius;

						var home = ObjectMgr.GetEntities<Entity>().FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain) as Unit;
						var findangle = me.NetworkPosition.ToVector2().FindAngleBetween(home.NetworkPosition.ToVector2(), true);
						var p3 = new Vector3(
							me.Position.X + safeRange * (float) Math.Cos(findangle),
							me.Position.Y + safeRange * (float) Math.Sin(findangle), 
							me.Position.Z);

						me.FindItem("item_blink").UseAbility(p3);
						Game.ExecuteCommand("+dota_camera_follow");
						Game.ExecuteCommand("dota_camera_center");
						//Utils.Sleep(150, "blink4");
					}
				*/
				
				
				
				if ((me.IsSilenced() 
					|| me.Modifiers.Any(y => y.Name == "modifier_item_rod_of_atos")
					//|| me.Modifiers.Any(y => y.Name == "modifier_life_stealer_open_wounds")
					//|| me.Modifiers.Any(y => y.Name == "modifier_oracle_fortunes_end_purge")
					||(me.IsRooted() && !me.HasModifier("modifier_phoenix_sun_ray") && !me.Modifiers.Any(y => y.Name == "modifier_razor_unstablecurrent_slow"))
                    || (me.MovementSpeed <= 180 && me.MovementSpeed >= 110)
					)
					&& !me.HasModifier("modifier_rattletrap_hookshot")
					&& !me.IsHexed() 
					&& !me.Modifiers.Any(y => y.Name == "modifier_doom_bringer_doom") 
					&& !me.Modifiers.Any(y => y.Name == "modifier_legion_commander_duel")
					) 
				{
					if (me.FindItem("item_blink") != null && me.FindItem("item_blink").CanBeCasted() 
						&& !me.FindItem("item_black_king_bar").CanBeCasted() 
						&& Utils.SleepCheck("blink4") 
						&& (me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen") || me.Modifiers.Any(y => y.Name == "modifier_disruptor_static_storm") || me.Modifiers.Any(y => y.Name == "modifier_pudge_rot"))
						)
					{
						var p5 = new Vector3(
									me.Position.X + 1000 * (float)Math.Cos(me.RotationRad),
									me.Position.Y + 1000 * (float)Math.Sin(me.RotationRad),
									100);
					
						me.FindItem("item_blink").UseAbility(p5);
						Game.ExecuteCommand("+dota_camera_follow");
						Game.ExecuteCommand("dota_camera_center");
						//Utils.Sleep(150, "blink4");


					}
					else if (me.FindItem("item_blink") != null && me.FindItem("item_blink").CanBeCasted() 
							&& ((!me.FindItem("item_manta").CanBeCasted() 
							&& !me.FindItem("item_cyclone").CanBeCasted() 
							&& !me.FindItem("item_diffusal_blade").CanBeCasted() 
							&& !me.FindItem("item_diffusal_blade_2").CanBeCasted()
							&& !me.FindItem("item_guardian_greaves").CanBeCasted() ) || me.Modifiers.Any(y => y.Name == "modifier_warlock_upheaval"))
							&& !me.FindItem("item_black_king_bar").CanBeCasted() 
							&& !me.IsMagicImmune()
							&& Utils.SleepCheck("blink4") 
							&& !me.Modifiers.Any(y => y.Name == "modifier_riki_smoke_screen")
							&& !me.Modifiers.Any(y => y.Name == "modifier_disruptor_static_storm")
							&& !me.Modifiers.Any(y => y.Name == "modifier_pudge_rot")
							)
					{
						var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
						safeRange -= (int)me.HullRadius;
						var home = ObjectMgr.GetEntities<Entity>().FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain) as Unit;
						var findangle = me.NetworkPosition.ToVector2().FindAngleBetween(home.NetworkPosition.ToVector2(), true);
						var p3 = new Vector3(
							me.Position.X + safeRange * (float) Math.Cos(findangle),
							me.Position.Y + safeRange * (float) Math.Sin(findangle), 
							me.Position.Z);

						me.FindItem("item_blink").UseAbility(p3);
						Game.ExecuteCommand("+dota_camera_follow");
						Game.ExecuteCommand("dota_camera_center");
						//Utils.Sleep(150, "blink4");
						return;
					}				
				
				}
				
				//spell modifiers
				var units =
					ObjectManager.GetEntities<Unit>()
						.Where(x => x.ClassID == ClassID.CDOTA_BaseNPC && x.Team == me.GetEnemyTeam());

				foreach (var unit in units) 
				{
					foreach (var modifier in unit.Modifiers) 
					{
						switch (modifier.Name) 
						{
							case "modifier_lina_light_strike_array": 
							{
								if (me.Distance2D(unit) <= 250)
								{
									var castPoint = 0.5 - modifier.ElapsedTime;

									if (Blink(castPoint, true, 500, unit)) return;

								}

								break;
							}
							case "modifier_kunkka_torrent_thinker": 
							{
								var elapsedTime = modifier.ElapsedTime;

								if (me.Distance2D(unit) <= 250 && elapsedTime > 1) 
								{
									var castPoint = 1.6 - elapsedTime;

									if (Blink(castPoint, true, 500, unit)) return;

								}

								break;
							}
							case "modifier_leshrac_split_earth_thinker": 
							{
								if (me.Distance2D(unit) <= 250) 
								{
									var castPoint = 0.35 - modifier.ElapsedTime;

									if (Blink(castPoint, true, 500, unit)) return;
								}

								break;
							}
							case "modifier_bloodseeker_bloodbath_thinker": 
							{
								var elapsedTime = modifier.ElapsedTime;

								if (me.Distance2D(unit) <= 500 && elapsedTime > 2) 
								{
									var castPoint = 2.6 - modifier.ElapsedTime;

									if (Blink(castPoint, true, 1100, unit)) return;

								}

								break;
							}
							
						}
					}
				
				}
				Utils.Sleep(150, "blinkM");

				
				
			}
				
		}
		
		
		
        public static void DrawRanges(EventArgs args)
        {
			if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            var me = ObjectMgr.LocalHero;
            if (me == null)
                return;
		
			var aether = me.FindItem("item_aether_lens");
			int herorotation = 0;
            var aetherrange = 0;
			if (aether == null)
				aetherrange = 0;
			else
				aetherrange = 200;
			

			if (Menu.Item("Show Direction").GetValue<bool>())
			{
				if (linedisplay == null)
				{
					linedisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf");
					linedisplay.SetControlPoint(1, me.Position);
					linedisplay.SetControlPoint(2, FindVector(me.Position, me.Rotation, 1200+aetherrange));
				}
				if (!me.IsChanneling() 
					&& !me.Modifiers.Any(y => y.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison")
					&& !me.Modifiers.Any(y => y.Name == "modifier_puck_phase_shift")
					&& !me.Modifiers.Any(y => y.Name == "modifier_eul_cyclone")
					&& !me.Modifiers.Any(y => y.Name == "modifier_shadow_demon_disruption")
					//&& !me.Modifiers.Any(y => y.Name == "modifier_item_forcestaff_active")
					) 
				{
					linedisplay.Dispose();
					linedisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf");
					linedisplay.SetControlPoint(1, me.Position);
					linedisplay.SetControlPoint(2, FindVector(me.Position, me.Rotation, 1200+aetherrange));
				}
			}
			else if (linedisplay!=null)
				linedisplay.Dispose();
			

			
			
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
			
			
			


		
	
			
		}
		
		
		
		
		
		
        private static bool Blink(double castpoint = 5, bool forwardBlink = false, int blinkrange = 500, Unit target = null, Ability key = null) 
		{
			if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return false;
            var me = ObjectMgr.LocalHero;
            if (me == null)
                return false;
				
			if (me.IsMagicImmune())
                return false;
				
			if (Menu.Item("Auto Escape").GetValue<bool>() && me.IsAlive && me.IsVisibleToEnemies)
			{
				castpoint -= 0.05;

				//var blink = me.Inventory.Items.Concat(me.Spellbook.Spells).FirstOrDefault(x => Spells.BlinkAbilities.Any(x.Name.Equals) && x.CanBeCasted());

				var blink = me.FindItem("item_blink");
				
				
				if (blink == null || !blink.CanBeCasted())
					return false;

				var castRange = blink.GetCastRange() - 50;

				//if (!(blink is Item) && !me.CanCast())
				 //   return false;

				var home =
					ObjectManager.GetEntities<Entity>()
						.FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain) as
						Unit;
				var enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.IsVisible && x.IsAlive && x.Team == me.GetEnemyTeam() && !x.IsIllusion);


				if (home == null)
					return false;

				var isLeap = blink.ClassID == ClassID.CDOTA_Item_ForceStaff ||
							 blink.ClassID == ClassID.CDOTA_Ability_Mirana_Leap;
				var isInvul = blink.ClassID == ClassID.CDOTA_Ability_EmberSpirit_Activate_FireRemnant ||
							  blink.ClassID == ClassID.CDOTA_Ability_FacelessVoid_TimeWalk;

				if (isLeap) castRange = 60;
				/*
				var findangle = me.NetworkPosition.ToVector2()
					.FindAngleBetween(home.Position.ToVector2(), true);
				var position = new Vector3(me.Position.X + castRange * (float) Math.Cos(findangle),
					me.Position.Y + castRange * (float) Math.Sin(findangle), me.Position.Z);
				var position = new Vector3(
									me.Position.X + castRange * (float)Math.Cos(me.RotationRad),
									me.Position.Y + castRange * (float)Math.Sin(me.RotationRad),
									me.Position.Z);*/
									

				var safeRange = me.FindItem("item_aether_lens") == null ? 1200 : 1400;
				var a2 = me.Position.ToVector2().FindAngleBetween(target.Position.ToVector2(), true);
				
				safeRange -= (int)me.HullRadius;
				var position = new Vector3(
					(me.Position.X - blinkrange * (float)Math.Cos(a2)),
					(me.Position.Y - blinkrange * (float)Math.Sin(a2)),
					100);
				
				

				if (blink.ClassID == ClassID.CDOTA_Ability_EmberSpirit_Activate_FireRemnant) {
					if (me.HasModifier("modifier_ember_spirit_fire_remnant_timer"))
						castpoint = 0.30;
					else return false;
				}

				if (blink.ClassID == ClassID.CDOTA_Ability_Mirana_Leap)
					castpoint -= me.GetTurnTime(home);

				if (blink.ClassID == ClassID.CDOTA_Item_ForceStaff)
					castpoint -= 0.13;

				if (me.HasModifier("modifier_bloodseeker_rupture") && !isInvul)
					return false;

				var castDelay = blink.GetCastDelay(me, home, true);
				//var enoughTime = castDelay < castpoint;

				
				
				/*
				if (key != null) { //&& enoughTime) {
					if (PhaseCanBeCanceled(castDelay, castpoint, blink))
						return true;
				}
				
				if (//!enoughTime &&
					!forwardBlink || blink.ClassID != ClassID.CDOTA_Item_BlinkDagger) {
					return false;
				}*/

				if (isLeap) {
					if (me.GetTurnTime(home) > 0) {
						me.Move(position);
						if (blink.IsAbilityBehavior(AbilityBehavior.NoTarget))
							blink.UseAbility(true);
						else
							blink.UseAbility(me, true);
					}
					else {
						if (blink.IsAbilityBehavior(AbilityBehavior.NoTarget))
							blink.UseAbility();
						else
							blink.UseAbility(me);
					}
				}
				else {
					if (forwardBlink) //&& !enoughTime || blink.ClassID == ClassID.CDOTA_Ability_EmberSpirit_Activate_FireRemnant)
						position = new Vector3(me.Position.X + blinkrange * (float) Math.Cos(me.RotationRad),
							me.Position.Y + blinkrange * (float) Math.Sin(me.RotationRad),
							me.Position.Z);
					blink.UseAbility(position);
				}

			
				Game.ExecuteCommand("+dota_camera_follow");
				Game.ExecuteCommand("dota_camera_center");

				
				Utils.Sleep(castDelay * 1000 + Program.Menu.Item("delay").GetValue<Slider>().Value, "CounterDelay");
				return true;
			}
			else return false;
			
        }		
		
		
       public static Vector3 FindVector(Vector3 first, double ret, float distance)
        {
            var retVector = new Vector3(first.X + (float) Math.Cos(Utils.DegreeToRadian(ret)) * distance,
                first.Y + (float) Math.Sin(Utils.DegreeToRadian(ret)) * distance, 100);

            return retVector;
        }	
		
		
        private static Vector3 TpPos { get; set; }
		
        private static bool IsCasted(Ability ability)
        {
            return ability.Level > 0 && ability.CooldownLength > 0 && Math.Ceiling(ability.CooldownLength).Equals(Math.Ceiling(ability.Cooldown));
        }

    }
}
