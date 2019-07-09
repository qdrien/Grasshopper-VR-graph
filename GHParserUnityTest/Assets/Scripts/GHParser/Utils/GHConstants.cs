using System;
using UnityEngine;
using Color = System.Drawing.Color;

namespace GHParser.Utils
{
    public static class GHConstants
    {
        public static string GroupAsString = "c552a431-af5b-46a9-a8a4-0fcbc27ef596";

        public static string ClusterAsString = "f31d8d7a-7536-4ac8-9c96-fde6ecda4d0a";

        public static Color DefaultGroupColor = Color.FromArgb(150, 170, 135, 255);

        public static UnityEngine.Color DefaultGroupColorAsUnity = new UnityEngine.Color(
            DefaultGroupColor.R / 255f, DefaultGroupColor.G / 255f, DefaultGroupColor.B / 255f,
            DefaultGroupColor.A / 255f);

        public static Guid Group = new Guid("c552a431-af5b-46a9-a8a4-0fcbc27ef596");
        public static Guid Panel = new Guid("59e0b89a-e487-49f8-bab8-b5bab16be14c");
        public static Guid BooleanToggle = new Guid("2e78987b-9dfb-42a2-8b76-3923ac8bd91a");
        public static Guid NumberSlider = new Guid("57da07bd-ecab-415d-9d86-af36d7073abc");
        public static Guid Cluster = new Guid("f31d8d7a-7536-4ac8-9c96-fde6ecda4d0a");

        public static bool IsDefaultColor(Color color)
        {
            return color.R == DefaultGroupColor.R &&
                   color.G == DefaultGroupColor.G &&
                   color.B == DefaultGroupColor.B;
        }

        public static bool IsDefaultColor(UnityEngine.Color color)
        {
            return Mathf.Approximately(color.r, DefaultGroupColorAsUnity.r)
                   && Mathf.Approximately(color.g, DefaultGroupColorAsUnity.g)
                   && Mathf.Approximately(color.b, DefaultGroupColorAsUnity.b);
        }
    }
}