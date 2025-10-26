using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadNext());
    }

    IEnumerator LoadNext()
    {
        yield return new WaitForSeconds(2f); // tempo do loading
        SceneManager.LoadScene("MainScene"); // nome da sua cena principal
    }
}
