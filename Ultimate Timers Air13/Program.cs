using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Items;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;
using Font = SharpDX.Direct3D9.Font;

using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;




namespace UltimateTimers
{
    internal static class Program
    {
		public static Hero me;
		
		private static readonly Menu Menu=new Menu("Ultimate Timers Air13","Ultimate Timers Air13",true);

		private static readonly Dictionary<Hero, Ability> UltimateAbilities = new Dictionary<Hero, Ability>();


	
		
		
		
        
        private static void Main()
        {
            Menu.AddItem(new MenuItem("TopPanel.Ultimates", "Enemy's Ultimates Cooldowns").SetValue(true).SetFontStyle(FontStyle.Bold, Color.Green));

			
			
			var settings = new Menu("Settings", "settings");
            settings.AddItem(new MenuItem("BarPosX", "Position X").SetValue(new Slider(0, -300, 300)));
            settings.AddItem(new MenuItem("BarPosY", "Position Y").SetValue(new Slider(0, -300, 300)));
            settings.AddItem(new MenuItem("BarSizeY", "Size").SetValue(new Slider(0, -10, 10)));
			

			Menu.AddSubMenu(settings);


            Menu.AddToMainMenu();
			

			


            Events.OnLoad += (sender, args) =>
            {
                Drawing.OnDraw+=Drawing_OnDraw;
            };

            Events.OnClose += (sender, args) =>
            {
                TopPos.Clear();
                Drawing.OnDraw -= Drawing_OnDraw;
            };
            
        }








        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame) return;


            var me = ObjectMgr.LocalHero;
            foreach (var v in Ensage.Common.Objects.Heroes.GetByTeam(me.GetEnemyTeam()))
            {
                var pos = GetTopPanelPosition(v) +
                          new Vector2(Menu.Item("BarPosX").GetValue<Slider>().Value,
                              Menu.Item("BarPosY").GetValue<Slider>().Value);
                var size = GetTopPalenSize(v) + new Vector2(0, Menu.Item("BarSizeY").GetValue<Slider>().Value);
                const int height = 7;
				
				
				
				if (Menu.Item("TopPanel.Ultimates").GetValue<bool>())
                {
                    try
                    {
                        Ability ultimate;
                        if (!UltimateAbilities.TryGetValue(v, out ultimate))
                        {
                            var ult = v.Spellbook.Spells.First(x => x.AbilityType == AbilityType.Ultimate);
                            if (ult != null) UltimateAbilities.Add(v, ult);
                        }
                        else if (ultimate != null && ultimate.Level > 0)
                        {
                            pos = GetTopPanelPosition(v) +new Vector2(Menu.Item("BarPosX").GetValue<Slider>().Value,Menu.Item("BarPosY").GetValue<Slider>().Value);									  
                            size = GetTopPalenSize(v) + new Vector2(0, Menu.Item("BarSizeY").GetValue<Slider>().Value);
       
							
								
								var ult1 = v.Spellbook.Spells.First(x => x.AbilityType == AbilityType.Ultimate);
								var cd = ult1.Cooldown;
								var ult1pos = pos + new Vector2(size.X/2 - 5, size.Y - 22);              
								var text = string.Format("{0:0}", cd);
								var ColorUlt = Color.Pink;

							
                            switch (ultimate.AbilityState)
                            {
                                case AbilityState.NotEnoughMana:
									ColorUlt = Color.Red;
                                    break;											
                                case AbilityState.OnCooldown:									
									ColorUlt = Color.White;                                   
                                    break;									
                                default:								
									ColorUlt = Color.Lime;                                    									
                                    break;									
                            }


							if (Menu.Item("TopPanel.Ultimates").GetValue<bool>())
							{
								Drawing.DrawText(text, ult1pos + new Vector2(6, size.Y + height*2+1),new Vector2(size.Y/2, size.X/2), Color.Black, FontFlags.AntiAlias | FontFlags.DropShadow);
								Drawing.DrawText(text, ult1pos + new Vector2(5, size.Y + height*2),new Vector2(size.Y/2, size.X/2), ColorUlt, FontFlags.AntiAlias | FontFlags.DropShadow);

							}
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
               
                
            }
	

        }
		
		
		
		
		
		
		
		
		
		

      

       

       




        private static Vector2 GetTopPalenSize(Hero hero)
        {
            return new Vector2((float)HUDInfo.GetTopPanelSizeX(hero), (float)HUDInfo.GetTopPanelSizeY(hero));
        } 
        
        private static readonly Dictionary<uint,Vector2> TopPos=new Dictionary<uint, Vector2>();

        private static Vector2 GetTopPanelPosition(Hero v)
        {
            Vector2 vec2;
            var handle = v.Handle;
            if (TopPos.TryGetValue(handle, out vec2)) return vec2;
            vec2 = HUDInfo.GetTopPanelPosition(v);
            TopPos.Add(handle,vec2);
            return vec2;
        }



    }
}
