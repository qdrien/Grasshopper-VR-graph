using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PortInteractUse : VRTK_InteractUse
{
    public GameObject SimpleLinePrefab;
    
    private GameObject _inputPortUsed;
    private GameObject _outputPortUsed;
    private LineRenderer _currentLine;

    void Start()
    {
        _currentLine = Instantiate(SimpleLinePrefab).GetComponent<LineRenderer>();
    }
    
    protected override void Update()
    {
        base.Update();

        if (_inputPortUsed)
        {
            _currentLine.SetPosition(0, transform.position);
            _currentLine.SetPosition(1, _inputPortUsed.transform.position);
        }
        else if (_outputPortUsed)
        {
            _currentLine.SetPosition(0, _outputPortUsed.transform.position);
            _currentLine.SetPosition(1, transform.position);
        }
        else
        {
            _currentLine.SetPosition(0, Vector3.zero);
            _currentLine.SetPosition(1, Vector3.zero);
        }
    }

    public override void OnControllerStartUseInteractableObject(ObjectInteractEventArgs e)
    {
        base.OnControllerStartUseInteractableObject(e);//todo: do we need to call that?
        
        if(e.target.name.Contains("RadialMenu")) return;

        if (e.target.GetComponent<UsePlaceholderHandler>() != null)
        {
            return;
        }

        bool isInputPort = e.target.transform.parent.name.Contains("Input");
        Debug.Log(e.controllerReference.hand + " controller started using with " + (isInputPort ? "input " : "output") + " port " + e.target.transform.name);

        //using the same one again
        if (isInputPort && _inputPortUsed != null && _inputPortUsed == e.target)
        {
            _inputPortUsed = null;
            return;
        }
        if (!isInputPort && _outputPortUsed != null && _outputPortUsed == e.target)
        {
            _outputPortUsed = null;
            return;
        }
        
        if (_inputPortUsed  != null)
        {
            //we had selected an input port and we just selected another one, switching
            if (isInputPort)
            {
                _inputPortUsed = e.target;
            }
            else //we have both an input and an output port, need to create an edge
            {
                GHModelManager.Instance.AddEdge(e.target, _inputPortUsed);
                
                _inputPortUsed = null;
                _outputPortUsed = null;
            }
        }
        else if (_outputPortUsed != null)
        {
            //we had selected an output port and we just selected another one, switching
            if (!isInputPort)
            {
                _outputPortUsed = e.target;
            }
            else //we have both an input and an output port, need to create an edge
            {
                GHModelManager.Instance.AddEdge(_outputPortUsed, e.target);

                _inputPortUsed = null;
                _outputPortUsed = null;
            }
        }
        else //nothing was selected before this one so just save it
        {
            if (isInputPort)
                _inputPortUsed = e.target;
            else
                _outputPortUsed = e.target;
        }
    }

    /*public override void OnControllerUseInteractableObject(ObjectInteractEventArgs e)
    {
        base.OnControllerUseInteractableObject(e);
    }*/

    public override void OnControllerStartUnuseInteractableObject(ObjectInteractEventArgs e)
    {
        base.OnControllerStartUnuseInteractableObject(e);
        
        if(e.target.name.Contains("RadialMenu")) return;
        
        bool isInputPort = e.target.transform.parent.name.Contains("Input");
        Debug.Log(e.controllerReference.hand + " controller stopped using with " + (isInputPort ? "input " : "output") + " port " + e.target.transform.name);

    }

    /*public override void OnControllerUnuseInteractableObject(ObjectInteractEventArgs e)
    {
        base.OnControllerUnuseInteractableObject(e);
    }*/
}
