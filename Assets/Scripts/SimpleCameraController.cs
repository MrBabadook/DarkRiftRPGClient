using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    public GameObject player;       

    private Vector3 offset; 

    // Use this for initialization
    public void Init(GameObject go)
    {
        player = go;
        offset = transform.position - player.transform.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        transform.position = player.transform.position + offset;
    }
}
