using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;


using SharpDX;

namespace CourierOwner
{
    internal static class Program
    {
        private static readonly Menu Menu = new Menu("Courier Owner Air13","cb",true);
		
        private static bool following = false;

        private static bool _loaded;
        private static Unit _fountain;

        private static void Main()
        {
			var avoidenemy = new Menu("AvoidEnemy", "AvoidEnemy");
			avoidenemy.AddItem(new MenuItem("AvoidEnemy.AvoidEnemy1", "Enable Avoid Enemy").SetValue(true).SetTooltip("Courier will use burst/haste if enemy in range"));
            avoidenemy.AddItem(new MenuItem("AvoidEnemy.Range", "Range").SetValue(new Slider(700, 100, 1000)));

		
            Menu.AddItem(new MenuItem("Abuse", "Bottle abuse").SetValue(new KeyBind('U', KeyBindType.Toggle, false)).SetTooltip("Courier pick items and abuse bottle for you (antireus indeed)"));
            Menu.AddItem(new MenuItem("Forced", "AntiReuse deliver").SetValue(new KeyBind('Y', KeyBindType.Toggle, false)).SetTooltip("Courier deliver items to you (antireus indeed)"));
            //Menu.AddItem(new MenuItem("Forced", "Forced courier").SetValue(false).SetTooltip("Forced courier to bring items to you"));
            Menu.AddItem(new MenuItem("Lock", "Stay ay fontain").SetValue(false).SetTooltip("Couriers stay at fountain (antireus indeed)"));
            //Menu.AddItem(new MenuItem("AntiReuse", "Anti Reuse").SetValue(false));
            Menu.AddItem(new MenuItem("Cd", "Rate").SetValue(new Slider(50, 10, 300)));
			
			Menu.AddSubMenu(avoidenemy);
            Menu.AddToMainMenu();
			
			Game.OnUpdate += Game_OnUpdate;
			Drawing.OnDraw+=Drawing_OnDraw;

        }
		
		
		
		

		
		
		

        private static void Game_OnUpdate(EventArgs args)
        {

            if (!Utils.SleepCheck("acd.cd"))
                return;
			if (!Utils.SleepCheck("nya"))
                return;

            //if (!Menu.Item("Lock").GetValue<bool>()/* && !Menu.Item("AntiReuse").GetValue<bool>()*/) 
               // return;

            var me = ObjectMgr.LocalHero;
			var couriers = ObjectMgr.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == me.Team);


			
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                _fountain = null;
            }

            if (!Game.IsInGame || me == null || couriers == null)
            {
                _loaded = false;
                return;
            }
			
			
			
			
            if (Game.IsPaused) return;

            if (_fountain == null || !_fountain.IsValid)
            {
                _fountain = ObjectMgr.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
            }
			

			
			//avoid enemy
			
				foreach (var courier in couriers)
				{
					if (Menu.Item("AvoidEnemy.AvoidEnemy1").GetValue<bool>())
					{
						var enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.IsAlive && !x.IsIllusion && x.Team != me.Team).ToList();
						foreach (var enemy in enemies)
						{
						    if (enemy.Distance2D(courier) < (Menu.Item("AvoidEnemy.Range").GetValue<Slider>().Value))
							{
								var burst = courier.Spellbook.SpellR;
								if (courier.IsFlying && burst.CanBeCasted())
								burst.UseAbility();
							}
						}
					}
					Utils.Sleep(200, "nya");
					
				}
			
			
			
			//anti reuse
				foreach (var courier in couriers)
				{
					//Debug.Assert(_fountain != null, "_fountain != null");					
					if (Menu.Item("Forced").GetValue<KeyBind>().Active)

					{
						if (me.Inventory.StashItems.Any())  
							courier.Spellbook.SpellD.UseAbility();
						else 
							{
							courier.Spellbook.SpellF.UseAbility();
							courier.Spellbook.SpellQ.UseAbility(true);
							}
							
					}

					Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "acd.cd");
				}
				
				
				//lock at base
				foreach (var courier in couriers.Where(courier => courier.Distance2D(_fountain) > 900))
				{
					if (Menu.Item("Lock").GetValue<bool>() && !Menu.Item("Forced").GetValue<KeyBind>().Active && !Menu.Item("Abuse").GetValue<KeyBind>().Active) 
						courier.Spellbook.SpellQ.UseAbility();
	
					Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "acd.cd");
				}
				
				
				
				
				//abuse bottle
				foreach (var courier in couriers)
				{
					
					
					if (Menu.Item("Abuse").GetValue<KeyBind>().Active)
					{
						var bottle = me.Inventory.Items.FirstOrDefault(x => x.Name == "item_bottle");
						var courBottle = courier.Inventory.Items.FirstOrDefault(x => x.Name == "item_bottle");
						var distance = me.Distance2D(courier);
						
						if (bottle == null && courBottle == null && courier.Distance2D(_fountain)>900)
							courier.Spellbook.SpellQ.UseAbility();

						if (distance > 200) 
						{
							if (me.Inventory.StashItems.Any()) 
							{
								//if (courier.Modifiers.Any(x => x.Name == "modifier_fountain_aura_buff")) 
								//if (courier.Distance2D(_fountain)<courier.Distance2D(me))
								if (courier.Distance2D(_fountain)<1100)
								{
									courier.Spellbook.SpellD.UseAbility();
									

								} 
								
								if (bottle != null && bottle.CurrentCharges < 3 && courier.Distance2D(_fountain) > courier.Distance2D(me))
								{
									courier.Follow(me);
								}
								if (bottle != null && bottle.CurrentCharges < 3 && courier.Distance2D(_fountain) < courier.Distance2D(me))
								{
									courier.Spellbook.SpellQ.UseAbility();
								}
								
								
								if (courBottle != null && courBottle.CurrentCharges == 0) 
								{
									courier.Spellbook.SpellQ.UseAbility();
								}
								
								//if (courBottle != null && courBottle.CurrentCharges == 3 && !courier.Modifiers.Any(x => x.Name == "modifier_fountain_aura_buff")) 
								if (courBottle != null && courBottle.CurrentCharges == 3 && !(courier.Distance2D(_fountain)<courier.Distance2D(me))) 
								{
									courier.Follow(me);
								}
								
								if (courBottle != null && courBottle.CurrentCharges == 3 && !(courier.Distance2D(_fountain)>courier.Distance2D(me))) 
								{
									courier.Spellbook.SpellQ.UseAbility();
								}
								
								
							}
							else
							{
								if (bottle != null && bottle.CurrentCharges < 3)
								{
									courier.Follow(me);
								}
								if (bottle != null && bottle.CurrentCharges == 3)
								{
									courier.Spellbook.SpellQ.UseAbility();

								}
								if (courBottle != null && courBottle.CurrentCharges == 3) 
								{
									courier.Follow(me);
								}
								if (courBottle != null && courBottle.CurrentCharges == 0) 
								{
									//courier.Spellbook.SpellD.UseAbility();
									courier.Spellbook.SpellQ.UseAbility();
								}
							}
							
						} 
						else if (distance <= 200)
						{
							if (courBottle != null && courBottle.CurrentCharges == 3)
								courier.Spellbook.SpellF.UseAbility();
							else if (bottle.CurrentCharges == 0)
								{
								courier.Spellbook.SpellF.UseAbility();
								me.Stop();
								me.GiveItem(bottle, courier);
								courier.Spellbook.SpellQ.UseAbility();

								
								}					
						} 
						
				
						
						
					Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "acd.cd");
					}
				}
				
				
            
        }
		
		
		private static void Drawing_OnDraw(EventArgs args)
        {
		if (Menu.Item("Forced").GetValue<KeyBind>().Active)
			Drawing.DrawText("ANTIREUSE DELIVER", new Vector2((int) HUDInfo.ScreenSizeX()/2-100,100),new Vector2(26, 26), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
		if (Menu.Item("Abuse").GetValue<KeyBind>().Active)
			Drawing.DrawText("BOTTLE ABUSE", new Vector2((int) HUDInfo.ScreenSizeX()/2-80,130),new Vector2(26, 26), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
		
		
		}
		
		
    }
}
