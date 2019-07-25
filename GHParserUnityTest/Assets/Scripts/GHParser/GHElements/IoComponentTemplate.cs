using System;
using System.Collections.Generic;
using System.Drawing;
using GHParser.GHElements;

[Serializable]
public class IoComponentTemplate
{

	private string _defaultName;
	private Guid _typeGuid;
	private string _typeName;
	private RectangleF _visualBounds;

	private List<InputPort> _inputPorts;
	private List<OutputPort> _outputPorts;
	
	public string DefaultName
	{
		get { return _defaultName; }
		set { _defaultName = value; }
	}

	public Guid TypeGuid
	{
		get { return _typeGuid; }
		set { _typeGuid = value; }
	}

	public string TypeName
	{
		get { return _typeName; }
		set { _typeName = value; }
	}

	public RectangleF VisualBounds
	{
		get { return _visualBounds; }
		set { _visualBounds = value; }
	}

	public List<InputPort> InputPorts
	{
		get { return _inputPorts; }
		set { _inputPorts = value; }
	}

	public List<OutputPort> OutputPorts
	{
		get { return _outputPorts; }
		set { _outputPorts = value; }
	}

	public IoComponentTemplate(string defaultName, Guid typeGuid, string typeName, RectangleF visualBounds, List<InputPort> inputPorts, List<OutputPort> outputPorts)
	{
		_defaultName = defaultName;
		_typeGuid = typeGuid;
		_typeName = typeName;
		_visualBounds = visualBounds;
		_inputPorts = inputPorts;
		_outputPorts = outputPorts;
	}

	public override string ToString()
	{
		return "Template of type " + _typeName + " (" + _typeGuid + ") with " + _inputPorts.Count +
		       " input ports and " + _outputPorts.Count + " output ports.";
	}
}
