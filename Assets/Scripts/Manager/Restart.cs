using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Restart : MonoBehaviour
{
    private void Update()
        {
            if (Input.GetKey("r"))
            {RestartGame();}
        }

    public void RestartGame()
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

}