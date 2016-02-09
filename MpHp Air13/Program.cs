namespace ManaHpBars
{
    using System;
    using System.Linq;

    using Ensage;
    using Ensage.Common;

    using SharpDX;
	




    internal class Program
    {
        #region Methods

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                return;
            }
            var me = ObjectMgr.LocalPlayer;
            if (me == null || me.Team == Team.Observer)
            {
                return;
            }

            var enemies = ObjectMgr.GetEntities<Hero>().Where(x => x.IsVisible && x.IsAlive && x.MaximumMana > 0 && !x.IsIllusion && x.Team != me.Team).ToList();
            foreach (var enemy in enemies)
            {

                var start = HUDInfo.GetHPbarPosition(enemy) + new Vector2(0, HUDInfo.GetHpBarSizeY(enemy) + 1);
                var mprc = enemy.Mana / enemy.MaximumMana;
                var size = new Vector2(HUDInfo.GetHPBarSizeX(enemy), HUDInfo.GetHpBarSizeY(enemy) / 2);
                Drawing.DrawRect(start, size + new Vector2(1, 1), new Color(0, 0, 50, 150));
                Drawing.DrawRect(start, new Vector2(size.X * mprc, size.Y), Color.RoyalBlue);
                Drawing.DrawRect(start + new Vector2(-1, -1), size + new Vector2(3, 3), Color.Black, true);
                var text = string.Format("{0}", (int)enemy.Mana);
				var text2 = string.Format("{0}", (int)enemy.Health);
                var textPos = start + new Vector2(40,9);
				var textPosShad = start + new Vector2(41,9);
				var textPos2 = start + new Vector2(40, -34);
				var textPos2Shad = start + new Vector2(41, -33);
				
                
				Drawing.DrawText(text,textPosShad,new Vector2(21, 20),Color.Black,FontFlags.AntiAlias | FontFlags.DropShadow);
				Drawing.DrawText(text,textPos,new Vector2(21, 20),Color.White,FontFlags.AntiAlias | FontFlags.DropShadow);
				
				Drawing.DrawText(text2,textPos2Shad,new Vector2(21, 21),Color.Black,FontFlags.AntiAlias | FontFlags.DropShadow);
				Drawing.DrawText(text2,textPos2,new Vector2(21, 21),Color.White,FontFlags.AntiAlias | FontFlags.DropShadow);
			}
        }

        private static void Main()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #endregion
    }
}