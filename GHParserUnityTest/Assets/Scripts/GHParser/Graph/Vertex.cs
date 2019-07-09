using GHParser.GHElements;

namespace GHParser.Graph
{
    public class Vertex
    {
        public Vertex(Chunk chunk)
        {
            Chunk = chunk;
        }

        public Chunk Chunk { get; set; }

        public override string ToString()
        {
            return Chunk.ToString();
        }
    }
}