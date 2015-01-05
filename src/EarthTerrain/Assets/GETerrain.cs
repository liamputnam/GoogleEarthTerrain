using UnityEngine;
using System.Collections;
using SimpleJSON;

public class GETerrain : MonoBehaviour
{
    public Vector2 worldPos = new Vector2(0,0);
    public int radius = 1;
	

	public void Start()
	{
		CreateTerrain ();
	}


    public GETerrain(Vector2 _worldPos, int _radius)
    {
        worldPos = _worldPos;
		radius = _radius;
    }
	 
    public void CreateTerrain()
    {
        StartCoroutine(GetHeights(worldPos, radius));
    }

    private IEnumerator GetHeights(Vector2 start, int radius)
    {

		Terrain terra = Terrain.activeTerrain;
		int width = terra.terrainData.heightmapWidth;
		int length = terra.terrainData.heightmapHeight;
		float[,] heights = new float[width,length];

		Debug.Log("GetVertexRow");
        Debug.Log("Start: " + start + "| " + "Radius: " + radius);
        string path = "center=" + start[0] + "," + start[1] + "&radius=" + 
				radius + "&width=" + width + "&length=" + length;
        string url = "104.236.5.114:5000/elevation" + "?" + path;

        Debug.Log(url);
        WWW www = new WWW(url);
        Debug.Log("firing request");
        yield return www;

        if(www.error == null)
        {
			Debug.Log("Request completed");
            //request completed!
        }
        else
        {
			Debug.Log ("WWW Error" + www.error);
            yield return null;
        }

		JSONNode elevation_data = SimpleJSON.JSONNode.Parse (www.text);
        Debug.Log("elevation_data " + elevation_data.ToString());

		if (elevation_data["error"] != null)
		{
			Debug.Log ("Error! " + elevation_data["error"]);
		}
		else
		{


			// loop over results
			for (int i = 0; i < elevation_data.Count; i++)
			{
				Debug.Log("elevation_data[" + i + "] " + elevation_data[i].AsArray);
	            for (int j = 0; j < elevation_data[i].Count; j++)
	            {
					Debug.Log("elevation_data[" + i + "][" + j + "] " + elevation_data[i][j]);
					// record the heights
					heights[i, j] = elevation_data[i][j].AsFloat;

				}
			}            
			terra.terrainData.SetHeights(0,0, heights);
		}
	} 

    public void SetWorldPosition(Vector2 worldPos)
    {
        this.worldPos = worldPos;
    }

    public void SetRadius(int radius)
    {
        this.radius = radius;
    }
	

}
