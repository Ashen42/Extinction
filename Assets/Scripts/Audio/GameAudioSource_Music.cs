using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioSource_Music : MonoBehaviour {


    //support for background song on loop in a way that leaves the GameAudioSource open (manually that is ;))
    AudioSource audioSource;
    bool initialized = false;

    private void Awake()
    {
        if (!initialized)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
            initialized = true;
        }
        //duplicate game object so destroy this one
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource.clip = Resources.Load<AudioClip>("Music1");
        audioSource.loop = true;
        audioSource.Play();
    }

    private void Update()
    {
        audioSource.volume = PlayerPrefs.GetFloat("MusicVolume");
    }
}
