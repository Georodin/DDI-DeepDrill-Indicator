using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static RimWorld.ColonistBar;
using System;
using System.Linq;

namespace DeepDrill_Indicator
{
    public class DeepResourcesCloseSectionLayer : SectionLayer
    {
        private MaterialPropertyBlock propertyBlock;
        public static bool isVisible = false;
        public Section Section => section;

        public DeepResourcesCloseSectionLayer(Section section)
            : base(section)
        {
            propertyBlock = new MaterialPropertyBlock();
            Map.GetComponent<GridController>().SectionLayersClose.Add(this);
        }

        public bool updateDeepResourceGrid = false;
        bool isInitialized = false;

        public override void DrawLayer()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                Regenerate();
            }
            
            if (!isVisible)
            {
                return;
            }

            if (Find.CameraDriver.ZoomRootSize > Settings.SwitchThreshold)
            {
                return;
            }

            if (updateDeepResourceGrid)
            {
                updateDeepResourceGrid = false;
                Regenerate();
            }

            //propertyBlock.SetFloat(ShaderPropertyIDs.AgeSecsPausable, RealTime.UnpausedRealTime);
            int count = subMeshes.Count;
            for (int i = 0; i < count; i++)
            {
                LayerSubMesh layerSubMesh = subMeshes[i];
                if (layerSubMesh.finalized && !layerSubMesh.disabled)
                {
                    Graphics.DrawMesh(layerSubMesh.mesh, Vector3.zero, Quaternion.identity, layerSubMesh.material, 0, null, 0, propertyBlock);
                }
            }
        }

        public override void Regenerate()
        {

            ClearSubMeshes(MeshParts.All);
            float altitude = AltitudeLayer.MetaOverlays.AltitudeFor();

            List<LayerSubMesh> subMeshs = new List<LayerSubMesh>();

            foreach (ResourceCell resource in Map.GetComponent<ResourceUtil>().resourceCells)
            {
                if (section.CellRect.Contains(resource.location))
                {
                    //Log.Message("RES: "+resource.ToString());
                    Material currentMaterial = resource.thingDef.graphic.MatSingle;

                    LayerSubMesh subMesh = GetSubMesh(currentMaterial);

                    int startVertIndex = subMesh.verts.Count;

                    AddCell(resource.location, resource.location.GetHashCode(), startVertIndex, subMesh, altitude, 1f);

                    if (!subMeshes.Contains(subMesh))
                    {
                        subMeshes.Add(subMesh);
                    }

                    CreateTextLine(resource.count + "", resource.location, altitude, .2f);
                }
            }

            for (int i = subMeshes.Count - 1; i >= 0; i--)
            {
                if (subMeshes[i].verts.Count == 0)
                {
                    subMeshes.RemoveAt(i);
                }
            }

            foreach (LayerSubMesh subMesh in subMeshes)
            {
                if (subMesh.verts.Count == 0)
                {
                    subMesh.disabled = true;
                }
                subMesh.FinalizeMesh(MeshParts.All);
            }
        }

        void CreateTextLine(string text, IntVec3 center, float altitude, float scaleFactor)
        {
            MeshHandler meshHandler = GridController.meshHandler;
            Mesh fontMesh = meshHandler.GetMeshFor(text);
            Material fontMaterial = meshHandler._fontHandler.GetMaterial();

            Vector3 worldPositionText = new Vector3(center.x + .5f, 0, center.z + .5f);

            LayerSubMesh fontSubMesh = GetSubMesh(fontMaterial);

            if (fontMesh != null)
            {
                // Calculate the bounds and center of the original mesh
                fontMesh.RecalculateBounds();
                Vector3 meshCenter = fontMesh.bounds.center;

                // Translate fontMesh vertices to the worldPosition and scale them down
                Vector3[] translatedAndScaledVertices = new Vector3[fontMesh.vertexCount];
                for (int i = 0; i < fontMesh.vertexCount; i++)
                {
                    // Scale down each vertex
                    Vector3 scaledVertex = fontMesh.vertices[i] * scaleFactor;

                    // Adjust for center alignment: translate the vertex so that the mesh center aligns with the worldPosition
                    Vector3 centeredVertex = scaledVertex - (meshCenter * scaleFactor);

                    // Finally adjust by the worldPosition and altitude
                    translatedAndScaledVertices[i] = centeredVertex + worldPositionText + new Vector3(0, altitude, 0); // Adjust Y by altitude if needed
                }

                // Now, add the translated and scaled vertices to the fontSubMesh
                foreach (Vector3 vertex in translatedAndScaledVertices)
                {
                    fontSubMesh.verts.Add(vertex);
                }

                // Add UVs, colors, and triangles as before, without needing further translation
                foreach (Vector2 uv in fontMesh.uv)
                {
                    fontSubMesh.uvs.Add(uv);
                }

                for (int i = 0; i < fontMesh.vertexCount; i++)
                {
                    fontSubMesh.colors.Add(Color.white);
                }

                int startVertIndex2 = fontSubMesh.verts.Count - fontMesh.vertexCount;
                foreach (int tri in fontMesh.triangles)
                {
                    fontSubMesh.tris.Add(tri + startVertIndex2);
                }
            }


            if (!subMeshes.Contains(fontSubMesh))
            {
                subMeshes.Add(fontSubMesh);
            }
        }

        protected void AddCell(IntVec3 c, int index, int startVertIndex, LayerSubMesh sm, float altitude, float scaleFactor)
        {
            // Adjust the base position to start from the bottom-left corner of the cell
            float baseX = c.x; // Centering correction for x
            float baseZ = c.z; // Centering correction for z
            float y = altitude;

            // Calculate the new size of the cell based on the scale factor
            float sizeX = 1f * scaleFactor;
            float sizeZ = 1f * scaleFactor;

            // Calculate offset to keep the scaled icon centered in the cell
            float offsetX = (1f - sizeX) / 2f;
            float offsetZ = (1f - sizeZ) / 2f;

            // Apply offset and scale to the position
            float x = baseX + offsetX;
            float z = baseZ + offsetZ;

            // Define the corners of the quad based on the adjusted coordinates
            Vector3 v1 = new Vector3(x, y, z); // Bottom-left
            Vector3 v2 = new Vector3(x, y, z + sizeZ); // Top-left
            Vector3 v3 = new Vector3(x + sizeX, y, z + sizeZ); // Top-right
            Vector3 v4 = new Vector3(x + sizeX, y, z); // Bottom-right

            // Add vertices
            sm.verts.Add(v1);
            sm.verts.Add(v2);
            sm.verts.Add(v3);
            sm.verts.Add(v4);

            // UV mapping should remain constant, as it maps the full texture to the quad
            sm.uvs.Add(new Vector2(0f, 0f));
            sm.uvs.Add(new Vector2(0f, 1f));
            sm.uvs.Add(new Vector2(1f, 1f));
            sm.uvs.Add(new Vector2(1f, 0f));

            // Adding color to the vertices
            Color color = Color.white;
            sm.colors.Add(color);
            sm.colors.Add(color);
            sm.colors.Add(color);
            sm.colors.Add(color);

            // Define two triangles to form the quad
            sm.tris.Add(startVertIndex);
            sm.tris.Add(startVertIndex + 1);
            sm.tris.Add(startVertIndex + 2);
            sm.tris.Add(startVertIndex);
            sm.tris.Add(startVertIndex + 2);
            sm.tris.Add(startVertIndex + 3);
        }
    }
}
