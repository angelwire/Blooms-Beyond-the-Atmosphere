/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Chunk))]
public class NormalsVisualization : MonoBehaviour 
{
    public bool drawNormals;
    [SerializeField][Range(.1f,2)]
    private float lineLength;
    [SerializeField] [Range(.1f, 1)]
    private float vertexSize;
    private MeshFilter debugMesh;

    // Start is called before the first frame update
    void Start()
    {
        debugMesh = GetComponent<MeshFilter>();
    }

    private void OnDrawGizmosSelected()
    {
        if (drawNormals && debugMesh!=null)
        {
            Vector3[] vertices = debugMesh.mesh.vertices;
            Vector3[] normals = debugMesh.mesh.normals;
            float worldCurvature = WorldManager.instance.getWorldCurvature();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertexPosition = vertices[i] + debugMesh.transform.position;
                Vector3 cameraPosition = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
                float zDistance = cameraPosition.z - vertexPosition.z;
                float xDistance = cameraPosition.x - vertexPosition.x;
                float distanceFromPlayer = (zDistance * zDistance) + (xDistance * xDistance);
                vertexPosition.y += distanceFromPlayer * - worldCurvature;

                if (!((vertices[i].x + 5) % 1 < .1f || (vertices[i].x + 5) % 1 > .9f))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.blue;
                }
                Gizmos.DrawCube(vertexPosition, Vector3.one * vertexSize);

                Gizmos.color = Color.Lerp(Color.white,Color.black,Vector3.Dot(Vector3.up,normals[i]));
                Gizmos.DrawLine(vertexPosition, vertexPosition + (normals[i] * lineLength));
            }
        }
    }
}
*/