using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void SwitchToScene2()
    {
        SceneManager.LoadScene("MainConnect");
    }
}