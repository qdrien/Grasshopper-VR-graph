using System;
using System.Drawing;
using GH_IO.Serialization;

namespace GHParser.GHElements
{
    public class InputPort : Port
    {
        public InputPort(Guid guid, string nickname, string defaultName, RectangleF visualBounds) :
            base(guid, nickname, defaultName, visualBounds)
        {
            DefaultNameToGive = defaultName;
        }

        public InputPort(Guid guid, string nickname, string defaultName, RectangleF visualBounds,
            GH_Chunk persistentData) : this(guid, nickname, defaultName, visualBounds)
        {
            PersistentData = persistentData;
        }

        public GH_Chunk PersistentData { get; set; }

        public string DefaultNameToGive { get; private set; }

        /*public override string ToString()
        {
            return "\"I:" + Guid.ToString().Split('-')[0] + "\"";
        }*/
    }
}