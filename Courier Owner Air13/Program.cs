using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using System.Windows.Input;

using SharpDX;

namespace CourierOwner
{
    internal static class Program
    {
        private static readonly Menu Menu = new Menu("Courier Owner Air13","cb",true);
		
		//private static bool owned = false;
        //private static Key keyOWNED = Key.I;
        //private static Key toggleOWNED = Key.O;


        private static bool _loaded;
        private static Unit _fountain;

        private static void Main()
        {
			var avoidenemy = new Menu("AvoidEnemy", "AvoidEnemy");
			avoidenemy.AddItem(new MenuItem("AvoidEnemy.AvoidEnemy1", "Enable Avoid Enemy").SetValue(true).SetTooltip("Courier will use burst / haste if enemy in range"));
            avoidenemy.AddItem(new MenuItem("AvoidEnemy.Range", "Range").SetValue(new Slider(700, 100, 1000)));

			//Menu.AddItem(new MenuItem("Selection", "Courier selection").SetValue(new KeyBind('I', KeyBindType.Press)));

            Menu.AddItem(new MenuItem("Burst", "Auto burst by BA and AD").SetValue(true).SetTooltip("Enable auto burst while abusing bottle or delivering items to you"));
            Menu.AddItem(new MenuItem("Abuse", "Bottle Abuse").SetValue(new KeyBind('U', KeyBindType.Toggle, false)).SetTooltip("Courier deliver items and abuse bottle for you (antireus indeed)"));
            Menu.AddItem(new MenuItem("Forced", "Anti Reuse deliver").SetValue(new KeyBind('Y', KeyBindType.Toggle, false)).SetTooltip("Courier deliver items to you (antireus indeed)"));
            Menu.AddItem(new MenuItem("Lock", "Lock at fountain").SetValue(new KeyBind('I', KeyBindType.Toggle, false)).SetTooltip("Couriers lock at fountain (antireus indeed)"));
            Menu.AddItem(new MenuItem("Cd", "Rate").SetValue(new Slider(150, 30, 300)));
			
			Menu.AddSubMenu(avoidenemy);
            Menu.AddToMainMenu();
			
			//Game.OnWndProc += Game_OnWndProc;
			Game.OnUpdate += Game_OnUpdate;
			Drawing.OnDraw+=Drawing_OnDraw;

        }
		
		
		/*
		
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
                if (Game.IsKeyDown(keyOWNED))
                    owned = true;
                else
                {
                    owned = false;
                }

                if (Game.IsKeyDown(toggleOWNED) && Utils.SleepCheck("toggle"))
                {
         
                    Utils.Sleep(200, "toggle");
                }



            }
        }*/
		
		
		

        private static void Game_OnUpdate(EventArgs args)
        {

            if (!Utils.SleepCheck("rate"))
                return;


          

            var me = ObjectMgr.LocalHero;
			var couriers = ObjectMgr.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == me.Team);


			
            if (!_loaded)
            {
                if (!Game.IsInGame || me == null || !me.IsAlive)
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
                    Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
					
				}
			
			
				var courierfontain = ClosestToFontain();
				var courierhero = ClosestToMyHero();
				var couriermouse = ClosestToMouse();
                var courierbottle = HavingBottle();

			//anti reuse
				foreach (var courier in couriers)
				{
					//Debug.Assert(_fountain != null, "_fountain != null");					
					if (Menu.Item("Forced").GetValue<KeyBind>().Active && !Menu.Item("Abuse").GetValue<KeyBind>().Active)
					{
                        


                            if (me.Inventory.StashItems.Any())
                            {
                                var burst1 = courierfontain.Spellbook.SpellR;
                                if (Menu.Item("Burst").GetValue<bool>() && courierfontain.IsFlying && burst1.CanBeCasted())
                                    burst1.UseAbility();
                                courierfontain.Spellbook.SpellD.UseAbility();
                                
                            }
                            else if (courier.Inventory.Items.Any())
                            {

                                courier.Spellbook.SpellF.UseAbility();
                                courier.Spellbook.SpellQ.UseAbility(true);
                            }

                            
                            //else if (courier.Inventory.FreeSlots.Any() && courier.Distance2D(_fountain) > 1000)
                          

					}

					Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
				}
				
				
				//lock at base
				foreach (var courier in couriers.Where(courier => courier.Distance2D(_fountain) > 900))
				{
					if (Menu.Item("Lock").GetValue<KeyBind>().Active && !Menu.Item("Forced").GetValue<KeyBind>().Active && !Menu.Item("Abuse").GetValue<KeyBind>().Active) 
						courier.Spellbook.SpellQ.UseAbility();
	
					Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
				}
				
				
				
				
				//abuse bottle
				foreach (var courier in couriers)
				{
					
					
					if (Menu.Item("Abuse").GetValue<KeyBind>().Active)
					{
						var bottle = me.Inventory.Items.FirstOrDefault(x => x.Name == "item_bottle");
						var courBottle = courier.Inventory.Items.FirstOrDefault(x => x.Name == "item_bottle");
                        var courOtherItems = courier.Inventory.Items.FirstOrDefault(x => x.Name != "item_bottle");

						var distance = me.Distance2D(courier);

                        
						
						/*
						if (bottle == null && courBottle == null && me.Inventory.StashItems.FirstOrDefault(x => x.Name == "item_bottle") == null && courier.Distance2D(_fountain)>900)
							{
								courierhero.Spellbook.SpellQ.UseAbility();
							}
						*/
						if (bottle == null && courBottle == null && me.Inventory.StashItems.FirstOrDefault(x => x.Name == "item_bottle") != null)
							{
                                courierhero.Spellbook.SpellD.UseAbility();
							}

						if (distance > 200) 
						{
                            var burst2 = courierhero.Spellbook.SpellR;
                            var burst3 = courierhero.Spellbook.SpellR;

							if (me.Inventory.StashItems.Any()) 
							{
								//if (courier.Modifiers.Any(x => x.Name == "modifier_fountain_aura_buff")) 
								//if (courier.Distance2D(_fountain)<courier.Distance2D(me))
								/*
								if (courier.Distance2D(_fountain)<1000)
								{
                                    courierfontain.Spellbook.SpellD.UseAbility();
									

								}*/
								
								

                                if (bottle != null && bottle.CurrentCharges < 3 && courierhero.Distance2D(_fountain) > courierhero.Distance2D(me))
								{
									courierhero.Follow(me);
                                    if (Menu.Item("Burst").GetValue<bool>() && courierhero.IsFlying && burst2.CanBeCasted())
                                        burst2.UseAbility();
								}
                                if (bottle != null && bottle.CurrentCharges < 3 && courierhero.Distance2D(_fountain) < courierhero.Distance2D(me))
								{
                                    courierhero.Spellbook.SpellQ.UseAbility();
                                    if (Menu.Item("Burst").GetValue<bool>() && courierhero.IsFlying && burst2.CanBeCasted())
                                        burst2.UseAbility();
								}

                                if (bottle != null && bottle.CurrentCharges < 3 && courierhero.Distance2D(_fountain) < 1000)
                                {
                                    courierhero.Spellbook.SpellD.UseAbility();
                                    if (Menu.Item("Burst").GetValue<bool>() && courierhero.IsFlying && burst2.CanBeCasted())
                                        burst2.UseAbility();
                                }
								
								
								if (courBottle != null && courBottle.CurrentCharges == 0) 
								{
									courierbottle.Spellbook.SpellQ.UseAbility();
                                    if (Menu.Item("Burst").GetValue<bool>() && courierbottle.IsFlying && burst3.CanBeCasted())
                                        burst3.UseAbility();
								}

                                if (courBottle != null && courierbottle.Distance2D(_fountain) < 1000)
                                {
                                    courierbottle.Spellbook.SpellD.UseAbility();
                                    if (Menu.Item("Burst").GetValue<bool>() && courierbottle.IsFlying && burst3.CanBeCasted())
                                        burst3.UseAbility();
                                }
								
								//if (courBottle != null && courBottle.CurrentCharges == 3 && !courier.Modifiers.Any(x => x.Name == "modifier_fountain_aura_buff")) 
                                if (courBottle != null && courBottle.CurrentCharges == 3 && courierbottle.Distance2D(_fountain) > courierbottle.Distance2D(me)) 
								{
                                    courierbottle.Follow(me);
                                    if (Menu.Item("Burst").GetValue<bool>() && courierbottle.IsFlying && burst3.CanBeCasted())
                                        burst3.UseAbility();
								}

                                if (courBottle != null && courBottle.CurrentCharges == 3 && courierbottle.Distance2D(_fountain) < courierbottle.Distance2D(me)) 
								{
                                    courierbottle.Spellbook.SpellQ.UseAbility();
                                    if (Menu.Item("Burst").GetValue<bool>() && courierbottle.IsFlying && burst3.CanBeCasted())
                                        burst3.UseAbility();
								}
								
								
							}
							else
							{
								if (bottle != null && bottle.CurrentCharges < 3)
								{
                                    courierhero.Follow(me);
                                    if (Menu.Item("Burst").GetValue<bool>() && courierhero.IsFlying && burst2.CanBeCasted())
                                        burst2.UseAbility();
								}
								if (bottle != null && bottle.CurrentCharges == 3)
								{
									courierbottle.Spellbook.SpellQ.UseAbility();
                                    if (Menu.Item("Burst").GetValue<bool>() && courierbottle.IsFlying && burst3.CanBeCasted())
                                        burst3.UseAbility();

								}
                                if (courBottle != null && courBottle.CurrentCharges == 3) 
								{
                                    courierbottle.Follow(me);
                                    if (Menu.Item("Burst").GetValue<bool>() && courierbottle.IsFlying && burst3.CanBeCasted())
                                        burst3.UseAbility();
								}
								if (courBottle != null && courBottle.CurrentCharges == 0) 
								{
									//courier.Spellbook.SpellD.UseAbility();
                                    courierbottle.Spellbook.SpellQ.UseAbility();
                                    if (Menu.Item("Burst").GetValue<bool>() && courierbottle.IsFlying && burst3.CanBeCasted())
                                        burst3.UseAbility();
								}
							}
							
						} 
						else if (distance <= 200)
						{
                            if (courBottle != null && courBottle.CurrentCharges == 3)
								courierhero.GiveItem(courBottle, me);
                                //courierhero.Spellbook.SpellF.UseAbility();

                            if (bottle != null && bottle.CurrentCharges > 0 && courier.Inventory.Items.Any())
                            {
                                //if (me.Inventory.FreeSlots == null)
                                courierhero.Spellbook.SpellF.UseAbility();
                            }	


							if (bottle != null && bottle.CurrentCharges == 0)
							{
								me.Stop();
								me.GiveItem(bottle, courier);
							}

                            if (bottle == null && courBottle.CurrentCharges == 0 && courOtherItems != null)
                            {
                                    courierhero.GiveItem(courOtherItems, me);                                              
                            }

                            if (bottle == null && courBottle.CurrentCharges == 0 && courOtherItems == null)
                            {
                                courierhero.Spellbook.SpellQ.UseAbility();
                            }

                            if (bottle != null && courBottle == null && courOtherItems == null)
                            {
                                courierhero.Spellbook.SpellQ.UseAbility();
                            }
                            
				

						} 
						
				
						
						
					Utils.Sleep(Menu.Item("Cd").GetValue<Slider>().Value, "rate");
					}
				}
				
				
            
        }
		
		
		private static void Drawing_OnDraw(EventArgs args)
        {
			if (Menu.Item("Forced").GetValue<KeyBind>().Active)
                Drawing.DrawText("ANTIREUSE DELIVER", new Vector2((int)HUDInfo.ScreenSizeX() / 2 - 110, 130), new Vector2(26, 26), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
			if (Menu.Item("Abuse").GetValue<KeyBind>().Active)
                Drawing.DrawText("BOTTLE ABUSE", new Vector2((int)HUDInfo.ScreenSizeX() / 2 - 85, 100), new Vector2(26, 26), Color.Cyan, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
            if (Menu.Item("Lock").GetValue<KeyBind>().Active)
                Drawing.DrawText("LOCK AT BASE", new Vector2((int)HUDInfo.ScreenSizeX() / 2 - 80, 70), new Vector2(26, 26), Color.White, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
			
            /*
			
			var me = ObjectMgr.LocalHero;
			var couriers = ObjectMgr.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == me.Team);
			
		
			if (Game.IsKeyDown(Menu.Item("Selection").GetValue<KeyBind>().Key) && !Game.IsChatOpen)
				{
				if (owned == false)
					owned = true;
				else owned = false;
				}
			
			//foreach (var courier in couriers)
			//var courier = ClosestToMyHero();
			//var courier = ClosestToMouse();
			if (Menu.Item("Abuse").GetValue<KeyBind>().Active)
			{
					var courier1 =  HavingBottle();
					{
					
						
						Vector2 screenPos;
						var pos = courier1.Position + new Vector3(0, 0, courier1.HealthBarOffset);
						Drawing.WorldToScreen(pos, out screenPos);
						var textPos = screenPos + new Vector2(-35, 52);

						Drawing.DrawText("Bottle!", textPos, new Vector2(21, 22), Color.Cyan, FontFlags.AntiAlias | FontFlags.DropShadow);
					
					}
			}
            */
			
			
          
			
		}
		

		
		
		public static Courier ClosestToMyHero()
        {
		
			var myHero = ObjectMgr.LocalHero;
            var Couriers = ObjectMgr.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == myHero.Team);
            Courier[] closestCourier = {null};
            foreach (var cour in Couriers.Where(cour =>
                            closestCourier[0] == null ||
                            closestCourier[0].Distance2D(myHero.Position) > cour.Distance2D(myHero.Position)))
            {
                closestCourier[0] = cour;
            }
            return closestCourier[0];
        }
		
		
		public static Courier ClosestToFontain()
        {
		
			var myHero = ObjectMgr.LocalHero;
            var Couriers = ObjectMgr.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == myHero.Team);
            Courier[] closestCourier = {null};
            foreach (var cour in Couriers.Where(cour =>
                            closestCourier[0] == null ||
                            closestCourier[0].Distance2D(_fountain.Position) > cour.Distance2D(_fountain.Position)))
            {
                closestCourier[0] = cour;
            }
            return closestCourier[0];
        }
		
		
		public static Courier ClosestToMouse()
        {
		
			var myHero = ObjectMgr.LocalHero;
            var mousePosition = Game.MousePosition;
            var Couriers = ObjectMgr.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == myHero.Team);
            Courier[] closestCourier = {null};
            foreach (var cour in Couriers.Where(cour =>
                            closestCourier[0] == null ||
                            closestCourier[0].Distance2D(mousePosition) > cour.Distance2D(mousePosition)))
            {
                closestCourier[0] = cour;
            }
            return closestCourier[0];
        }

        public static Courier HavingBottle()
        {
		
			var myHero = ObjectMgr.LocalHero;
            var Couriers = ObjectMgr.GetEntities<Courier>().Where(x => x.IsAlive && x.Team == myHero.Team);
            Courier[] closestCourier = {null};
            foreach (var cour in Couriers.Where(cour =>
                            //closestCourier[0] == null ||
                            cour.Inventory.Items.FirstOrDefault(x => x.Name == "item_bottle") != null ))
            {
                closestCourier[0] = cour;
            }
            return closestCourier[0];
        }

		
		
    }
}
