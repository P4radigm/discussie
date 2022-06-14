using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UZKresultInterperter : MonoBehaviour
{
	public string ConvertToServerString(Vector2[] positions)
	{
		string _output = "";
		for (int i = 0; i < positions.Length; i++)
		{
			if(i != positions.Length - 1) { _output += positions[i].x.ToString("F2") + "c" + positions[i].y.ToString("F2") + "n"; ; }
			else { _output += positions[i].x.ToString("F2") + "c" + positions[i].y.ToString("F2"); }
		}
		return _output;
	} 

	public Vector2[] ConvertToUsableVar(string serverString)
	{
		string[] vecs = serverString.Split('n');
		List<Vector2> outputList = new();

		for (int i = 0; i < vecs.Length; i++)
		{
			string[] coords = vecs[i].Split('c');
			Vector2 newCoords = new Vector2(float.Parse(coords[0]), float.Parse(coords[1]));
			outputList.Add(newCoords);
		}

		Vector2[] _output = new Vector2[outputList.Count];
		for (int i = 0; i < outputList.Count; i++)
		{
			_output[i] = outputList[i];
		}
		return _output;
	}

}
