using UnityEngine;
using UnityEngine.XR;

// Ce script déplace le rig du drone (position + yaw) via un CharacterController,
// ce qui bloque automatiquement le passage à travers les colliders de la map.
// Il ne touche à la caméra que hors VR (mouse-look desktop) : casque branché,
// la rotation de la caméra reste gérée par le tracking / le simulateur.
[RequireComponent(typeof(CharacterController))]
public class DroneController : MonoBehaviour
{
    [SerializeField] private Transform rig;          // objet parent : contient le mesh du drone ET le XR Origin
    [SerializeField] private Transform visualMesh;    // le mesh du drone uniquement, pour l'inclinaison visuelle
    [SerializeField] private Transform desktopCamera; // caméra pilotée au regard souris, hors VR uniquement
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float yawSpeed = 60f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float tiltAmount = 10f;
    [SerializeField] private float tiltSmoothing = 5f;

    private CharacterController controller;
    private float currentTiltX = 0f;
    private float currentTiltZ = 0f;
    private float currentPitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (!XRSettings.isDeviceActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // --- Regard souris (yaw + pitch façon FPS), desktop uniquement ---
        // En VR, XRSettings.isDeviceActive est vrai et c'est le tracking du casque
        // qui pilote la vue : on laisse la caméra tranquille.
        if (!XRSettings.isDeviceActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            rig.Rotate(0, mouseX, 0);

            if (desktopCamera != null)
            {
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
                currentPitch = Mathf.Clamp(currentPitch - mouseY, minPitch, maxPitch);
                desktopCamera.localRotation = Quaternion.Euler(currentPitch, 0, 0);
            }
        }

        // --- Rotation horizontale (yaw) au clavier : Q/E ---
        float yawInput = 0f;
        if (Input.GetKey(KeyCode.Q)) yawInput = -1f;
        if (Input.GetKey(KeyCode.E)) yawInput = 1f;
        rig.Rotate(0, yawInput * yawSpeed * Time.deltaTime, 0);

        // --- Déplacement horizontal (ZQSD/WASD) relatif à l'orientation du rig ---
        float strafe = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");
        Vector3 horizontalMove = rig.TransformDirection(new Vector3(strafe, 0, forward)) * moveSpeed;

        // --- Déplacement vertical (Espace / Ctrl) ---
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;
        if (Input.GetKey(KeyCode.LeftControl)) verticalInput = -1f;
        Vector3 verticalMove = Vector3.up * verticalInput * verticalSpeed;

        // --- Un seul appel Move() par frame : le CharacterController stoppe au contact des colliders ---
        controller.Move((horizontalMove + verticalMove) * Time.deltaTime);

        // --- Inclinaison visuelle du mesh du drone selon le mouvement (purement cosmétique) ---
        if (visualMesh != null)
        {
            float targetTiltZ = -strafe * tiltAmount;
            float targetTiltX = forward * tiltAmount;
            currentTiltX = Mathf.Lerp(currentTiltX, targetTiltX, Time.deltaTime * tiltSmoothing);
            currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, Time.deltaTime * tiltSmoothing);
            visualMesh.localRotation = Quaternion.Euler(currentTiltX, 0, currentTiltZ);
        }
    }
}
