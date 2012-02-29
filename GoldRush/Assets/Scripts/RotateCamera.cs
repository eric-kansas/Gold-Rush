using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {

    public Transform target;
    public float edgeBorder = 0.1f;
    public float horizontalSpeed = 360.0f;
    public float verticalSpeed = 360.0f;
    public float minVertical = 20.0f;
    public float maxVertical = 85.0f;

    private float x = 0.0f;
    private float y = 0.0f;
    private float distance;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
