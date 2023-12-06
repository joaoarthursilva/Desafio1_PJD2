using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    [SerializeField] private Transform cam;

    private void Start()
    {
        cam = transform;
    }

    public void SetTarget(Transform target1)
    {
        target = target1;
    }

    private void LateUpdate()
    {
        cam.position = target.position;
    }
}