using UnityEngine;
using System.Collections;
using SimpleJSON;

public class GETerrain : MonoBehaviour
{
    public Vector2 worldPos;
    public int radius = 2;
    [HideInInspector]
    public bool requestFailed = false;

    private float[,] heights;

    private string urlStem = "https://maps.googleapis.com/maps/api/elevation/json";

    private float km_per_degree_latitude = 111.2f;
	
    private float[,] vertexGrid;

    public GETerrain(Vector2 _worldPos, int _radius)
    {
        worldPos = _worldPos;
		radius = _radius;
    }

    public void ClearVertexGrid()
    {
        vertexGrid = new float[2 * radius, 2 * radius];
        for (int i = 0; i < 2 * radius; i++)
        {
            for (int j = 0; j < 2 * radius; j++)
            {
                vertexGrid[i, j] = Mathf.Infinity;
            }
        }
        km_per_degree_latitude = 111.2f;
        urlStem = "https://maps.googleapis.com/maps/api/elevation/json";
    }

    public void CreateTerrain()
    {
        StartCoroutine(GetHeights(worldPos, radius));
    }

    private IEnumerator GetHeights(Vector2 start, int radius)
    {
        Debug.Log("GetVertexRow");
        Debug.Log("Start: " + start + "| " + "Radius: " + radius);
        string path = "center=" + start[0] + "," + start[1] + "&radius=" + radius;
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
            requestFailed = true;
            yield return null;
        }

		JSONNode elevation_data = SimpleJSON.JSONNode.Parse (www.text);
        Debug.Log("elevation_data " + elevation_data.ToString());
		// loop over results
		for (int i = 0; i < elevation_data.Count; i++)
		{
            Debug.Log(elevation_data[i]);
            for (int j = 0; j < elevation_data[i].Count; j++)
            {
                Debug.Log("i = " + i);
                // record the heights
                vertexGrid[i, j] = elevation_data.AsArray[i].AsArray[j].AsFloat;
            }
        }
        
        TerrainData terrainData = new TerrainData();
        terrainData.SetHeights(0, 0, vertexGrid);
        Terrain.CreateTerrainGameObject(terrainData);

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
