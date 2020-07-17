using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameJoltAPIHelper : MonoBehaviour
{
    public static string errorText = "ERROR";
    public static string flowers_planted = "flowers_planted";
    public static string flowers_picked = "flowers_picked";
    public static string flowers_sold = "flowers_sold";
    public static string return_flowers_planted;
    public static string return_flowers_picked;
    public static string return_flowers_sold;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (!PlayerPrefs.HasKey("auth"))
        {
            PlayerPrefs.SetString("auth", Random.Range(int.MinValue, int.MaxValue).ToString("X8") + Random.Range(int.MinValue, int.MaxValue).ToString("X8")+ Random.Range(int.MinValue, int.MaxValue).ToString("X8") + Random.Range(int.MinValue, int.MaxValue).ToString("X8"));
        }
    }

    public static void refresh()
    {
        GameJolt.API.DataStore.Get(flowers_planted, true, value => {
            if (value != null)
            {
                GameJoltAPIHelper.return_flowers_planted = value;
            }
            else
            {
                GameJoltAPIHelper.return_flowers_planted = errorText;
            }
        });
        GameJolt.API.DataStore.Get(flowers_picked, true, value => {
            if (value != null)
            {
                GameJoltAPIHelper.return_flowers_picked = value;
            }
            else
            {
                GameJoltAPIHelper.return_flowers_picked = errorText;
            }
        });
        GameJolt.API.DataStore.Get(flowers_sold, true, value => {
            if (value != null)
            {
                GameJoltAPIHelper.return_flowers_sold = value;
            }
            else
            {
                GameJoltAPIHelper.return_flowers_sold = errorText;
            }
        });
    }

    /// <summary>
    /// Gets any data store key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetDataStoreKey(string key)
    {
        string toReturn = errorText;
        GameJolt.API.DataStore.Get(key, true, value => {
            if (value != null)
            {
                toReturn = value;
            }
        });
        return toReturn;
    }

    /// <summary>
    /// Increases any datastore key by 1
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string IncDataStoreKey(string key)
    {
        string toReturn = errorText;
        Debug.Log("Update DataStore Key. Click to see source.");

        GameJolt.API.DataStore.Update(key, "1", GameJolt.API.DataStoreOperation.Add, true, value => {
            if (value != null)
            {
                toReturn = "SUCCESS";
            }
        });
        return toReturn;
    }

    public static void DownloadWorld(string key)
    {
    }

    public static void SetDataStoreKey(string key, string data)
    {
        // for testing: limit the upload size
        // if the data is larger than 50 bytes, it will be split into 5 packages
        int uploadSize = 50;
        if (data.Length > uploadSize) uploadSize = data.Length / 5 + 1;
        GameJolt.API.DataStore.SetSegmented(key, data, true, success => { }, progress => { }, uploadSize);
    }

}
