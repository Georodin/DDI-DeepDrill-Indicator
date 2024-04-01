using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DeepDrill_Indicator
{
    [StaticConstructorOnStartup]
    public class GridController : MapComponent
    {
        public List<DeepResourcesFarSectionLayer> SectionLayersFar = new List<DeepResourcesFarSectionLayer>();
        public List<DeepResourcesCloseSectionLayer> SectionLayersClose = new List<DeepResourcesCloseSectionLayer>();

        public static MeshHandler meshHandler;

        public void ShouldUpdate(IntVec3 position, bool forceFar = false)
        {
            foreach (DeepResourcesFarSectionLayer layer in SectionLayersFar)
            {
                if (layer.Section.CellRect.Contains(position)||forceFar)
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
                if (layer.Section.CellRect.Contains(position))
                {
                    layer.updateDeepResourceGrid = true;
                }
            }


        }

        static GridController()
        {
            meshHandler = new MeshHandler(new FontHandler());
        }

        public GridController(Map map) : base(map)
        {
        }
    }
}
