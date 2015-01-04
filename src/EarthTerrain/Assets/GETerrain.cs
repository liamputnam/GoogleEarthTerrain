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
        StartCoroutine(GetVertexGrid(worldPos));
    }

    private IEnumerator GetVertexRow(Vector2 start, Vector2 end, int index)
    {
        Debug.Log("GetVertexRow");
        Debug.Log("Start: " + start + "| " + "End: " + end + "| " + "Index: " + index);
        string path = "path=" + start[0] + "," + start[1] + "|" + end[0] + "," + end[1];
        string samples = "samples=" + radius * 2;
        string url = urlStem + "?" + path + "&" + samples;// + "&key=AIzaSyAF4Bkkocrsf9tbab2UDsUfP8P5mSUpupc";

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
        Debug.Log("elevation_data " + elevation_data["results"].ToString());
        Debug.Log(2 * radius - 1);
		// loop over results
		for (int i = 0; i < elevation_data["results"].Count; i++)
		{
            Debug.Log("i = " + i);
			// record the heights
			vertexGrid[index, i] = elevation_data["results"][i]["elevation"].AsFloat;
            Debug.Log("height: " + vertexGrid[index, i]);
        }
	}
    
	// get a 2d-grid of vertices around the center
	private IEnumerator GetVertexGrid(Vector2 center)
    {
        Debug.Log("GetVertexGrid(" + center + ")"); 
		// latitudes are between -90 and 90
		float top_latitude    = ((center[0] + (radius / km_per_degree_latitude) + 90) % 180) - 90;
		float bottom_latitude = ((center[0] - (radius / km_per_degree_latitude) + 90) % 180) - 90;
		
		// longitudes are between 0 and 180
		float right_longitude = center[1] + (radius / km_per_degree_latitude) % 180;
		float left_longitude  = center[1] - (radius / km_per_degree_latitude) % 180;

        Debug.Log("Right Longitude: " + right_longitude);
		
		// get distance between grid bounds
		//float dy = (top_latitude - bottom_latitude) / radius / 2;
		float dx = ((right_longitude - left_longitude) % 180) / radius / 2;
		//print('dy = %f dx = %f' % (dy, dx));

		// loop over rows	
		for (int i = 0; i < 2 * radius; i++)
		{
            Debug.Log("loop " + i);
			float row_latitude = top_latitude - (i * dx);
            yield return StartCoroutine(GetVertexRow(new Vector2(row_latitude, left_longitude),
                                                       new Vector2(row_latitude, right_longitude), i));
			/*while (vertexGrid[i,(2 * radius) - 1] == Mathf.Infinity)
			{
                if (requestFailed)
                {
                    Debug.Log("request failed");
                    return;
                }

				Debug.Log("vertex_grid[" + i + "] not set. Waiting");
                wait(1);
			} */
		}
        StartCoroutine(wait(3));
        Debug.Log("Gog here");
        TerrainData terrainData = new TerrainData();
        terrainData.SetHeights(0, 0, vertexGrid);
        Terrain.CreateTerrainGameObject(terrainData);	
	}

    private IEnumerator wait(int seconds)
    {
        yield return new WaitForSeconds(seconds);
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
