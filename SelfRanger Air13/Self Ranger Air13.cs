using System;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;


using System.Collections.Generic;
using Ensage.Common;
using Ensage.Common.Extensions;

using System.Linq;



namespace SelfRanger
{
    internal class Program
    {
		private static bool _alens = false;
	    private static bool menurange_dagger = true;
        private static bool menurange_aura = true;
		private static bool menurange_exp = true;
		private static bool menurange_atck = true;
		private static bool menurange_own = true;
		private static int menurange_own_amount;
		private static readonly Menu Menu = new Menu("Self Ranger Air13", "Self Ranger Air13", true);
		
        private static Hero me;
        private static int range_exp, range_dagger, range_aura;
		private static float range_atck;
        private static ParticleEffect rangedisplay_exp,rangedisplay_dagger,rangedisplay_aura, rangedisplay_atck, rangedisplay_own;
		
        private static void Main(string[] args)
        {

			
		    if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            me = ObjectMgr.LocalHero;
            if (me == null)
                return;
				
			var dagger = new MenuItem("menurange_dagger", "BLINK DAGGER (cyan)").SetValue(true);
			var aura = new MenuItem("menurange_aura", "AURA (blue)").SetValue(true);
			var atck = new MenuItem("menurange_atck", "ATTACK (red)").SetValue(true);
			var exp = new MenuItem("menurange_exp", "EXP (yellow)").SetValue(true);
			var own = new MenuItem("menurange_own", "YOUR RANGE (green)").SetValue(true);
			var own_amount = new MenuItem("menurange_own_amount", "Set your own range").SetValue(new Slider(260, 100, 3500));
 
			menurange_dagger = dagger.GetValue<bool>();
            menurange_aura = aura.GetValue<bool>();
			menurange_exp = exp.GetValue<bool>();
			menurange_atck = atck.GetValue<bool>();
			menurange_own = own.GetValue<bool>();
			menurange_own_amount = own_amount.GetValue<Slider>().Value; 

            dagger.ValueChanged += MenuItem_ValueChanged;
            aura.ValueChanged += MenuItem_ValueChanged;
			exp.ValueChanged += MenuItem_ValueChanged;
			atck.ValueChanged += MenuItem_ValueChanged;
			own.ValueChanged += MenuItem_ValueChanged;
			

            Menu.AddItem(dagger.SetTooltip("Show range of Blink Dagger at 1200 range (Aether lens supported - reload script to apply new range)"));
            Menu.AddItem(aura.SetTooltip("Show range of most items with aura at 900 range"));
			Menu.AddItem(atck.SetTooltip("Attack range (if u have dragon lance or change range attack - reload script to apply new range)"));
			Menu.AddItem(exp.SetTooltip("Show range of gained expierence at 1300"));
			
			Menu.AddItem(own.SetTooltip("Allow to set your own range at slider below. Use it for hero's spells"));
			Menu.AddItem(own_amount);

            Menu.AddToMainMenu(); 
			
			var alens = me.FindItem("item_aether_lens");
			if (alens == null) 
				_alens = false;
			else
				_alens = true;

			
            if(rangedisplay_exp == null)
                  rangedisplay_exp = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
			if(rangedisplay_dagger == null)
				  rangedisplay_dagger = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
			if(rangedisplay_aura == null)
				  rangedisplay_aura = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
			if(rangedisplay_atck == null)
				  rangedisplay_atck = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
			if(rangedisplay_own == null)
				  rangedisplay_own = me.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf");
				  

			if (me.IsAlive)
            {
				if (menurange_exp)
				{
					range_exp = 1300 + 50 + 80;
                    rangedisplay_exp.SetControlPoint(1, new Vector3(255, 255, 0));
					rangedisplay_exp.SetControlPoint(2, new Vector3(range_exp, 255, 0));
				}
				if (menurange_own)
				{
					//range_exp = 260 + 70;
					//range_own = 1300 + 50 + 80;
                    rangedisplay_own.SetControlPoint(1, new Vector3(0, 255, 0));
					rangedisplay_own.SetControlPoint(2, new Vector3(menurange_own_amount + 60, 255, 0));
				}

				if (menurange_dagger && !_alens)
				{	 
					range_dagger = 1200 + 50 + 80;
                    rangedisplay_dagger.SetControlPoint(1, new Vector3(0, 255, 255));
					rangedisplay_dagger.SetControlPoint(2, new Vector3(range_dagger, 255, 0));
				}
				if (menurange_dagger && _alens)
				{	 
					range_dagger = 1200 + 50 + 80 + 200;
                    rangedisplay_dagger.SetControlPoint(1, new Vector3(0, 255, 255));
					rangedisplay_dagger.SetControlPoint(2, new Vector3(range_dagger, 255, 0));
				}
				
				if (menurange_aura)
				{	
					range_aura = 900 + 50 + 80;
                    rangedisplay_aura.SetControlPoint(1, new Vector3(0, 0, 255));
					rangedisplay_aura.SetControlPoint(2, new Vector3(range_aura, 255, 0));
				}
				
				if (menurange_atck)
				{	
                    range_atck = me.GetAttackRange() + 100;					
                    rangedisplay_atck.SetControlPoint(1, new Vector3(255, 0, 0));
                    rangedisplay_atck.SetControlPoint(2, new Vector3(range_atck, 255, 0));
				}
				
					
                
            }
            else if(!me.IsAlive)
            {

                    rangedisplay_dagger.Dispose();
                    rangedisplay_dagger = null;
					
					rangedisplay_aura.Dispose();
                    rangedisplay_aura = null;
					
					rangedisplay_exp.Dispose();
                    rangedisplay_exp = null;
					
					rangedisplay_atck.Dispose();
                    rangedisplay_atck = null;
                
            }
        }
		
		
		private static void MenuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;

           
            if (item.Name == "menurange_dagger") menurange_dagger = e.GetNewValue<bool>();
            if (item.Name == "menurange_aura") menurange_aura = e.GetNewValue<bool>();
			if (item.Name == "menurange_exp") menurange_exp = e.GetNewValue<bool>();
			if (item.Name == "menurange_atck") menurange_atck = e.GetNewValue<bool>();
			if (item.Name == "menurange_own") menurange_own = e.GetNewValue<bool>();
			
 
        }

    }
}
