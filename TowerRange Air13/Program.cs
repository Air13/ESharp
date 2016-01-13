using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;

namespace TowerRangeAir
{
    internal class Program
    {


        private static readonly List<ParticleEffect> Effects = new List<ParticleEffect>(); 

        private static void Main()
        {
            if (!Game.IsInGame)
                return;

            foreach (var efcts in Effects)
            {
                efcts.Dispose();
            }
            Effects.Clear();

            var me = ObjectMgr.LocalPlayer;
            if (me == null)
                return;
            var towers = ObjectMgr.GetEntities<Building>().Where(x => x.IsAlive && x.ClassID == ClassID.CDOTA_BaseNPC_Tower).ToList();
			var thrones = ObjectMgr.GetEntities<Building>().Where(x => x.IsAlive && x.ClassID == ClassID.CDOTA_BaseNPC_Fort).ToList();			
			
            if (!towers.Any())
                return;
			if (!thrones.Any())
                return;

                
    

                    foreach (var effect in towers.Where(x => x.Team != me.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf")))
                    {
	                effect.SetControlPoint(1, new Vector3(255, 255, 0));
					effect.SetControlPoint(2, new Vector3(1000, 255, 0));
					Effects.Add(effect);
                    }
                    foreach (var effect in towers.Where(x => x.Team == me.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf")))
                    {
	                effect.SetControlPoint(1, new Vector3(255, 255, 0));
					effect.SetControlPoint(2, new Vector3(1000, 255, 0));
					Effects.Add(effect);
                    }
					
					foreach (var effect in towers.Where(x => x.Team != me.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf")))
                    {
	                effect.SetControlPoint(1, new Vector3(255, 0, 0));
					effect.SetControlPoint(2, new Vector3(950, 255, 0));
					Effects.Add(effect);
                    }
                    foreach (var effect in towers.Where(x => x.Team == me.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf")))
                    {
	                effect.SetControlPoint(1, new Vector3(255, 0, 0));
					effect.SetControlPoint(2, new Vector3(950, 255, 0));
                    Effects.Add(effect);
                    }
					
					

					foreach (var effect2 in thrones.Where(x => x.Team != me.Team).Select(throne => throne.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf")))
                    {
                    effect2.SetControlPoint(1, new Vector3(255, 255, 0));
					effect2.SetControlPoint(2, new Vector3(1000, 255, 0));
                    Effects.Add(effect2);
                    }
                    foreach (var effect2 in thrones.Where(x => x.Team == me.Team).Select(throne => throne.AddParticleEffect(@"particles\ui_mouseactions\drag_selected_ring.vpcf")))
                    {
 	                effect2.SetControlPoint(1, new Vector3(255, 255, 0));
					effect2.SetControlPoint(2, new Vector3(1000, 255, 0));
                    Effects.Add(effect2);
                    }
			
                
        }

    }
}
