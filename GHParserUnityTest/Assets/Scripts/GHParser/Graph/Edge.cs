using GHParser.GHElements;
using QuickGraph;

namespace GHParser.Graph
{
    public class Edge : IEdge<Vertex>
    {
        public Vertex Source { get; set; }

        public Vertex Target { get; set; }

        public override string ToString()
        {
            if (Source.Chunk is Component && Target.Chunk is OutputPort
                || Source.Chunk is InputPort && Target.Chunk is Component)
            {
                return "\"" + Source.Chunk.Guid + "\" -> \"" + Target.Chunk.Guid + "\" [style=dashed, color=grey]";
            }

            return "\"" + Source.Chunk.Guid + "\" -> \"" + Target.Chunk.Guid + "\"";
        }
    }
}