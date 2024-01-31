using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public float delay = 5f; // Time in seconds before the scene changes
    public string nextSceneName = "NextScene"; // The name of the next scene to load

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ChangeSceneAfterDelay());
    }

    IEnumerator ChangeSceneAfterDelay()
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        SceneManager.LoadScene(nextSceneName); // Load the next scene
    }
}