using System;
using System.Collections.Generic;
using System.Drawing;
using GHParser.Graph;
using GH_IO.Serialization;
using QuickGraph;

namespace GHParser.GHElements
{
    [Serializable]
    public abstract class Port : Chunk
    {
        protected Port(Guid guid, string nickname, string defaultName, RectangleF visualBounds)
        {
            Guid = guid;
            Nickname = nickname;
            DefaultName = defaultName;
            VisualBounds = visualBounds;
        }

        public string DefaultName { get; set; }

        public RectangleF VisualBounds { get; set; }

        public override string ToString()
        {
            return "\"" + DefaultName + "\"";
        }

        public override int Add(
            GH_Chunk definitionObjects, int objectIndex, BidirectionalGraph<Vertex, Edge> graph, Vertex vertex)
        {
            //ignoring ports as they are handled by their corresponding IOComponent
            return objectIndex;
        }

        public override bool Remove(Vertex vertex, BidirectionalGraph<Vertex, Edge> graph, List<Group> groups)
        {
            return graph.RemoveVertex(vertex);
        }
    }
}