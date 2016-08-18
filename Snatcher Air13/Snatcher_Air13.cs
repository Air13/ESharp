using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

using SharpDX;

namespace Snatcher_Air13 {
    internal class Program {
	
        private static Hero me;
        private static readonly Menu Menu = new Menu("Snatcher Air13", "Snatcher Air13", true, "rune_haste", true);
        private static void Main() {


            Menu.AddItem(new MenuItem("ToggleKey", "Toggle Key").SetValue(new KeyBind('G', KeyBindType.Toggle)));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
			Drawing.OnDraw += Information;

        }

        private static void Game_OnUpdate(EventArgs args) 
		{
			if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;

            if (!me.IsAlive || me == null || !Menu.Item("ToggleKey").GetValue<KeyBind>().Active) 
                return;
            
            if (Menu.Item("ToggleKey").GetValue<KeyBind>().Active)  
			{
                var rune = ObjectMgr.GetEntities<Rune>().FirstOrDefault(x => x.IsVisible && x.Distance2D(me) < 350);
                var aegis = ObjectMgr.GetEntities<PhysicalItem>().FirstOrDefault(x => x.IsVisible && x.Distance2D(me) < 380 && x.Item.Name == "item_aegis");
                if (rune != null) 
				{
                    me.PickUpRune(rune);
                    return;
                }
                if (aegis != null && me.Inventory.FreeSlots.Any()) 
				{
                    me.PickUpItem(aegis);
                    return;
                }
            }
        }
		
		
		static void Information(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;

            if (Menu.Item("ToggleKey").GetValue<KeyBind>().Active == true)
            {
                Drawing.DrawText("SNATCHING!", new Vector2(HUDInfo.ScreenSizeX() / 2 +2, HUDInfo.ScreenSizeY() / 2 + 235 + 2), new Vector2(30, 200), Color.Black, FontFlags.AntiAlias);
                Drawing.DrawText("SNATCHING!", new Vector2(HUDInfo.ScreenSizeX() / 2, HUDInfo.ScreenSizeY() / 2 + 235), new Vector2(30, 200), Color.Cyan, FontFlags.AntiAlias);
            }
		}
    }
}