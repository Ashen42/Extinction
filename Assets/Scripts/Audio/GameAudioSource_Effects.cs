using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioSource_Effects : MonoBehaviour {

    AudioSource audioSource;

    private void Awake()
    {
        if (!AudioManager.Initialized)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            AudioManager.Initialize(audioSource);
            DontDestroyOnLoad(gameObject);
        }
        //duplicate game object so destroy this one
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        audioSource.volume = PlayerPrefs.GetFloat("SoundVolume");
    }




}
