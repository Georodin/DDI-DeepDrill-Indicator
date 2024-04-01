using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Diagnostics; // Make sure to include this for Stopwatch
using UnityEngine;
using Verse.Noise;
using RimWorld;

namespace DeepDrill_Indicator
{

    public class ResourceUtil : MapComponent
    {
        internal List<ResourceField> resourcesField = new List<ResourceField>();
        internal List<ResourceCell> resourceCells = new List<ResourceCell>();

        public ResourceUtil(Map map) : base(map)
        {
            InitializeGrid();
        }

        internal void InitializeGrid()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            resourcesField.Clear();
            resourceCells.Clear();

            // Populate resourceCells list
            foreach (IntVec3 c in map.AllCells)
            {
                ThingDef thingDef = map.deepResourceGrid.ThingDefAt(c);
                if (thingDef != null)
                {
                    int resourceCount = map.deepResourceGrid.CountAt(c);
                    if (resourceCount > 0)
                    {
                        resourceCells.Add(new ResourceCell(thingDef, c, resourceCount));
                    }
                }
            }

            ProcessAllCellsIntoFields();

            stopwatch.Stop();
            Log.Message($"InitializeGrid execution time: {stopwatch.ElapsedMilliseconds} ms");
        }

        public void UpdateAt(IntVec3 position)
        {
            // Find an existing ResourceCell at the given position
            ResourceCell existingCell = resourceCells.FirstOrDefault(cell => cell.location.Equals(position));

            // Attempt to get the ThingDef and resource count at the position
            ThingDef thingDef = map.deepResourceGrid.ThingDefAt(position);
            int resourceCount = thingDef != null ? map.deepResourceGrid.CountAt(position) : 0;

            // If there is an existing cell at the position...
            if (existingCell != null)
            {
                if (thingDef == null || resourceCount <= 0)
                {
                    // If ThingDef is null or count is 0, remove the existing cell
                    if (existingCell.resourceField != null)
                    {
                        existingCell.resourceField.resources.Remove(existingCell);
                    }
                    resourceCells.Remove(existingCell);
                }
                else
                {
                    // Update the existing cell's thingDef and count
                    existingCell.thingDef = thingDef;
                    existingCell.count = resourceCount;
                }
            }
            else if (thingDef != null && resourceCount > 0)
            {
                // If there's no existing cell but we have valid ThingDef and count, add a new cell
                resourceCells.Add(new ResourceCell(thingDef, position, resourceCount));
            }
        }

        void RecursiveAddToField(ResourceCell cellA, ResourceField field = null)
        {
            if (field == null)
            {
                field = new ResourceField(cellA);
                resourcesField.Add(field);
            }
            else
            {
                field.AddToField(cellA);
            }

            var neighbors = resourceCells.Where(cell => cell.resourceField == null && IsCloseEnough(cellA, cell)).ToList();
            foreach (ResourceCell neighbor in neighbors)
            {
                neighbor.resourceField = field;
                RecursiveAddToField(neighbor, field);
            }
        }

        internal void RemoveEmptyFields()
        {
            resourcesField.RemoveAll(field => field.resources.Count == 0);
        }

        internal void ProcessAllCellsIntoFields()
        {
            while (resourceCells.Any(c => c.resourceField == null))
            {
                var firstUnassignedCell = resourceCells.FirstOrDefault(c => c.resourceField == null);
                if (firstUnassignedCell != null)
                {
                    RecursiveAddToField(firstUnassignedCell);
                }
            }

            RemoveEmptyFields();
        }

        internal void UpdateGridFields()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ProcessAllCellsIntoFields();

            Log.Message($"UpdateGrid execution time: {stopwatch.ElapsedMilliseconds} ms");
        }

        static bool IsCloseEnough(ResourceCell cellA, ResourceCell cellB)
        {
            if (cellA.thingDef != cellB.thingDef)
            {
                return false;
            }

            // Check the difference in X and Z coordinates
            int diffX = Math.Abs(cellA.location.x - cellB.location.x);
            int diffZ = Math.Abs(cellA.location.z - cellB.location.z);

            // Resources are "close enough" if either coordinate difference is 0 or 1
            // and not both coordinates are differing by more than 1 at the same time
            return (diffX <= 1 && diffZ <= 1) && !(diffX == 1 && diffZ == 1);
        }
    }

    internal class ResourceCell
    {
        public ThingDef thingDef;
        public IntVec3 location;
        public int count;
        public ResourceField resourceField = null;

        public ResourceCell(ThingDef thingDef, IntVec3 location, int count)
        {
            this.thingDef = thingDef;
            this.location = location;
            this.count = count;
        }

        public override string ToString()
        {
            return $"Cell:{thingDef?.defName ?? "null"}, ({location.x}, {location.y}, {location.z}), {count}, {resourceField}]";
        }
    }

    internal class ResourceField
    {
        public ThingDef thingDef;
        public IntVec3 center => GetCenter();
        public List<ResourceCell> resources = new List<ResourceCell>();

        IntVec3 GetCenter()
        {
            // Calculate the center of the field (average X and Z)
            double averageX = resources.Average(resource => resource.location.x);
            double averageZ = resources.Average(resource => resource.location.z);

            // Round or format averageX and averageZ if needed for better readability
            // For example, rounding to the nearest whole number:
            int centerX = (int)Math.Round(averageX);
            int centerZ = (int)Math.Round(averageZ);

            return new IntVec3(centerX, 0, centerZ);
        }

        public int GetResCount()
        {
            return resources.Sum(resource => resource.count);
        }
        public int GetTileCount()
        {
            return resources.Count;
        }

        public void AddToField(ResourceCell cell)
        {
            if (!resources.Contains(cell))
            {
                resources.Add(cell);
                cell.resourceField = this;
            }
        }

        public ResourceField(ResourceCell cell)
        {
            AddToField(cell);
            thingDef = resources[0].thingDef;
        }

        public override string ToString()
        {
            return $"Field: {thingDef?.defName ?? "null"}, ({center.x}, {center.y}, {center.z}), {GetResCount()}, {GetTileCount()}]";
        }
    }
}
