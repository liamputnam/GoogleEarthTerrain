using UnityEngine;
using System.Collections;
using SimpleJSON;

public class GETerrain
{
    public Vector2 worldPos;
    public int radius = 2;

    private float[,] heights;

    private string urlStem = "https://maps.googleapis.com/maps/api/elevation/json";

    private float km_per_degree_latitude = 111.2f;
	
    private float[,] vertexGrid;

    public GETerrain(Vector2 _worldPos, int _radius)
    {
        worldPos = _worldPos;
		radius = _radius;
		vertexGrid = new float[2 * radius, 2 * radius];
		for (int i = 0; i < 2 * radius; i++)
		{
			for (int j = 0; j < 2 * radius; j++)
			{
				vertexGrid[i,j] = Mathf.Infinity;
			}
		}
    }

    public void CreateTerrain()
    {
        TerrainData terrainData = new TerrainData();
		GetVertexGrid (worldPos);
		terrainData.SetHeights(0, 0, vertexGrid);
        Terrain.CreateTerrainGameObject(terrainData);
    }

    private IEnumerator GetVertexRow(Vector2 start, Vector2 end, int index)
    {
        string path = "path=" + start[0] + "," + start[1] + "|" + end[0] + "," + end[1];
        string samples = "samples=" + radius * 2;
        string url = urlStem + "?" + path + "&" + samples;

        WWW www = new WWW(url);

        yield return www;

        if(www.error == null)
        {
			Debug.Log("Request completed");
            //request completed!
        }
        else
        {
			Debug.Log ("WWW Error" + www.error); 
        }

		JSONNode elevation_data = SimpleJSON.JSONNode.Parse (www.text);

		// loop over results
		for (int i = 0; i < elevation_data["results"].Count; i++)
		{
			// record the heights
			vertexGrid[index, i] = elevation_data["results"][i]["elevation"].AsFloat;
		}
	}

    
	// get a 2d-grid of vertices around the center
	private IEnumerator GetVertexGrid(Vector2 center)
	{
		
		// latitudes are between -90 and 90
		float top_latitude    = ((center[0] + (radius / km_per_degree_latitude) + 90) % 180) - 90;
		float bottom_latitude = ((center[0] - (radius / km_per_degree_latitude) + 90) % 180) - 90;
		
		// longitudes are between 0 and 180
		float right_longitude = center[1] + (radius / km_per_degree_latitude) % 180;
		float left_longitude  = center[1] - (radius / km_per_degree_latitude) % 180;
		
		// get distance between grid bounds
		//float dy = (top_latitude - bottom_latitude) / radius / 2;
		float dx = ((right_longitude - left_longitude) % 180) / radius / 2;
		//print('dy = %f dx = %f' % (dy, dx));
		
		// loop over rows	
		for (var i = 0; i < vertexGrid.Length; i++)
		{
			var row_latitude = top_latitude - (i * dx);
			GetVertexRow(new Vector2(row_latitude, left_longitude), 
			             new Vector2(row_latitude, right_longitude), i);
			while (vertexGrid[i,(2 * radius) - 1] == Mathf.Infinity)
			{
				Debug.Log("vertex_grid[" + i + "] not set. Waiting");
				yield return new WaitForSeconds(1);
			} 
		}
		
	}
	

}