using System.Collections.Generic;
using GameWorld.Core.Components.Rendering;
using GameWorld.Core.Rendering.Geometry;
using GameWorld.Core.Rendering.Materials.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameWorld.Core.Rendering.RenderItems
{
    public class PartialGeometryRenderItem : IRenderItem
    {
        private readonly MeshObject _geometry;
        private readonly Matrix _modelMatrix;
        private readonly IShader _shader;
        private readonly List<int> _selectedFaces;

        public PartialGeometryRenderItem(MeshObject geometry, Matrix modelMatrix, IShader shader, List<int> selectedFaces)
        {
            _geometry = geometry;
            _modelMatrix = modelMatrix;
            _shader = shader;
            _selectedFaces = selectedFaces;
        }

        public void Draw(GraphicsDevice device, CommonShaderParameters parameters, RenderingTechnique renderingTechnique)
        {
            _shader.Apply(parameters, _modelMatrix);
            ApplyMeshPart(_shader, device, _selectedFaces, _geometry.GetGeometryContext());
        }

        void ApplyMeshPart(IShader effect, GraphicsDevice device, List<int> faceSelection, IGraphicsCardGeometry geometry)
        {
            if (faceSelection.Count == 0)
                return;

            device.Indices = geometry.IndexBuffer;
            device.SetVertexBuffer(geometry.VertexBuffer);

            // Batch consecutive faces into single DrawIndexedPrimitives calls
            // Each face is one triangle (primitiveCount=1), consecutive faces can be merged
            var sortedFaces = faceSelection;
            if (sortedFaces.Count == 1)
            {
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, sortedFaces[0], 1);
                return;
            }

            int batchStart = sortedFaces[0];
            int batchCount = 1;
            for (int i = 1; i < sortedFaces.Count; i++)
            {
                // Check if current face is adjacent to previous (3 indices apart)
                if (sortedFaces[i] == sortedFaces[i - 1] + 3)
                {
                    batchCount++;
                }
                else
                {
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, batchStart, batchCount);
                    batchStart = sortedFaces[i];
                    batchCount = 1;
                }
            }
            // Draw remaining batch
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, batchStart, batchCount);
        }
    }
}

