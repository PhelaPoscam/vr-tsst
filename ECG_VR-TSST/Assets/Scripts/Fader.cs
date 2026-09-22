using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class Fader : MonoBehaviour
{

    public enum VisualFadeType
    {
        Start,
        View
    }

    private static Fader s_instance;
    public static Fader Instance
    {
        get
        {
            return s_instance;
        }
    }

    [SerializeField]
    private AudioMixer _audio;

    [SerializeField]
    private string _volumePropertyName = "MasterVolume";

    private float _initialAudioVolume;
    private bool _hasVolumeProperty;

    private void Awake()
    {
        if (_audio != null)
        {
            _hasVolumeProperty = _audio.GetFloat(_volumePropertyName, out _initialAudioVolume);
        }
        if (!s_instance)
        {
            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(this);
    }

    public void Fade(Color color, float time, bool fadeIn = true, VisualFadeType type = VisualFadeType.Start)
    {
        if (type == VisualFadeType.Start) ScreenFader.Fade(color, time);
        //else if (type == VisualFadeType.View) SteamVR_Fade.View(color, time); //removed because never used
        if (_audio != null && _hasVolumeProperty)
        {
            StartCoroutine(FadeAudio(time, fadeIn));
        }
    }

    private IEnumerator FadeAudio(float time, bool fadeIn)
    {
        float timer, targetVolume, currentVolume, delta;
        timer = time;
        _audio.GetFloat(_volumePropertyName, out currentVolume);
        targetVolume = fadeIn ? _initialAudioVolume : -50f;
        delta = targetVolume - currentVolume;
        while (timer > 0)
        {
            currentVolume += delta / time * Time.deltaTime;
            _audio.SetFloat(_volumePropertyName, currentVolume);
            timer -= Time.deltaTime;
            yield return null;
        }
        _audio.SetFloat(_volumePropertyName, targetVolume);
    }

}
