using System;
using System.Collections.Generic;
using GHParser.Graph;
using GHParser.Utils;
using GH_IO.Serialization;
using QuickGraph;

namespace GHParser.GHElements
{
    public abstract class PrimitiveComponent : Component
    {
        protected PrimitiveComponent(
            GH_Chunk objectChunk, Dictionary<Guid, Vertex> existingVertices, List<Guid[]> edges)
            : base(objectChunk, existingVertices, edges)
        {
            DefaultName = TypeName;
        }

        /*public override string ToString()
        {
            return "\"P:" + Nickname + "@" + Guid.ToString().Split('-')[0] + "\"";
        }*/

        public static PrimitiveComponent CreatePrimitive(
            GH_Chunk objectChunk, Dictionary<Guid, Vertex> vertices, List<Guid[]> edges)
        {
            Guid typeGuid = objectChunk.GetGuid("GUID");
            if (GHConstants.BooleanToggle.Equals(typeGuid))
            {
                return new BooleanToggleComponent(objectChunk, vertices, edges);
            }

            if (GHConstants.NumberSlider.Equals(typeGuid))
            {
                return new NumberSliderComponent(objectChunk, vertices, edges);
            }

            if (GHConstants.Panel.Equals(typeGuid))
            {
                return new PanelComponent(objectChunk, vertices, edges);
            }

            return new GenericPrimitiveComponent(objectChunk, vertices, edges);
        }

        public override bool Remove(Vertex vertex, BidirectionalGraph<Vertex, Edge> graph, List<Group> groups)
        {
            return graph.RemoveVertex(vertex);
        }
    }
}