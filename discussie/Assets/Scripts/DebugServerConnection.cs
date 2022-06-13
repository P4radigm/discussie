using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DebugServerConnection : MonoBehaviour
{
    TextMeshProUGUI textObject;
    bool hasInternetConnection;
    bool hasServerConnection;
    [SerializeField] private float internetCheckInterval;
    Coroutine hasServerRoutine;

    void Start()
    {
        textObject = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasServerRoutine == null) { StartCoroutine(ServerCheck()); }
        textObject.text = $"ServerConnection = {hasServerConnection}";
		if (hasServerConnection) { textObject.color = Color.green; }
		else { textObject.color = Color.red; }
    }

    private IEnumerator ServerCheck()
    {
        hasInternetConnection = Application.internetReachability == NetworkReachability.NotReachable ? false : true;

        //Might want to update this to server ping instead of google ping
        UnityWebRequest request = new UnityWebRequest("https://studenthome.hku.nl/~leon.vanoldenborgh/");
        yield return request.SendWebRequest();

        hasServerConnection = request.error != null ? false : true;

        yield return new WaitForSeconds(internetCheckInterval);

        hasServerRoutine = null;
    }
}
