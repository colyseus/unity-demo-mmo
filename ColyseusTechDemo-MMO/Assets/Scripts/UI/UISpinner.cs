using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpinner : MonoBehaviour
{
    [SerializeField]
    private float spinSpeed = 5.0f;

    [SerializeField]
    private Transform spinTransform;

    // Update is called once per frame
    void Update()
    {
        spinTransform.Rotate(Vector3.forward, Time.deltaTime * spinSpeed);
    }
}
