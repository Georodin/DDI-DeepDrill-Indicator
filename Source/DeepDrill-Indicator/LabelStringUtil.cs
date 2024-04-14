using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

//THIS IS A COPY FROM https://github.com/Falconne/LabelsOnFloor
namespace DeepDrill_Indicator
{
    public class PlacementData
    {
        public IntVec3 Position;
        public Vector3 Scale;
        public bool Flipped = false;
    }

    public class Label
    {
        public Mesh LabelMesh;
        public IntVec3 startAt;
        public string myText;
        public Vector3 Scale;

        public bool IsValid()
        {
            return startAt != null && LabelMesh != null && myText != null;
        }
    }

    public class LabelHolder
    {
        private readonly List<Label> _currentLabels = new List<Label>();

        private bool _dirty = true;

        public void Clear()
        {
            _currentLabels.Clear();
        }

        public void Add(Label label)
        {
            _currentLabels.Add(label);
            _dirty = true;
        }


        public IEnumerable<Label> GetLabels()
        {
            return _currentLabels;
        }

    }

    public class LabelDrawer
    {
        private readonly FontHandler _fontHandler;

        private readonly LabelHolder _labelHolder;


        public LabelDrawer(LabelHolder labelHolder, FontHandler fontHandler)
        {
            _labelHolder = labelHolder;
            _fontHandler = fontHandler;
        }

        public void Draw()
        {
            var currentViewRect = Find.CameraDriver.CurrentViewRect;
            foreach (var label in _labelHolder.GetLabels())
            {
                if (!currentViewRect.Contains(label.startAt))
                    continue;
                DrawLabel(label);
            }
        }

        private void DrawLabel(Label label)
        {
            Matrix4x4 matrix = default;
            var pos = label.startAt.ToVector3();
            pos.x += 0.2f;

            var rotation = Quaternion.identity;

            matrix.SetTRS(pos, rotation, label.Scale);

            float layer = AltitudeLayer.LowPlant.AltitudeFor();

            Graphics.DrawMesh(label.LabelMesh, matrix, _fontHandler.GetMaterial(), 1);

        }

        public static Vector3 GetScalingVector(int cellCount, int labelLength)
        {
            var scaling = (cellCount - 0.4f) / labelLength;

            if (scaling > 1.2)
                scaling = 1.2f;
            if (scaling < 0.5)
                scaling = 0.5f;
            Log.Message($"returning scale {scaling} for {cellCount} cells and {labelLength} chars");

            return new Vector3(scaling, 1f, scaling);

        }

    }

    public struct CharBoundsInTexture
    {
        public float Left, Right;
    }

    public class FontHandler
    {
        private float _charWidthAsTexturePortion = -1f;

        public Material _material;

        public bool IsFontLoaded()
        {
            if (Resources.Font == null)
                return false;

            if (_charWidthAsTexturePortion < 0f)
                _charWidthAsTexturePortion = 35f / Resources.Font.width;

            return true;
        }

        public Material GetMaterial()
        {
            if (_material == null)
            {
                var color = Color.white;
                color.a = .5f;
                _material = MaterialPool.MatFrom(Resources.Font, ShaderDatabase.MetaOverlay, color);
            }

            return _material;
        }

        public void Reset()
        {
            _material = null;
        }
        public List<List<CharBoundsInTexture>> GetBoundsInTextureFor(string text)
        {

            return text.Split(new string[] { "<br>" }, StringSplitOptions.None)
                       .Select(line => line.Select(c => GetCharBoundsInTextureFor(c)).ToList())
                       .ToList();
        }

        private CharBoundsInTexture GetCharBoundsInTextureFor(char c)
        {
            var index = GetIndexInFontForChar(c);
            var left = index * _charWidthAsTexturePortion;
            return new CharBoundsInTexture()
            {
                Left = left,
                Right = left + _charWidthAsTexturePortion
            };
        }

        private int GetIndexInFontForChar(char c)
        {
            var asciiVal = (int)c;
            if (asciiVal < 33)
                return 0;

            if (asciiVal < 97)
                return asciiVal - 32;

            if (asciiVal < 127)
                return asciiVal - 58;

            return 0;
        }
    }

    public class MeshHandler
    {
        private readonly Dictionary<string, Mesh> _cachedMeshes = new Dictionary<string, Mesh>();

        public readonly FontHandler _fontHandler;


        public MeshHandler(FontHandler fontHandler)
        {
            _fontHandler = fontHandler;
        }

        public Mesh GetMeshFor(string label)
        {
            if (string.IsNullOrEmpty(label))
                return null;

            if (!_fontHandler.IsFontLoaded())
                return null;

            if (!_cachedMeshes.ContainsKey(label))
            {
                _cachedMeshes[label] = CreateMeshFor(label);
            }

            return _cachedMeshes[label];
        }

        public Mesh CreateMeshFor(string label)
        {
            var vertices = new List<Vector3>();
            var uvMap = new List<Vector2>();
            var triangles = new List<int>();
            var size = new Vector2
            {
                x = 1f,
                y = 2f
            };

            var boundsInTextureLineList = _fontHandler.GetBoundsInTextureFor(label);
            var startingTriangleVertex = 0;
            var startingVertexXOffset = 0f;
            var yTop = size.y - 0.4f;
            var yBot = -0.4f;
            var lineSpacing = -1.6f;
            
            foreach (var line in boundsInTextureLineList)
            {
                startingVertexXOffset = 0f;
                foreach (var charBoundsInTexture in line)
                {
                    vertices.Add(new Vector3(startingVertexXOffset, 0f, yBot));
                    vertices.Add(new Vector3(startingVertexXOffset, 0f, yTop));
                    vertices.Add(new Vector3(startingVertexXOffset + size.x, 0f, yTop));
                    vertices.Add(new Vector3(startingVertexXOffset + size.x, 0f, yBot));
                    startingVertexXOffset += size.x;

                    uvMap.Add(new Vector2(charBoundsInTexture.Left, 0f));
                    uvMap.Add(new Vector2(charBoundsInTexture.Left, 1f));
                    uvMap.Add(new Vector2(charBoundsInTexture.Right, 1f));
                    uvMap.Add(new Vector2(charBoundsInTexture.Right, 0f));

                    triangles.Add(startingTriangleVertex + 0);
                    triangles.Add(startingTriangleVertex + 1);
                    triangles.Add(startingTriangleVertex + 2);
                    triangles.Add(startingTriangleVertex + 0);
                    triangles.Add(startingTriangleVertex + 2);
                    triangles.Add(startingTriangleVertex + 3);
                    startingTriangleVertex += 4;
                }
                yTop += lineSpacing;
                yBot += lineSpacing;
            }

            var mesh = new Mesh
            {
                name = "NewPlaneMesh()",
                vertices = vertices.ToArray(),
                uv = uvMap.ToArray()
            };
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

    }

        [StaticConstructorOnStartup]
    public class Resources
    {
        public static Texture2D Font = ContentFinder<Texture2D>.Get("DDIConsolas");
    }
}