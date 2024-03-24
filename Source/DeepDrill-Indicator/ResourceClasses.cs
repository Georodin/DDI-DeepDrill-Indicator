using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DeepDrill_Indicator
{

    public class ResourceUtil
    {
        internal static List<ResourceField> resourcesField = new List<ResourceField>();
        internal static List<ResourceInGrid> resourcesInGrid = new List<ResourceInGrid>();
        internal static bool isInitialized = false;
        internal static void UpdateGrid(Map map)
        {
            
            List<List<ResourceInGrid>> fields = new List<List<ResourceInGrid>>();
            isInitialized = true;
            resourcesField.Clear();
            resourcesInGrid.Clear();

            foreach (IntVec3 c in map.AllCells)
            {
                ThingDef thingDef = map.deepResourceGrid.ThingDefAt(c);
                if (thingDef != null) // If there's a resource at this cell
                {
                    int resourceCount = map.deepResourceGrid.CountAt(c);
                    if (resourceCount > 0) // And there's some amount of it
                    {
                        resourcesInGrid.Add(new ResourceInGrid(thingDef, c, resourceCount));
                        //Log.Message($"{c}\t{thingDef.defName}\t{resourceCount}");
                    }
                }
            }

            //Log.Message($"count... {resourcesInGrid.Count}");

            for (int i = 0; i < resourcesInGrid.Count; i++)
            {
                if (resourcesInGrid[i].Sorted)
                {
                    continue;
                }

                List<ResourceInGrid> field = new List<ResourceInGrid>();
                //Log.Message("CREATED FIELD");
                fields.Add(field);
                RecursiveFieldGrouping(resourcesInGrid[i], field, resourcesInGrid);
            }

            foreach (var field in fields)
            {
                if (!field.Any()) continue; // Skip empty fields
                                            // Assuming all resources in a field have the same ThingDef
                string resourceName = field.First().thingDef.defName;
                int fieldCount = field.Count;
                int totalResourceCount = field.Sum(resource => resource.count);

                // Calculate the center of the field (average X and Z)
                double averageX = field.Average(resource => resource.location.x);
                double averageZ = field.Average(resource => resource.location.z);

                // Round or format averageX and averageZ if needed for better readability
                // For example, rounding to the nearest whole number:
                int centerX = (int)Math.Round(averageX);
                int centerZ = (int)Math.Round(averageZ);

                IntVec3 center = new IntVec3(centerX, 0, centerZ);

                resourcesField.Add(new ResourceField(field.First().thingDef, center, totalResourceCount, fieldCount));
            }
        }

        static void RecursiveFieldGrouping(ResourceInGrid origin, List<ResourceInGrid> output, List<ResourceInGrid> resourcesInGrid)
        {
            origin.Sorted = true;
            output.Add(origin);
            //Log.Message($"S: {origin} {resourcesInGrid.Count}");

            List<ResourceInGrid> recursiv = new List<ResourceInGrid>();

            foreach (ResourceInGrid child in resourcesInGrid)
            {
                if (child != origin && !child.Sorted)
                {
                    if (IsCloseEnough(child, origin))
                    {
                        //Log.Message($"Y {child}");
                        recursiv.Add(child);
                    }
                    else
                    {
                        //Log.Message($"N {child}");
                    }
                }
            }

            foreach (ResourceInGrid child in recursiv)
            {
                if (!child.Sorted)
                {
                    RecursiveFieldGrouping(child, output, resourcesInGrid);
                }
            }
        }

        static bool IsCloseEnough(ResourceInGrid resource1, ResourceInGrid resource2)
        {
            // Check the difference in X and Z coordinates
            int diffX = Math.Abs(resource1.location.x - resource2.location.x);
            int diffZ = Math.Abs(resource1.location.z - resource2.location.z);

            // Resources are "close enough" if either coordinate difference is 0 or 1
            // and not both coordinates are differing by more than 1 at the same time
            return (diffX <= 1 && diffZ <= 1) && !(diffX == 1 && diffZ == 1);
        }
    }



    internal class ResourceInGrid
    {
        public ThingDef thingDef;
        public IntVec3 location;
        public int count;
        bool sorted = false;

        public ResourceInGrid(ThingDef thingDef, IntVec3 location, int count)
        {
            this.thingDef = thingDef;
            this.location = location;
            this.count = count;
        }

        public bool Sorted { get => sorted; set => sorted = value; }

        public override string ToString()
        {
            return $"{thingDef?.defName ?? "null"}, ({location.x}, {location.y}, {location.z}), {count}, {sorted}]";
        }
    }

    internal class ResourceField
    {
        public ThingDef thingDef;
        public IntVec3 center;
        public int totalCount;
        public int tileCount;
        bool sorted = false;

        public ResourceField(ThingDef thingDef, IntVec3 location, int totalCount, int tileCount)
        {
            this.thingDef = thingDef;
            this.center = location;
            this.totalCount = totalCount;
            this.tileCount = tileCount;
        }

        public bool Sorted { get => sorted; set => sorted = value; }

        public override string ToString()
        {
            return $"{thingDef?.defName ?? "null"}, ({center.x}, {center.y}, {center.z}), {totalCount}, {sorted}]";
        }
    }
}
