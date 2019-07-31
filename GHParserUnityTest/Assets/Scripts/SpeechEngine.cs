using System;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechEngine : MonoBehaviour {
	[SerializeField]
	private string relativeGrammarFilePath;
	private GrammarRecognizer grammarRecognizer;
	private DictationRecognizer dictationRecognizer;

	private bool _isAwaitingPanelText;
	void Start() {
		grammarRecognizer = new GrammarRecognizer(Application.dataPath + relativeGrammarFilePath);
		grammarRecognizer.OnPhraseRecognized += Grammar_OnPhraseRecognized;
		grammarRecognizer.Start();
		Debug.Log("Recognition started.");
	}
	
	void Grammar_OnPhraseRecognized(PhraseRecognizedEventArgs args) {
		Debug.Log("word: " + args.text + "; conf: " + args.confidence);
		SemanticMeaning type_sm = Array.Find(args.semanticMeanings, (x) => {return x.key.Equals("type");});
		SemanticMeaning value_sm = Array.Find(args.semanticMeanings, (x) => {return x.key.Equals("value");});
		switch(type_sm.values[0]) {
			case "point": //those cases will just fall through up to yzplane
			case "circle":
			case "scale":
			case "vectorxyz":
			case "yzplane":
				GHModelManager.Instance.AttachComponent(type_sm.values[0]);
			break;
			case "boolean":
				//CreateBoolean(Convert.ToBoolean(Int32.Parse(value_sm.values[0])));
				GHModelManager.Instance.AttachComponent(type_sm.values[0], "Boolean", value_sm.values[0]);
			break;
			case "radtodeg": //not sure those 2 should expect a value (they're just components)
				CreateRadtoDeg(Int32.Parse(value_sm.values[0]));
			break;
			case "degtorad":
				CreateDegtoRad(Int32.Parse(value_sm.values[0]));
			break;
			case "panel":
				//todo: should recognise "add component panel" and "add component panel with value"
				//and then treat them differently (no need for the dictation recogniser in the first case)
				CreatePanel();
			break;
			default:
				Debug.Log("Default case reached, trying to add the component anyway.");
				GHModelManager.Instance.AttachComponent(type_sm.values[0]);
			break;
		}
		
		
	}

	void CreateDegtoRad(float x) {
		Debug.Log("CreateDegtoRad with " + x.ToString());
	}

	void CreateRadtoDeg(float x) {
		Debug.Log("CreateRadtoDeg with " + x.ToString());
	}

	void CreatePanel() //should rename this since we're only starting the dictation recogniser here
	{
		//todo: need to give some feedback to the user here ("ok panel") + when dictation is started ("ok text?")
		_isAwaitingPanelText = true;
		Debug.Log("CreatePanel");
		PhraseRecognitionSystem.Shutdown();
		dictationRecognizer = new DictationRecognizer();
		dictationRecognizer.DictationResult += Dictation_OnDictationResult;
		dictationRecognizer.Start();
	}

	void Dictation_OnDictationResult(string text, ConfidenceLevel confidence) {
		Debug.Log("dictation text: " + text);

		if (_isAwaitingPanelText)
		{
			_isAwaitingPanelText = false;
			dictationRecognizer.Stop();
			dictationRecognizer.Dispose();
			PhraseRecognitionSystem.Restart();
			GHModelManager.Instance.AttachComponent("Panel", "Panel", text);
		}
	}
}