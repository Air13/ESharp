using System;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;


using System.Collections.Generic;
using Ensage.Common;
using Ensage.Common.Extensions;

using System.Linq;



namespace ShowDirectionAir13
{
    internal class Program
    {


		private static readonly Menu Menu = new Menu("Show Direction Air13", "ShowDirection Air13", true, "npc_dota_hero_wisp", true);
        private static Hero me;
        private static int range_arrow;
        private static ParticleEffect rangedisplay;

        private static void Main(string[] args)
        {

		    if (!Game.IsInGame || Game.IsWatchingGame)// || me.ClassID != ClassID.CDOTA_Unit_Hero_Nevermore)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
				
            Menu.AddItem(new MenuItem("menurange_arrow", "Show Hero Direction (arrow)")).SetValue(true);
            Menu.AddItem(new MenuItem("AutoMode", "Auto")).SetValue(true).SetTooltip("Mirana, Slark, SF, Forcestaff/Pike (else attack range)");
			Menu.AddItem(new MenuItem("menurange_own", "Set your range").SetValue(new Slider(700, 100, 2500)));
			
            Menu.AddToMainMenu(); 
			Drawing.OnDraw += DrawRanges;
        }
		

		
		
		
		
		public static void DrawRanges(EventArgs args)
        {
			if (!Game.IsInGame || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
		
			if (Menu.Item("menurange_arrow").GetValue<bool>())
			{
				if (Menu.Item("AutoMode").GetValue<bool>())
				{
					if (me.ClassID == ClassID.CDOTA_Unit_Hero_Nevermore)
						range_arrow = 700;
					else if (me.ClassID == ClassID.CDOTA_Unit_Hero_Slark)
						range_arrow = 700;
					else if (me.ClassID == ClassID.CDOTA_Unit_Hero_Mirana)
					{
						if (me.Spellbook.SpellE.Level == 1)
							range_arrow = 600;
						else if (me.Spellbook.SpellE.Level == 2)
							range_arrow = 700;
						else if (me.Spellbook.SpellE.Level == 3)
							range_arrow = 800;
						else if (me.Spellbook.SpellE.Level == 4)
							range_arrow = 900;
						else
							range_arrow = (int)me.GetAttackRange();
					}
					else if (me.FindItem("item_force_staff")!=null || me.FindItem("item_hurricane_pike")!=null)
						range_arrow = 600;
					else
                        range_arrow = (int)me.GetAttackRange();
				}
				else
					range_arrow = Menu.Item("menurange_own").GetValue<Slider>().Value;
				
				if (rangedisplay == null)
				{
					rangedisplay = new ParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf", me);     
					rangedisplay.SetControlPoint(1, me.NetworkPosition);
					rangedisplay.SetControlPoint(2, FindVector(me.NetworkPosition, me.Rotation, range_arrow));
				}
				else 
				{
					rangedisplay.SetControlPoint(1, me.NetworkPosition);
					rangedisplay.SetControlPoint(2, FindVector(me.NetworkPosition, me.Rotation, range_arrow));
				}
			}
			else if (rangedisplay!=null)
			{
				rangedisplay.Dispose();
				rangedisplay = null;
			}			
			

		}
		
		public static Vector3 FindVector(Vector3 first, double ret, float distance)
        {
            var retVector = new Vector3(first.X + (float) Math.Cos(Utils.DegreeToRadian(ret)) * distance,
                first.Y + (float) Math.Sin(Utils.DegreeToRadian(ret)) * distance, 100);

            return retVector;
        }	
		 


    }
}
