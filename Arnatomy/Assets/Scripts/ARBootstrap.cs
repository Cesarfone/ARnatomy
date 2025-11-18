using UnityEngine;

public class ARBootstrap : MonoBehaviour
{
    public ARSpawner spawner;
    public ARTouchManipulator touchManipulator;

    void Start()
    {
        // tenta vincular depois de um pequeno atraso para garantir instância
        Invoke(nameof(Wire), 0.2f);
    }

    void Wire()
    {
        var skeleton = GameObject.Find("Esqueleto_AR");
        if (skeleton && touchManipulator)
            touchManipulator.targetRoot = skeleton.transform;
    }
}
