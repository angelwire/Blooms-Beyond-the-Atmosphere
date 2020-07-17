using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HomeInventoryData : MonoBehaviour
{
    public static HomeInventoryData instance;
    public Dictionary<uint, int> homeFlowerInventory;
    public Dictionary<uint, int> homeDecorationInventory;
    public Dictionary<uint, int> homeDecorationsBought;
    public List<uint> challangeFlowerIds;

    public int challangeFlowerValue = 500;

    public bool[,] knownParts = new bool[4,16];
    public bool[,] knownColors = new bool[4,16];

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        GameObject.DontDestroyOnLoad(gameObject);
        homeFlowerInventory = new Dictionary<uint, int>();
        /*for (int ii = 0; ii < 6; ii += 1)
        {
            homeFlowerInventory.Add((uint)ii * 324 * 2134 * 2342 * 235, Random.Range(1, 10));
        }*/
        homeDecorationInventory = new Dictionary<uint, int>();
        homeDecorationsBought = new Dictionary<uint, int>();
        challangeFlowerIds = new List<uint>();


        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                knownParts[i, j] = false;
                knownColors[i, j] = false;
            }
        }

        if (PlayerPrefs.HasKey("homeFlowerInventory_keys"))
        {
            load();
        }
        else
        {
            /*homeDecorationInventory.Add(0, 5);
            homeDecorationInventory.Add(1, 5);
            homeDecorationInventory.Add(2, 5);
            homeDecorationInventory.Add(3, 5);*/
        }
    }

    public void addToInventory(Placeable p)
    {
        if (p.isFlower)
        {
            if (homeFlowerInventory.ContainsKey(p.id))
            {
                homeFlowerInventory[p.id]++;
            }
            else
            {
                homeFlowerInventory.Add(p.id, 1);
            }
            
        }
        else
        {
            if (homeDecorationInventory.ContainsKey(p.id))
            {
                homeDecorationInventory[p.id]++;
            }
            else
            {
                homeDecorationInventory.Add(p.id, 1);
            }
        }
    }

    public void addToKnownFlowers(string id)
    {
        for (int i = 0; i < 4; i++)
        {
            knownParts[i, Convert.ToInt32("" + id[i*2], 16)] = true;
            knownColors[i, Convert.ToInt32("" + id[(i*2)+1], 16)] = true;
        }
    }

    public void save()
    {
        saveDict(homeFlowerInventory, "homeFlowerInventory");
        saveDict(homeDecorationInventory, "homeDecorationInventory");
        saveDict(homeDecorationsBought, "homeDecorationsSold");
        string knownPartsS = "";
        string knownColorsS = "";
        for (int i = 0; i < 4; i++)
        {
            for (int k = 0; k < 16; k++)
            {
                knownPartsS += knownParts[i, k] ? '1' : '0';
                knownColorsS += knownColors[i, k] ? '1' : '0';
            }
        }
        PlayerPrefs.SetString("knownParts", knownPartsS);
        PlayerPrefs.SetString("knownColors", knownColorsS);
        string toSet = "";
        for (int i = 0; i < challangeFlowerIds.Count; i++)
        {
            toSet += challangeFlowerIds[i];

            if (i != challangeFlowerIds.Count - 1)
            {
                toSet += "|";
            }
        }
        PlayerPrefs.SetString("challengeFlower", toSet);
    }

    public void load()
    {
        Debug.Log("Loading in HomeInventoryData...");

        loadDict(homeFlowerInventory, "homeFlowerInventory");
        loadDict(homeDecorationInventory, "homeDecorationInventory");
        loadDict(homeDecorationsBought, "homeDecorationsSold");


        if (PlayerPrefs.HasKey("knownParts"))
        {
            string knownPartsS = PlayerPrefs.GetString("knownParts");
            string knownColorsS = PlayerPrefs.GetString("knownColors");
            for (int i = 0; i < 4; i++)
            {
                for (int k = 0; k < 16; k++)
                {
                    knownParts[i, k] = (knownPartsS[k + (i * 16)] == '1');
                    knownColors[i, k] = (knownColorsS[k + (i * 16)] == '1');
                }
            }
        }
        if (PlayerPrefs.HasKey("challengeFlower") && PlayerPrefs.GetString("challengeFlower")!="")
        {
            string[] processed = PlayerPrefs.GetString("challengeFlower").Split('|');
            for (int i = 0; i < processed.Length; i++)
            {
                challangeFlowerIds.Add(uint.Parse(processed[i]));
            }
        }
    }
    private void loadDict(Dictionary<uint, int> toLoad, String name)
    {
        if (PlayerPrefs.GetString(name + "_keys") != "")
        {
            string[] keys = PlayerPrefs.GetString(name + "_keys").Split('|');
            string[] values = PlayerPrefs.GetString(name + "_values").Split('|');
            for (int i = 0; i < keys.Length; i++)
            {
                toLoad.Add(Convert.ToUInt32(keys[i], 16), int.Parse(values[i]));
            }
        }
    }

    private void saveDict(Dictionary<uint, int> toSave, String name)
    {
        List<uint> keys = toSave.Keys.ToList();
        string toReturn = "";
        for (int i = 0; i < keys.Count; i++)
        {
            toReturn += keys[i].ToString("X8");
            if (i != keys.Count - 1)
            {
                toReturn += "|";
            }
        }
        PlayerPrefs.SetString(name + "_keys", toReturn);

        List<int> values = toSave.Values.ToList(); toReturn = "";
        for (int i = 0; i < keys.Count; i++)
        {
            toReturn += values[i];
            if (i != values.Count - 1)
            {
                toReturn += "|";
            }
        }
        PlayerPrefs.SetString(name + "_values", toReturn);

    }

    public void resetSave()
    {
        homeFlowerInventory = new Dictionary<uint, int>();
        homeDecorationInventory = new Dictionary<uint, int>();
        homeDecorationsBought = new Dictionary<uint, int>();
        challangeFlowerIds = new List<uint>();


        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                knownParts[i, j] = false;
                knownColors[i, j] = false;
            }
        }

    }
}
