using Cinemachine;
using UnityEngine;

public class FinalRoomActionSettings : MonoBehaviour
{
    [Header("Duplicate Character")]
    [SerializeField] private Transform[] cloneSpawnPoints;
    [SerializeField] private Vector3[] cloneWorldPositions;
    [SerializeField] private float duplicateYawDegrees = 180f;
    [SerializeField] private bool useSpawnPoints = true;
    [SerializeField] private int cloneCount = 2;

    [Header("Environment Lighting")]
    [SerializeField] private bool disableAmbientLight = true;
    [SerializeField] private Color ambientOffColor = Color.black;
    [SerializeField] private float ambientOffIntensity = 0.5f;
    [SerializeField] private Light[] environmentLightsToDisable;

    [Header("Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float cameraAttachDelay = 2f;
    [SerializeField] private string headTag = "Head";
    [SerializeField] private Vector3 cameraLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 cameraLocalEuler = Vector3.zero;
    [SerializeField] private GameObject[] objectsToActivateWithPrimaryCamera;
    [SerializeField] private CinemachineVirtualCamera secondaryVirtualCamera;
    [SerializeField] private float secondaryCameraDelay = 2f;
    [SerializeField] private bool disablePrimaryWhenSecondaryActive = false;
    [SerializeField] private GameObject[] objectsToActivateWithSecondaryCamera;
    [SerializeField] private int primaryCameraPriority = 20;
    [SerializeField] private int secondaryCameraPriority = 30;
    [SerializeField] private int primaryCameraPriorityWhenSecondaryActive = 0;

    [Header("Typing UI")]
    [SerializeField] private TypewriterPanel typingPanel;
    [SerializeField] private float typingPanelDelay = 0.5f;

    [Header("Animator Override")]
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Deactivate Objects")]
    [SerializeField] private GameObject[] objectsToDeactivate;
    [SerializeField] private GameObject[] selectableObjectsToDeactivate;
    [SerializeField] private FinalRoomMissionTrigger missionTrigger;

    public Transform[] CloneSpawnPoints => cloneSpawnPoints;
    public Vector3[] CloneWorldPositions => cloneWorldPositions;
    public float DuplicateYawDegrees => duplicateYawDegrees;
    public bool UseSpawnPoints => useSpawnPoints;
    public int CloneCount => Mathf.Max(0, cloneCount);

    public bool DisableAmbientLight => disableAmbientLight;
    public Color AmbientOffColor => ambientOffColor;
    public float AmbientOffIntensity => ambientOffIntensity;
    public Light[] EnvironmentLightsToDisable => environmentLightsToDisable;

    public CinemachineVirtualCamera VirtualCamera => virtualCamera;
    public float CameraAttachDelay => cameraAttachDelay;
    public string HeadTag => headTag;
    public Vector3 CameraLocalPosition => cameraLocalPosition;
    public Vector3 CameraLocalEuler => cameraLocalEuler;
    public GameObject[] ObjectsToActivateWithPrimaryCamera => objectsToActivateWithPrimaryCamera;
    public CinemachineVirtualCamera SecondaryVirtualCamera => secondaryVirtualCamera;
    public float SecondaryCameraDelay => secondaryCameraDelay;
    public bool DisablePrimaryWhenSecondaryActive => disablePrimaryWhenSecondaryActive;
    public GameObject[] ObjectsToActivateWithSecondaryCamera => objectsToActivateWithSecondaryCamera;
    public int PrimaryCameraPriority => primaryCameraPriority;
    public int SecondaryCameraPriority => secondaryCameraPriority;
    public int PrimaryCameraPriorityWhenSecondaryActive => primaryCameraPriorityWhenSecondaryActive;
    public TypewriterPanel TypingPanel => typingPanel;
    public float TypingPanelDelay => typingPanelDelay;

    public RuntimeAnimatorController AnimatorController => animatorController;

    public GameObject[] ObjectsToDeactivate => objectsToDeactivate;
    public GameObject[] SelectableObjectsToDeactivate => selectableObjectsToDeactivate;
    public FinalRoomMissionTrigger MissionTrigger => missionTrigger;
}
