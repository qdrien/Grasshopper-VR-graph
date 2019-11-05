using System.Collections.Generic;
using Rhino;
using UnityEngine;
using UnityEditor;

public class GrasshopperInUnity : MonoBehaviour
{
  private Rhino.Geometry.Mesh _mesh;
  
  /*public void OpenGHFile(string absolutePath)
  {
    string rhinoCommand = "_Open \"C:\\Users\\Adrien Coppens\\Documents\\projects\\MeshStreamingGrasshopper-master-orig\\GH files\\test-callunity.ghx\"";
    RhinoApp.RunScript(rhinoCommand, false);
  }*/

  // This function will be called from a component in Grasshopper
  void FromGrasshopper(object sender, Rhino.Runtime.NamedParametersEventArgs args)
  {
    Rhino.Geometry.GeometryBase[] values;
    if (args.TryGetGeometry("mesh", out values))
      _mesh = values[0] as Rhino.Geometry.Mesh;
    if (_mesh != null)
      gameObject.GetComponent<MeshFilter>().mesh = _mesh.ToHost();
  }

  void Start()
  {
    gameObject.AddComponent<MeshFilter>();

    gameObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"))
    {
      color = new Color(1.0f, 0.0f, 0.0f, 1f)
    };
    Rhino.Runtime.HostUtils.RegisterNamedCallback("Unity:FromGrasshopper", FromGrasshopper);
  }
}

