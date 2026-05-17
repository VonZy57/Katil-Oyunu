using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerTester : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("EndlessRunnerTestScene");
        }
    }
}
