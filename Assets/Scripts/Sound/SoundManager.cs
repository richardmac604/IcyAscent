using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager {

    public enum Sound {
        None,
        IceHit,
        MetalHit,
        RockHit,
        SnowHit,
        WoodHit,
    }

    public static void PlaySound(Sound sound) {
        GameObject soundGameObject = new GameObject("Sound");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        AudioClip clip = GetAudioClip(sound);

        if (clip == null) {
            Debug.LogError($"Sound clip for {sound} not found!");
            GameObject.Destroy(soundGameObject);
            return;
        }

        audioSource.PlayOneShot(clip);

        // Schedule the destruction of the GameObject after the clip's length
        GameObject.Destroy(soundGameObject, clip.length);
    }

    private static AudioClip GetAudioClip(Sound sound) {

        if (sound == Sound.None) return null;

        foreach (SoundAssets.SoundAudioClip soundAudioClip in SoundAssets.Instance.soundAudioClipArray) {
            if (soundAudioClip.sound == sound) {
                return soundAudioClip.audioClip;
            }
        }
        Debug.LogError($"Sound: {sound} not found!");
        return null;
    }

}