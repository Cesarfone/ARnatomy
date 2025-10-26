using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
    public GameObject modelPrefab;         
    private ARRaycastManager raycastManager;
    private GameObject spawnedObject;
    public ConfirmSpawnUI confirmUI;


    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
{

    if (Input.touchCount == 0)
        return;

    Touch touch = Input.GetTouch(0);

    // Faz o Raycast para detectar o plano tocado.
    if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
    {
        // Pega a posição onde o toque acertou o plano.
        var hitPose = hits[0].pose;

        confirmUI.Show(result =>
        {
            if (result) 
            {
                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(modelPrefab, hitPose.position, hitPose.rotation);
              spawnedObject.transform.localScale = Vector3.one * 0.01f;
                    spawnedObject.transform.rotation = Quaternion.Euler(0, 180, 0) * spawnedObject.transform.rotation;
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                }
            }
        });
    }
}

}


