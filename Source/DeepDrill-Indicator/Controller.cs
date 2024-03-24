using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DeepDrill_Indicator
{
    [StaticConstructorOnStartup]
    public class Controller
    {
        public static List<DeepResourcesFarSectionLayer> SectionLayersFar = new List<DeepResourcesFarSectionLayer>();
        public static List<DeepResourcesCloseSectionLayer> SectionLayersClose = new List<DeepResourcesCloseSectionLayer>();

        public static MeshHandler meshHandler;

        public static void ShouldUpdate(IntVec3 position, bool forceFar = false)
        {
            ResourceUtil.UpdateGrid(Find.CurrentMap);

            foreach (DeepResourcesFarSectionLayer layer in SectionLayersFar)
            {
                if (layer.p_Section.CellRect.Contains(position)||forceFar)
                {
                    layer.updateDeepResourceGrid = true;
                }
            }

            if (forceFar)
            {
                return;
            }

            foreach (DeepResourcesCloseSectionLayer layer in SectionLayersClose)
            {
                if (layer.p_Section.CellRect.Contains(position))
                {
                    layer.updateDeepResourceGrid = true;
                }
            }
        }

        static Controller(){
            meshHandler = new MeshHandler(new FontHandler());
        }
    }
}
