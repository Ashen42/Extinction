using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager
{

    #region fields

    static bool initialized = false;
    static AudioSource audioSource;
    static Dictionary<AudioClipName, AudioClip> audioClips = new Dictionary<AudioClipName, AudioClip>();

    #endregion

    #region properties

    public static bool Initialized
    {
        get { return initialized; }
    }

    #endregion

    #region methods

    public static void Initialize(AudioSource source)
    {
        initialized = true;
        audioSource = source;
        audioClips.Add(AudioClipName.MassExtinction, Resources.Load<AudioClip>("MassExtinction"));
    }

    public static void Play(AudioClipName name)
    {
        audioSource.PlayOneShot(audioClips[name]);
    }

    public static int GetLength(AudioClipName name)
    {
        return (int)audioClips[name].length;
    }



    #endregion

}


