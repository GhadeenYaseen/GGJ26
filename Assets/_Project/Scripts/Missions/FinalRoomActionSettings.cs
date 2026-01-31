using UnityEngine;

public class FinalRoomActionSettings : MonoBehaviour
{
    [Header("Duplicate Character")]
    [SerializeField] private Transform duplicateSpawnPoint;
    [SerializeField] private Vector3 duplicateWorldPosition;
    [SerializeField] private float duplicateYawDegrees = 180f;
    [SerializeField] private bool useSpawnPoint = true;

    [Header("Mirror Move")]
    [SerializeField] private Transform mirrorTransform;
    [SerializeField] private float mirrorMoveDownDistance = 1f;
    [SerializeField] private float mirrorMoveDuration = 1f;
    [SerializeField] private bool mirrorUseLocalSpace = true;

    [Header("Camera Rotate")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraRotateDuration = 1.5f;
    [SerializeField] private float cameraRotateZDegrees = 180f;

    public Transform DuplicateSpawnPoint => duplicateSpawnPoint;
    public Vector3 DuplicateWorldPosition => duplicateWorldPosition;
    public float DuplicateYawDegrees => duplicateYawDegrees;
    public bool UseSpawnPoint => useSpawnPoint;

    public Transform MirrorTransform => mirrorTransform;
    public float MirrorMoveDownDistance => mirrorMoveDownDistance;
    public float MirrorMoveDuration => mirrorMoveDuration;
    public bool MirrorUseLocalSpace => mirrorUseLocalSpace;

    public Transform CameraTransform => cameraTransform;
    public float CameraRotateDuration => cameraRotateDuration;
    public float CameraRotateZDegrees => cameraRotateZDegrees;
}
