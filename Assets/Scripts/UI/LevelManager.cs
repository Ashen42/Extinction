using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    private AudioSource audioSource;

    private void Start() {
        audioSource = GameObject.FindObjectOfType<AudioSource>();
    }

    public void LoadLevel(string name) {
        Cursor.visible = true;
        Debug.Log("Level load request for: " + name);
        //Application.LoadLevel(name);

        if (name == "Level") {
            AudioManager.Play(AudioClipName.MassExtinction);
        } else if (name == "Start") {
            // no music (yet)
        }

        SceneManager.LoadScene(name);
    }

    public void LoadNextLevel(){
        Cursor.visible = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +1);
        // see SceneManager.LoadScene https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadScene.html
        // see SceneManager Unity Class https://docs.unity3d.com/ScriptReference/30_search.html?q=SceneManager
    }

    public void QuitRequest() {
        Debug.Log("Quit Requested");
        Application.Quit();
    }


    private void levelCleanup() {
        
    }
}