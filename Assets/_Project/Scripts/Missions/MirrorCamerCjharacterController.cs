using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorCamerCjharacterController : MonoBehaviour
{
    private void Update()
    {
        transform.forward = Vector3.Lerp(Vector3.right, transform.parent.forward, 0.1f);
    }
}
