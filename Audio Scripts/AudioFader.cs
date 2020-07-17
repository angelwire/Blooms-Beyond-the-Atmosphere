using System.Collections;
using UnityEngine;

/*
 * Helper class to fade sound effects in and out
 * Author William Jones
 * 12-31-19
 */
public static class AudioFader
{
    private static bool shouldStopIn = false;
    private static bool shouldStopOut = false;
    //Fades out the given sound effect
    public static IEnumerator FadeOut(AudioSource source, float time)
    {
        shouldStopOut= false;
        float startingVolume = source.volume;

        while (source.volume > 0 && !shouldStopOut)
        {
            source.volume -= startingVolume * Time.deltaTime / time;
            yield return null;
        }
        if (source.volume < .1f)
        {
            source.Stop();
        }
        source.volume = startingVolume;
    }

    //Fades in the given sound effect
    public static IEnumerator FadeIn(AudioSource source, float time, float volume)
    {
        shouldStopIn = false;
        source.volume = 0;
        source.Play();
        while (source.volume < volume && !shouldStopIn)
        {
            source.volume += volume * Time.deltaTime / time;

            if (source.volume > volume)
            {
                source.volume = volume;
            }
            yield return null;
        }
    }

    //Coroutine stopper methods
    public static void StopFadingIn()
    {
        shouldStopIn = true;
    }

    public static void StopFadingOut()
    {
        shouldStopOut = true;
    }


}
