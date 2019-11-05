using System;
using System.Collections.Generic;
using System.Drawing;
using GHParser.GHElements;

[Serializable]
public class IoComponentTemplate
{

	private string _scriptSource;
	private IoComponent.IoComponentType _componentType;
	private Guid _inputId;
	private Guid _outputId;
	
	private string _defaultName;
	private Guid _typeGuid;
	private string _typeName;
	private RectangleF _visualBounds;
	private string _nickname;

	private List<InputPort> _inputPorts;
	private List<OutputPort> _outputPorts;

	public string ScriptSource
	{
		get { return _scriptSource; }
		set { _scriptSource = value; }
	}

	public IoComponent.IoComponentType ComponentType 
	{
		get { return _componentType; }
		set { _componentType = value; }
	}

	public Guid InputId
	{
		get { return _inputId; }
		set { _inputId = value; }
	}

	public Guid OutputId
	{
		get { return _outputId; }
		set { _outputId = value; }
	}

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

	public string Nickname
	{
		get { return _nickname; }
		set { _nickname = value; }
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

	public IoComponentTemplate(string defaultName, Guid typeGuid, string typeName, RectangleF visualBounds, string nickname, List<InputPort> inputPorts, List<OutputPort> outputPorts)
	{
		_defaultName = defaultName;
		_typeGuid = typeGuid;
		_typeName = typeName;
		_visualBounds = visualBounds;
		_inputPorts = inputPorts;
		_outputPorts = outputPorts;
		_nickname = nickname;
	}
	
	public IoComponentTemplate(string defaultName, Guid typeGuid, string typeName, RectangleF visualBounds, string nickname, List<InputPort> inputPorts, List<OutputPort> outputPorts, string scriptSource, IoComponent.IoComponentType componentType, Guid inputId, Guid outputId)
	{
		_defaultName = defaultName;
		_typeGuid = typeGuid;
		_typeName = typeName;
		_visualBounds = visualBounds;
		_inputPorts = inputPorts;
		_outputPorts = outputPorts;
		_nickname = nickname;
		_scriptSource = scriptSource;
		_componentType = componentType;
		_inputId = inputId;
		_outputId = outputId;
	}

	public override string ToString()
	{
		return "Template of type " + _typeName + " (" + _typeGuid + ") with " + _inputPorts.Count +
		       " input ports and " + _outputPorts.Count + " output ports.(" + _defaultName + " / " + _visualBounds + " / " + _componentType + " / " + _inputId + " / " + _outputId;
		//template.DefaultName, template.TypeGuid, template.TypeName, template.VisualBounds, instanceGuid, componentName, template.ScriptSource, template.ComponentType, template.InputId, template.OutputId);
	}
}
