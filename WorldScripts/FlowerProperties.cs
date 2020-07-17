//updated: 11-24-19
//Author William Jones
//version: .03
/*
 * Helper for flowers, holds a bunch of useful methods and values
 */


using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FlowerProperties : MonoBehaviour
{
    public Color[] flowerColors = new Color[16]; //The colors for the flower elements
    public int elementCount = 11; //How many flower elements there are
    public GameObject[] flowerStem = new GameObject[16]; //The stem game objects
    public GameObject[] flowerPetal = new GameObject[16]; //The petal game objects
    public GameObject[] flowerLeaf = new GameObject[16]; //The leaf game objects
    public GameObject[] flowerPistil = new GameObject[16]; //The pistil game objects

    //The materials
    public Material stemMaterial;
    public Material petalMaterial;
    public Material leafMaterial;
    public Material pistilMaterial;
    public Material flowerMaterial;

    //The decorations
    Dictionary<uint, GameObject> decorationObject = new Dictionary<uint, GameObject>();
    [SerializeField] public Shader decorationUIShader;
    [SerializeField] public Material decorationUIMaterial;
    [SerializeField] public Shader grassUIShader;
    public GameObject growingFlowerEmitter;

    public int[,] availablePlanetPetals = new int[4,4];
    public int[,] availablePlanetPistils = new int[4,4];
    public int[,] availablePlanetLeafs = new int[4,4];
    public int[,] availablePlanetStems = new int[4,4];
    public int[,] availablePlanetPetalColors = new int[4, 4];
    public int[,] availablePlanetStemColors = new int[4, 4];
    public int[,] availablePlanetLeafColors = new int[4, 4];
    public int[,] availablePlanetPistilColors = new int[4, 4];

    public void Start()
    {

        //Petals
            //Home planet petals
            availablePlanetPetals[0, 0] = 1; availablePlanetPetals[0, 1] = 2;
            availablePlanetPetals[0, 2] = 4; availablePlanetPetals[0, 3] = 7;
            //Hot planet petals
            availablePlanetPetals[1, 0] = 5; availablePlanetPetals[1, 1] = 6;
            availablePlanetPetals[1, 2] = 13; availablePlanetPetals[1, 3] = 15;
            //Cold planet petals
            availablePlanetPetals[2, 0] = 3; availablePlanetPetals[2, 1] = 8;
            availablePlanetPetals[2, 2] = 12; availablePlanetPetals[2, 3] = 14;
            //Neon planet petals
            availablePlanetPetals[3, 0] = 0; availablePlanetPetals[3, 1] = 9;
            availablePlanetPetals[3, 2] = 10; availablePlanetPetals[3, 3] = 11;
            //Colors
                //Home planet colors
                availablePlanetPetalColors[0, 0] = 1; availablePlanetPetalColors[0, 1] = 4;
                availablePlanetPetalColors[0, 2] = 12; availablePlanetPetalColors[0, 3] = 10;
                //Hot planet colors
                availablePlanetPetalColors[1, 0] = 2; availablePlanetPetalColors[1, 1] = 5;
                availablePlanetPetalColors[1, 2] = 3; availablePlanetPetalColors[1, 3] = 15;
                //Cold planet colors
                availablePlanetPetalColors[2, 0] = 0; availablePlanetPetalColors[2, 1] = 6;
                availablePlanetPetalColors[2, 2] = 9; availablePlanetPetalColors[2, 3] = 14;
                //Neon planet colors
                availablePlanetPetalColors[3, 0] = 7; availablePlanetPetalColors[3, 1] = 8;
                availablePlanetPetalColors[3, 2] = 11; availablePlanetPetalColors[3, 3] = 13;

        //Pistils
        //Home planet pistils
            availablePlanetPistils[0, 0] = 0; availablePlanetPistils[0, 1] = 1;
            availablePlanetPistils[0, 2] = 3; availablePlanetPistils[0, 3] = 6;
            //Hot planet pistils
            availablePlanetPistils[1, 0] = 7; availablePlanetPistils[1, 1] = 10;
            availablePlanetPistils[1, 2] = 12; availablePlanetPistils[1, 3] = 12;
            //Cold planet pistils
            availablePlanetPistils[2, 0] = 8; availablePlanetPistils[2, 1] = 9;
            availablePlanetPistils[2, 2] = 13; availablePlanetPistils[2, 3] = 13;
            //Neon planet pistils
            availablePlanetPistils[3, 0] = 2; availablePlanetPistils[3, 1] = 4;
            availablePlanetPistils[3, 2] = 5; availablePlanetPistils[3, 3] = 11;
                //Colors
                //Home planet colors
                availablePlanetPistilColors[0, 0] = 1; availablePlanetPistilColors[0, 1] = 2;
                availablePlanetPistilColors[0, 2] = 8; availablePlanetPistilColors[0, 3] = 12;
                //Hot planet colors
                availablePlanetPistilColors[1, 0] = 4; availablePlanetPistilColors[1, 1] = 6;
                availablePlanetPistilColors[1, 2] = 11; availablePlanetPistilColors[1, 3] = 15;
                //Cold planet colors
                availablePlanetPistilColors[2, 0] = 0; availablePlanetPistilColors[2, 1] = 3;
                availablePlanetPistilColors[2, 2] = 5; availablePlanetPistilColors[2, 3] = 7;
                //Neon planet colors
                availablePlanetPistilColors[3, 0] = 9; availablePlanetPistilColors[3, 1] = 10;
                availablePlanetPistilColors[3, 2] = 13; availablePlanetPistilColors[3, 3] = 14;
        //Leafs
            //Home planet leafs
            availablePlanetLeafs[0, 0] = 0; availablePlanetLeafs[0, 1] = 2;
            availablePlanetLeafs[0, 2] = 8; availablePlanetLeafs[0, 3] = 15;
            //Hot planet leafs
            availablePlanetLeafs[1, 0] = 1; availablePlanetLeafs[1, 1] = 3;
            availablePlanetLeafs[1, 2] = 5; availablePlanetLeafs[1, 3] = 10;
            //Cold planet leafs
            availablePlanetLeafs[2, 0] = 4; availablePlanetLeafs[2, 1] = 12;
            availablePlanetLeafs[2, 2] = 13; availablePlanetLeafs[2, 3] = 14;
            //Neon planet leafs
            availablePlanetLeafs[3, 0] = 6; availablePlanetLeafs[3, 1] = 7;
            availablePlanetLeafs[3, 2] = 9; availablePlanetLeafs[3, 3] = 11;
                //Colors
                //Home planet colors
                availablePlanetLeafColors[0, 0] = 1; availablePlanetLeafColors[0, 1] = 5;
                availablePlanetLeafColors[0, 2] = 11; availablePlanetLeafColors[0, 3] = 12;
                //Hot planet colors
                availablePlanetLeafColors[1, 0] = 2; availablePlanetLeafColors[1, 1] = 6;
                availablePlanetLeafColors[1, 2] = 8; availablePlanetLeafColors[1, 3] = 15;
                //Cold planet colors
                availablePlanetLeafColors[2, 0] = 0; availablePlanetLeafColors[2, 1] = 9;
                availablePlanetLeafColors[2, 2] = 10; availablePlanetLeafColors[2, 3] = 14;
                //Neon planet colors
                availablePlanetLeafColors[3, 0] = 3; availablePlanetLeafColors[3, 1] = 4;
                availablePlanetLeafColors[3, 2] = 7; availablePlanetLeafColors[3, 3] = 13;
        //Stems
            //Home planet stems
            availablePlanetStems[0, 0] = 0; availablePlanetStems[0, 1] = 4;
            availablePlanetStems[0, 2] = 9; availablePlanetStems[0, 3] = 11;
            //Hot planet stems
            availablePlanetStems[1, 0] = 1; availablePlanetStems[1, 1] = 7;
            availablePlanetStems[1, 2] = 8; availablePlanetStems[1, 3] = 14;
            //Cold planet stems
            availablePlanetStems[2, 0] = 2; availablePlanetStems[2, 1] = 10;
            availablePlanetStems[2, 2] = 12; availablePlanetStems[2, 3] = 15;
            //Neon planet stems
            availablePlanetStems[3, 0] = 3; availablePlanetStems[3, 1] = 5;
            availablePlanetStems[3, 2] = 6; availablePlanetStems[3, 3] = 13;
                //Home planet colors
                availablePlanetStemColors[0, 0] = 1; availablePlanetStemColors[0, 1] = 2;
                availablePlanetStemColors[0, 2] = 12; availablePlanetStemColors[0, 3] = 14;
                //Hot planet colors
                availablePlanetStemColors[1, 0] = 3; availablePlanetStemColors[1, 1] = 4;
                availablePlanetStemColors[1, 2] = 5; availablePlanetStemColors[1, 3] = 15;
                //Cold planet colors
                availablePlanetStemColors[2, 0] = 0; availablePlanetStemColors[2, 1] = 8;
                availablePlanetStemColors[2, 2] = 10; availablePlanetStemColors[2, 3] = 11;
                //Neon planet colors
                availablePlanetStemColors[3, 0] = 6; availablePlanetStemColors[3, 1] = 7;
                availablePlanetStemColors[3, 2] = 9; availablePlanetStemColors[3, 3] = 13;
    }

    //Helpful helper functions hopefully
    //petal petalcolor stem stemcolor leaf leafcolor pistil pistilcolor
    public int getPetalIndexFromIndex(uint index)
    { return (int)(index >> 28); }
    public int getPetalColorIndexFromIndex(uint index)
    { return (int)((index << 4) >> 28); }
    public int getStemIndexFromIndex(uint index)
    { return (int)((index << 8) >> 28); }
    public int getStemColorIndexFromIndex(uint index)
    { return (int)((index << 12) >> 28); }
    public int getLeafIndexFromIndex(uint index)
    { return (int)((index << 16) >> 28); }
    public int getLeafColorIndexFromIndex(uint index)
    { return (int)((index << 20) >> 28); }
    public int getPistilIndexFromIndex(uint index)
    { return (int)((index << 24) >> 28); }
    public int getPistilColorIndexFromIndex(uint index)
    { return (int)((index << 28) >> 28); }

    /// <summary>
    /// Load the decoration object from the dictionary if possible, if not then load from resources
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameObject getDecorationObject(uint index)
    {
        if (decorationObject.ContainsKey(index)) //If it has already been loaded before
        {
            return decorationObject[index]; 
        }
        else //If not then load it
        {
            return loadDecorationObject(index);
        }
    }

    /*
     * Decoration ID codes:
     * 0 ... 9: Japanese style path, bench, fence, archway, and then some extra IDs for padding in case more need added
     * 10...19: Urban style path, bench, fence, archway...
     * 20...29: Suburban style- path, bench, fence, archway...
     * 30...99: Padding for other styles that might be added in the future
     * 100+... misc IDs
     */
    /// <summary>
    /// Loads the given decoration object into the dictionary
    /// </summary>
    private GameObject loadDecorationObject(uint index)
    {
        /*
         * Decoration prefabs should be named in this convention:
         * "typeStylePrefab"
         * -type is first (path, bench, etc... with no capitalization)
         * -Style is second (Japanese, Urban, etc... with the first letter capitalized)
         * -"Prefab" should be on the end with the first letter capitalized
         * no spaces
         */
        string prefabName;
        if (index < 100) //It's a decoration
        {
            string decorationType;
            string decorationStyle;
            switch (Mathf.FloorToInt(index / 10))
            {
                case 0: decorationStyle = "Japanese"; break;
                case 1: decorationStyle = "Urban"; break;
                case 2: decorationStyle = "Suburban"; break;
                default: decorationStyle = "Japanese"; break;
            }
            switch (index % 10)
            {
                case 0: decorationType = "path"; break;
                case 1: decorationType = "bench"; break;
                case 2: decorationType = "post"; break;
                case 3: decorationType = "archway"; break;
                default: decorationType = "fence"; break;
            }
            prefabName = decorationType + decorationStyle + "Prefab";
        }
        else
        {
            switch (index)
            {
                case 100: prefabName = "grassBladesPrefab"; break;
                case 101: prefabName = "megaPillarPrefab"; break;
                case 102: prefabName = "flagPrefab"; break;
                case 103: prefabName = "windmillPrefab"; break;
                default: prefabName = "grassBladesPrefab"; break;
            }
        }

        Debug.Log("Building UI decoration... Prefab name for index ID {" + index + "} is " + prefabName);

        GameObject prefabObject = Resources.Load("Prefabs/Decorations/" + prefabName) as GameObject;
        decorationObject.Add(index, prefabObject);
        return prefabObject;
    }

    public GameObject getConnectorPrefab(uint index)
    {
        /*
         * Decoration prefabs should be named in this convention:
         * "typeStylePrefab"
         * -type is first (path, bench, etc... with no capitalization)
         * -Style is second (Japanese, Urban, etc... with the first letter capitalized)
         * -"Prefab" should be on the end with the first letter capitalized
         * no spaces
         */
        string prefabName;
        if (index < 100) //It's a decoration
        {
            string decorationStyle;
            switch (Mathf.FloorToInt(index / 10))
            {
                case 0: decorationStyle = "Japanese"; break;
                case 1: decorationStyle = "Urban"; break;
                case 2: decorationStyle = "Suburban"; break;
                default: decorationStyle = "Japanese"; break;
            }
            prefabName = "connector" + decorationStyle + "Prefab";
        }
        else
        {
            prefabName = "megaPillarPrefab";
        }

        GameObject prefabObject = Resources.Load("Prefabs/Decorations/" + prefabName) as GameObject;
        return prefabObject;
    }


    //petal petalcolor stem stemcolor leaf leafcolor pistil pistilcolor
    /// <summary>
    /// Returns a uint ID for a generated flower for the given planet ID (home-0, hot-1, cold-2, neon-3)
    /// </summary>
    /// <param name="planetID"></param>
    /// <returns></returns>
    public uint getRandomFlowerIDForPlanet(Planet.PlanetType planet)
    {
        uint returnValue = 0;
        int maxElementsPerPlanet = 4;

        int planetID = (int)planet;

        //Get random petal
        int addValue = availablePlanetPetals[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;
        returnValue *= 16;

        //Get random petal color
        addValue = availablePlanetPetalColors[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;
        returnValue *= 16;

        //Get random stem
        addValue = availablePlanetStems[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;
        returnValue *= 16;

        //Get random stem color
        addValue = availablePlanetStemColors[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;
        returnValue *= 16;

        //Get random leaf
        addValue = availablePlanetLeafs[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;
        returnValue *= 16;

        //Get random leaf color
        addValue = availablePlanetLeafColors[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;
        returnValue *= 16;

        //Get random pistil
        addValue = availablePlanetPistils[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;
        returnValue *= 16;

        //Get random pistil color
        addValue = availablePlanetPistilColors[planetID, Random.Range(0, maxElementsPerPlanet)];
        returnValue += (uint)addValue;

        return returnValue;
    }

    public string getRandomFlowerStringForPlanet(Planet.PlanetType planet)
    {
        StringBuilder result = new StringBuilder();
        int maxElementsPerPlanet = 4;

        int planetID = (int)planet;

        result.Append(availablePlanetPetals[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));
        result.Append(availablePlanetPetalColors[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));
        result.Append(availablePlanetStems[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));
        result.Append(availablePlanetStemColors[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));
        result.Append(availablePlanetLeafs[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));
        result.Append(availablePlanetLeafColors[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));
        result.Append(availablePlanetPistils[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));
        result.Append(availablePlanetPistilColors[planetID, Random.Range(0, maxElementsPerPlanet)].ToString("X"));

        return result.ToString();
    }

    //Compares 2 flowers with a full mask (4294967295)
    public static bool compareFlowers(uint flower1, uint flower2)
    {
        return compareFlowers(flower1, flower2, 4294967295);
    }

    //Compares to flowers with the given mask
    public static bool compareFlowers(uint flower1, uint flower2, uint mask)
    {
        return ((flower1 & mask) == (flower2 & mask));
    }
}