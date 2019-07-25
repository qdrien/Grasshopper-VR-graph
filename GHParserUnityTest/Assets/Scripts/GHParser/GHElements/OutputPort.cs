using System;
using System.Drawing;

namespace GHParser.GHElements
{
    [Serializable]
    public class OutputPort : Port
    {
        public OutputPort(Guid guid, string nickname, string defaultName, RectangleF visualBounds) :
            base(guid, nickname, defaultName, visualBounds)
        {
        }

        /*public override string ToString()
        {
            return "\"O:" + Guid.ToString().Split('-')[0] + "\"";
        }*/
    }
}