using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InjectDefinition : MonoBehaviour
{
    public enum CurrentGame{
        uitzoeken,
        conformeren,
        discussie,
        verrijken
	}

    [SerializeField] private CurrentGame game;
    [SerializeField] private bool isPersonal;
    [TextArea]
    [SerializeField] private string definition;
    [SerializeField] private string source;
    [SerializeField] private Vector2 initialRating;
    [SerializeField] private string location;
    [SerializeField] private bool privacy;
    [SerializeField] private string gameResult;


    private void Start()
	{
        int _privacyInt = privacy ? 1 : 0;
        int _isPersonalInt = isPersonal ? 1 : 0;
        Debug.Log(game.ToString());
        StartCoroutine(Inject(game.ToString(), _isPersonalInt, definition, source, initialRating, location, _privacyInt, gameResult));
	}

	private IEnumerator Inject(string _game, int _isPersonal, string _definition, string _source, Vector2 _initialRating, string _location, int _privacy, string _gameResult)
    {
        Debug.Log($"definition = {_definition}");
		WWWForm form = new WWWForm();
		form.AddField("game", _game);
		form.AddField("isPersonal", _isPersonal);
		form.AddField("definition", _definition);
		form.AddField("source", _source);
		form.AddField("initialRatingX", _initialRating.x.ToString());
		form.AddField("initialRatingY", _initialRating.y.ToString());
		form.AddField("location", _location);
		form.AddField("privacy", _privacy);
		form.AddField("gameResult", _gameResult);
		UnityWebRequest req = UnityWebRequest.Post("https://studenthome.hku.nl/~leon.vanoldenborgh/PolarisatieFiles/AddDefinition.php", form);
		yield return req.SendWebRequest();
       
        Debug.Log($"Debug: {req.downloadHandler.text}");

        if (req.downloadHandler.text[0] == '0')
        {
            Debug.Log("Added definition succesfully");
        }
        else
        {
            Debug.Log($"Failed to add definition error: {req.downloadHandler.text}");
        }

        req.Dispose();
    }
}
