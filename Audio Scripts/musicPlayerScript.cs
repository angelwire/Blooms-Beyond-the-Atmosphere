using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/* - - - -
 * A class to play background music
 * Last updated: 12-28-19
 * Author William Jones
  - - - - */

public class musicPlayerScript : MonoBehaviour
{
    [SerializeField] AudioSource currentlyPlayingMusic;
    [SerializeField] AudioSource fadeToMusic;
    [SerializeField] AudioMixer mixer;
    float muffleAmount = 0.0f;
    float defaultFadeSpeed = 1.0f;
    bool isFading = false;
    public static musicPlayerScript instance;
    string currentlyPlayingTag = "none";

    float minCutoff = 700;
    float maxCutoff = 6000;

    int tryAgainCount = 0;
    float tryAgainTimer = 0;
    float tryAgainDelay = .5f;
    string tryAgainTag = "";
    bool doTryAgain = false;

    // Start is called before the first frame update
    void Start()
    {
        //Make sure this object won't be destroyed
        GameObject.DontDestroyOnLoad(gameObject);

        //Destroy itself if there's another BackrgroundMusicController
        if (musicPlayerScript.instance != null)
        {
            GameObject.DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        SceneManager.sceneLoaded += setupPlanetMusic;
    }

    private void setupPlanetMusic(Scene scene, LoadSceneMode load)
    {
        if (scene.name != "HomeScreenScene" && scene.name != "titleScene")
        {
            switch(WarpDriveController.instance.GetDestinationPlanet().getType())
            {
                case Planet.PlanetType.HOME: playSong("day"); break;
                case Planet.PlanetType.HOT: playSong("hot"); break;
                case Planet.PlanetType.COLD: playSong("cold"); break;
                case Planet.PlanetType.NEON: playSong("neon"); break;
            }
        }
        if (scene.name == "titleScene")
        {
            playSong("title");
        }
    }

    //Play a song with the given tag
    void playSong(string tag)
    {
        /* Tags:
         * "title" - The song that plays at the title screen
         * "day" - The song that plays on the home planet during the day
         * "night" - The song that plays on the home planet during the night
         * TODO add more music
         */
        if (!isFading)
        {
            AudioClip clipToPlay = null;
            switch (tag)
            {
                case "title": clipToPlay = (Resources.Load("Music/" + "reachForTheStars") as AudioClip); break;
                case "day": clipToPlay = (Resources.Load("Music/" + "followThatDream") as AudioClip); break;
                case "night": clipToPlay = (Resources.Load("Music/" + "almostNew") as AudioClip); break;
                case "neon": clipToPlay = (Resources.Load("Music/" + "dropsOfGlas") as AudioClip); break;
                case "hot": clipToPlay = (Resources.Load("Music/" + "savannah") as AudioClip); break;
                case "cold": clipToPlay = (Resources.Load("Music/" + "paperFlakes") as AudioClip); break;
                case "space": clipToPlay = (Resources.Load("Music/" + "slowingDown") as AudioClip); break;
            }

            //If there is a clip to play and it's not currently playing
            if (currentlyPlayingTag != tag)
            {
                if (clipToPlay != null)
                {
                    //Set the current tag
                    currentlyPlayingTag = tag;
                    //Play the new music
                    StartCoroutine(CrossFadeMusic(clipToPlay, 1.0f));

                    doTryAgain = false;
                    tryAgainCount = 0;
                }
                else
                {
                    Debug.LogError("No clip found for tag: " + tag);
                    if (tryAgainCount < 2)
                    {
                        tryAgainTimer = 0;
                        tryAgainTag = tag;
                        doTryAgain = true;
                        tryAgainCount += 1;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Cannot play music while fading between songs");
        }
    }

    //Cross fade the music with a coroutine
    private IEnumerator CrossFadeMusic(AudioClip clip, float speed)
    {
        isFading = true;

        fadeToMusic.clip = clip;
        fadeToMusic.volume = 0;
        fadeToMusic.Play();

        float scaledRate = defaultFadeSpeed * speed;
        while (currentlyPlayingMusic.volume > 0)
        {
            currentlyPlayingMusic.volume -= scaledRate * Time.deltaTime;
            fadeToMusic.volume += scaledRate * Time.deltaTime;

            yield return null;
        }

        AudioSource temp = currentlyPlayingMusic;

        currentlyPlayingMusic = fadeToMusic;

        fadeToMusic = temp;
        fadeToMusic.Stop();

        isFading = false;
    }

    //Sets the muffling for the background music
    public void setMuffle(float muffle)
    {
         mixer.SetFloat("cutoff", Mathf.Lerp(maxCutoff, minCutoff, muffle));
    }

    //Fades out the music
    public void fadeOutMusic(float speed)
    {
        StartCoroutine(CrossFadeMusic(null, speed));
    }

    public void Update()
    {
        if (doTryAgain)
        {
            if (tryAgainTimer > tryAgainDelay)
            {
                playSong(tryAgainTag);
                doTryAgain = false;
            }
            else
            {
                tryAgainTimer += Time.deltaTime;
            }
        }
    }
}
