using UnityEngine;
using UnityEngine.UI;

namespace PuzzleSpire.Hotel
{
    // Reveal the mesh, not a substring: line breaks remain stable while typing.
    [RequireComponent(typeof(Text))]
    public sealed class HotelTextReveal : BaseMeshEffect
    {
        private int visibleQuads = int.MaxValue;
        public int QuadCount { get; private set; }
        public void Reveal(int count) { visibleQuads = count; graphic.SetVerticesDirty(); }
        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive()) return;
            QuadCount = mesh.currentVertCount / 4;
            var vertex = new UIVertex();
            for (int i = Mathf.Clamp(visibleQuads, 0, QuadCount) * 4; i < mesh.currentVertCount; i++)
            {
                mesh.PopulateUIVertex(ref vertex, i); vertex.color.a = 0; mesh.SetUIVertex(vertex, i);
            }
        }
    }
}
