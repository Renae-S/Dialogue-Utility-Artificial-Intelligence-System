using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            if (SceneManager.GetActiveScene().name == "House")
                SceneManager.LoadScene("Overworld");
            else if (SceneManager.GetActiveScene().name == "Overworld")
                SceneManager.LoadScene("House");
    }
}
