using UnityEngine;

// Interaction souris desktop sur les ResultPanel muraux : au clic gauche, lance un
// raycast depuis la caméra et appelle ResultPanel.ShowDetails() si elle touche un
// panneau. Complète l'interaction XR (XR Simple Interactable posé sur le prefab
// ResultPanel, event Select Entered -> ResultPanel.ShowDetails(), configuré côté
// Inspector) exigée par l'énoncé en plus de la souris.
public class MouseClickInteractor : MonoBehaviour
{
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private float maxDistance = 100f;

    private void Awake()
    {
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || raycastCamera == null)
            return;

        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance) &&
            hit.collider.TryGetComponent(out ResultPanel resultPanel))
        {
            resultPanel.ShowDetails();
        }
    }
}
