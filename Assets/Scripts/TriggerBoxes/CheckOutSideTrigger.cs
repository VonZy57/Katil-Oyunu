using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckOutSideTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            EndScene();
    }

    void EndScene()
    {
        MissionObjective missionObj = GetComponent<MissionObjective>();
        //Sonraki g�revi tetikler ve sahne de�i�tirir
        if (missionObj != null)
            missionObj.OnInteracted();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
