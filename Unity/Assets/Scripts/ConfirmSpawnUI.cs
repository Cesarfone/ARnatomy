using UnityEngine;
using UnityEngine.UI;
using System;

public class ConfirmSpawnUI : MonoBehaviour
{
    public GameObject panel;
    private Action<bool> callback;

    public void Show(Action<bool> result)
    {
        panel.SetActive(true);
        callback = result;
    }

    public void OnYes()
    {
        panel.SetActive(false);
        callback?.Invoke(true);
    }

    public void OnNo()
    {
        panel.SetActive(false);
        callback?.Invoke(false);
    }
}
