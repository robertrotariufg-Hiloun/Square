using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private Transform[] layers = new Transform[12];
    [SerializeField] private float[] parallaxFactors = new float[12];

    private Transform cam;
    private Vector3 lastCamPosition;

    void Awake()
    {
        cam = GameObject.Find("Main Camera").transform;
        lastCamPosition = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPosition;

        for (int i = 0; i < layers.Length; i++)
        {
            float factor = parallaxFactors[i];
            layers[i].position += new Vector3(delta.x * factor, delta.y * factor, 0f);
        }

        lastCamPosition = cam.position;
    }
}