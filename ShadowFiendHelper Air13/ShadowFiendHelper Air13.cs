using System;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;


using System.Collections.Generic;
using Ensage.Common;
using Ensage.Common.Extensions;

using System.Linq;



namespace ShadowFiendHelper
{
    internal class Program
    {
	    private static bool menurange_dagger = true;
        private static bool menurange_aura = true;
		private static bool menurange_exp = true;
		private static bool menurange_atck = true;
		private static bool menurange_coil = true;

		private static readonly Menu Menu = new Menu("SF Helper Air13", "ShadowFiendHelper Air13", true, "npc_dota_hero_nevermore", true);
		
        private static Hero me;
        private static int range_exp, range_dagger, range_aura, range_coil;
		private static float range_atck;
        private static ParticleEffect effect, rangedisplay_exp,rangedisplay_dagger,rangedisplay_aura, rangedisplay_atck, rangedisplay_coilQ, rangedisplay_coilW, rangedisplay_coilE;
        private static readonly Dictionary<Unit, ParticleEffect> Effects2 = new Dictionary<Unit, ParticleEffect>();

        private static void Main(string[] args)
        {

		    if (!Game.IsInGame || Game.IsWatchingGame)// || me.ClassID != ClassID.CDOTA_Unit_Hero_Nevermore)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
				
			var coil = new MenuItem("menurange_coil", "Show Hero Direction (arrow)").SetValue(true);		
			var atck = new MenuItem("menurange_atck", "ATTACK Range (red)").SetValue(true);
			var dagger = new MenuItem("menurange_dagger", "BLINK Range (cyan)").SetValue(true);
			var aura = new MenuItem("menurange_aura", "AURA Range (blue)").SetValue(true);
			var exp = new MenuItem("menurange_exp", "EXP Range (white)").SetValue(true);			
 
			menurange_dagger = dagger.GetValue<bool>();
            menurange_aura = aura.GetValue<bool>();
			menurange_exp = exp.GetValue<bool>();
			menurange_atck = atck.GetValue<bool>();
			menurange_coil = coil.GetValue<bool>();

            dagger.ValueChanged += MenuItem_ValueChanged;
            aura.ValueChanged += MenuItem_ValueChanged;
			exp.ValueChanged += MenuItem_ValueChanged;
			atck.ValueChanged += MenuItem_ValueChanged;
			coil.ValueChanged += MenuItem_ValueChanged;
			
			Menu.AddItem(coil.SetTooltip("Show direction for Shadow Razes"));
			Menu.AddItem(atck.SetTooltip("Attack range (Pike, Lance supported)"));
            Menu.AddItem(dagger.SetTooltip("Show range of Blink Dagger at 1200 range (Aether lens supported)"));
            Menu.AddItem(aura.SetTooltip("Show range of most items with aura at 900 range"));
			Menu.AddItem(exp.SetTooltip("Show range of gained expierence at 1300"));

            Menu.AddToMainMenu(); 
			
			Drawing.OnDraw += DrawRanges;

			

        }
		
		
		private static void MenuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;

           
            if (item.Name == "menurange_dagger") menurange_dagger = e.GetNewValue<bool>();
            if (item.Name == "menurange_aura") menurange_aura = e.GetNewValue<bool>();
			if (item.Name == "menurange_exp") menurange_exp = e.GetNewValue<bool>();
			if (item.Name == "menurange_atck") menurange_atck = e.GetNewValue<bool>();
			if (item.Name == "menurange_coil") menurange_coil = e.GetNewValue<bool>();
			
 
        }
		
		
		
		
		public static void DrawRanges(EventArgs args)
        {
			if (!Game.IsInGame || Game.IsWatchingGame)// || me.ClassID != ClassID.CDOTA_Unit_Hero_Nevermore)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
		
			if (menurange_coil)
			{

				if (rangedisplay_coilQ == null)
				{

					rangedisplay_coilQ = new ParticleEffect(@"particles\ui_mouseactions\range_finder_directional_b.vpcf", me);     
					rangedisplay_coilQ.SetControlPoint(1, me.NetworkPosition);
					rangedisplay_coilQ.SetControlPoint(2, FindVector(me.NetworkPosition, me.Rotation, 700));


				}
				else 
				{
					rangedisplay_coilQ.SetControlPoint(1, me.NetworkPosition);
					rangedisplay_coilQ.SetControlPoint(2, FindVector(me.NetworkPosition, me.Rotation, 700));
				} 

	
			}
			else if (rangedisplay_coilQ!=null)
			{
				rangedisplay_coilQ.Dispose();
				rangedisplay_coilQ = null;
			}			
			
			
			
		
		
		
		
		
			if (menurange_atck)
			{
				if(rangedisplay_atck == null)
				{
					rangedisplay_atck = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");	
					range_atck = me.GetAttackRange() + 100;					
					rangedisplay_atck.SetControlPoint(1, new Vector3(255, 0, 0));
					rangedisplay_atck.SetControlPoint(2, new Vector3(range_atck, 255, 0));
				}
				
				if (range_atck != (me.GetAttackRange() + 100))
				{
					range_atck = me.GetAttackRange() + 100;
					rangedisplay_atck.Dispose();
					rangedisplay_atck = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
					rangedisplay_atck.SetControlPoint(1, new Vector3(255, 0, 0));
					rangedisplay_atck.SetControlPoint(2, new Vector3(range_atck, 255, 0));
				}
			}
			else if (rangedisplay_atck!=null)
			{
				rangedisplay_atck.Dispose();
				rangedisplay_atck = null;
			}

		


				
			if (menurange_dagger)
			{
				var aether = me.FindItem("item_aether_lens");
				var aetherrange = 0;
				if (aether == null)
					aetherrange = 0;
				else
					aetherrange = 200;
			
				if (me.FindItem("item_blink")!=null)
				{	
					if(rangedisplay_dagger == null)
					{
						rangedisplay_dagger = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
						range_dagger = 1200  + aetherrange + 130;
						rangedisplay_dagger.SetControlPoint(1, new Vector3(150, 255, 255));
						rangedisplay_dagger.SetControlPoint(2, new Vector3(range_dagger, 255, 0));
					}
					if (range_dagger != 1200  + aetherrange + 130)
					{
						range_dagger = 1200  + aetherrange + 130;
						if(rangedisplay_dagger != null)
							rangedisplay_dagger.Dispose();
						rangedisplay_dagger = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
						rangedisplay_dagger.SetControlPoint(1, new Vector3(150, 255, 255));
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
				
				
				
				
			if (menurange_aura)
			{	
				if(rangedisplay_aura == null)
				{
					rangedisplay_aura = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
					range_aura = 900 + 50 + 80;
                    rangedisplay_aura.SetControlPoint(1, new Vector3(0, 0, 255));
					rangedisplay_aura.SetControlPoint(2, new Vector3(range_aura, 255, 0));
				}
			}
			else if (rangedisplay_aura!=null)
			{
				rangedisplay_aura.Dispose();
				rangedisplay_aura = null;
			}
			
				
			if (menurange_exp)
			{	
				if(rangedisplay_exp == null)
				{
					rangedisplay_exp = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
					range_exp = 1300 + 50 + 80;
                    rangedisplay_exp.SetControlPoint(1, new Vector3(255, 255, 255));
					rangedisplay_exp.SetControlPoint(2, new Vector3(range_exp, 255, 0));
				}
			}
			else if (rangedisplay_exp!=null)
			{
				rangedisplay_exp.Dispose();
				rangedisplay_exp = null;
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
