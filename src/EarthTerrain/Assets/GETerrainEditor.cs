using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GETerrain))]
public class GETerrainEditor : Editor
{
    private Vector2 worldPosition;
    private int radius;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Terrain Creator");

        EditorGUILayout.Vector2Field("Latitude and Longitude", worldPosition);

        EditorGUILayout.IntField("Radius", radius);

        if (GUILayout.Button("Create Terrain"))
        {
            if (radius == 0)
            {
                Debug.LogWarning("Please fill out all the world information!");
                worldPosition = Vector2.zero;
                radius = 1;
            }

            GameObject obj = new GameObject();
            GETerrain geTerrain = obj.AddComponent<GETerrain>();
            geTerrain.SetWorldPosition(worldPosition);
            geTerrain.SetRadius(radius);
            geTerrain.ClearVertexGrid();
            geTerrain.CreateTerrain();
            //DestroyImmediate(obj);
        }
    }
}
