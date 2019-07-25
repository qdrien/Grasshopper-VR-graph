using System;
using System.Drawing;
using GH_IO.Serialization;

namespace GHParser.GHElements
{
    [Serializable]
    public class InputPort : Port
    {
        [NonSerialized]
        private GH_Chunk _persistentData;

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

        public GH_Chunk PersistentData
        {
            get { return _persistentData; }
            set { _persistentData = value; }
        }

        public string DefaultNameToGive { get; private set; }

        /*public override string ToString()
        {
            return "\"I:" + Guid.ToString().Split('-')[0] + "\"";
        }*/
    }
}