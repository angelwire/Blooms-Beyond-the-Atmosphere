using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author Vance Howald

public class HomeBaseController : MonoBehaviour
{
    enum jumpIndex { l, u };
    int totalFlowersSold;
    int startingWorldCount;
    public float[,] partValue = new float[4, 16];
    public FlowerPart[,] partsEconomy = new FlowerPart[4, 16];
    int[] categories = { 4, 4, 4, 8, 8, 8, 8, 12, 12, 12, 15, 15, 19, 23, 27, 31 };//simulated distribution to help keep values in good spots
    int[] max = { 8, 8, 8, 12, 12, 12, 12, 15, 15, 15, 19, 19, 23, 27, 31, 35 };//used for creating jumps
    float[] lowerJumpRange = { -.75f, -.1f };//hardcoded elsew
    float[] upperJumpRange = { 1, 2.5f};
    int valueCap = 34;

    int maxPartValue = 35;
    int minPartValue = 4;
    int maxPriceJump = 2;
    FlowerPlayerControler player;
    Camera mainCamera;

    public static HomeBaseController instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        mainCamera = Camera.main;
        player = GameObject.Find("FPSController").GetComponent<FlowerPlayerControler>();
        foreach (MeshFilter filter in GetComponentsInChildren<MeshFilter>())
        {
            filter.mesh.bounds = new Bounds(Vector3.zero, new Vector3(10, 20, 10));
        }

        if (PlayerPrefs.HasKey("economy_0_0"))
        {
            loadEconomy();
        }
        else
        {
            for (int x = 0; x < 4; x++)
            {
                List<int> toPickFrom = new List<int>();
                List<int> toPickFrom2 = new List<int>();
                for (int y = 0; y < 16; y++)//for every part of the flower the possible options are shuffleded 
                {
                    toPickFrom.Add(y);
                    toPickFrom2.Add(y);
                }
                for (int y = 0; y < 16; y++)
                {
                    int rand = Random.Range(0, toPickFrom.Count);//pick random start
                    int index = toPickFrom[rand];
                    toPickFrom.RemoveAt(rand);
                    float currentVal = (float)categories[index] + Random.Range(0f, (float)max[index] - categories[index]);
                    rand = Random.Range(0, toPickFrom2.Count);//pick random target
                    int index2 = toPickFrom2[rand];
                    toPickFrom2.RemoveAt(rand);
                    float targetVal = (float)categories[index2] + Random.Range(0f, (float)max[index2] - categories[index2]);
                    partsEconomy[x, y] = new FlowerPart(index, index2, currentVal, targetVal, Random.Range(lowerJumpRange[(int)jumpIndex.l], lowerJumpRange[(int)jumpIndex.u]), Random.Range(upperJumpRange[(int)jumpIndex.l], upperJumpRange[(int)jumpIndex.u]));
                }
            }
            float minValue = 20f;
            int[,] shuffleHelper = //I want to make sure there is at least one of each part on home planet that is worth more than minValue, having this makes the following code easier
            {
            {1,2,4,7},//these are the parts that are available on homeworld
            {0,4,9,11},
            {0,2,8,15},
            {0,1,3,6}

        };
            for (int i = 0; i < 4; i++)
            {
                //if nothing is above minValue
                if (partsEconomy[i, shuffleHelper[i, 0]].currentValue < minValue && partsEconomy[i, shuffleHelper[i, 1]].currentValue < minValue && partsEconomy[i, shuffleHelper[i, 2]].currentValue < minValue && partsEconomy[i, shuffleHelper[i, 3]].currentValue < minValue)
                {
                    //find one that is and swap them
                    int toAdjust = shuffleHelper[i, Random.Range(0, 4)];
                    int newOne = Random.Range(0, 16);
                    while (partsEconomy[i, newOne].currentValue < minValue)
                    {
                        newOne = Random.Range(0, 16);
                    }
                    FlowerPart temp = partsEconomy[i, newOne];
                    partsEconomy[i, newOne] = partsEconomy[i, toAdjust];
                    partsEconomy[i, toAdjust] = temp;
                }
            }
        }
        totalFlowersSold = startingWorldCount;
        //sell balance test
        /*string toPrint = "";
        for (int i = 0; i < 4; i++)
        {
            toPrint += "{";
            for (int j = 0; j < 16; j++)
            {
                toPrint += partValue[i, j] + ", ";
            }
            toPrint = toPrint.Substring(0, toPrint.Length - 2);
            toPrint += "}\n";
        }
        print(toPrint);
        for (int i = 0; i < 50; i++)
        {
            uint returnIndex = 0;
            for (int j = 0; j < 8; j++)
            {
                returnIndex += (uint)Random.Range(0, 5);
                returnIndex *= 16;
            }
            processFlowerSell(returnIndex);
        }
        for (int i = 0; i < 80; i++)
        {
            uint returnIndex = 0;
            for (int j = 0; j < 8; j++)
            {
                returnIndex += (uint)Random.Range(0,9);
                returnIndex *= 16;
            }
            processFlowerSell(returnIndex);
        }
        for (int i = 0; i < 110; i++)
        {
            uint returnIndex = 0;
            for (int j = 0; j < 8; j++)
            {
                returnIndex += (uint)Random.Range(0, 13);
                returnIndex *= 16;
            }
            processFlowerSell(returnIndex);
        }
        for (int i = 0; i < 200; i++)
        {
            uint returnIndex = 0;
            for (int j = 0; j < 8; j++)
            {
                returnIndex += (uint)Random.Range(0, 17);
                returnIndex *= 16;
            }
            processFlowerSell(returnIndex);
        }
        toPrint = "";
        for (int i = 0; i < 4; i++)
        {
            toPrint += "{";
            for (int j = 0; j < 16; j++)
            {
                toPrint += partValue[i, j] + ", ";
            }
            toPrint = toPrint.Substring(0, toPrint.Length - 2);
            toPrint += "}\n";
        }
        print(toPrint);*/
    }

    public void processFlowerSell(uint flowerId)
    {
        player.money += getFlowerValue(flowerId);
        string flowerIdS = flowerId.ToString("X8");
        for (int i = 0; i < 4; i++)
        {
            int partId = System.Uri.FromHex(flowerIdS[i * 2]);
            int newIndex = Random.Range(0, partsEconomy[i, partId].targetIndex);//swap with a random lower value part
            for (int j = 0; j < 16; j++)
            {
                if (partsEconomy[i, j].targetIndex == newIndex)
                {
                    swap(partsEconomy[i, partId], partsEconomy[i, j]);
                    break;
                }
            }

        }
        for (int i = 0; i < 4; i++)//update vals
        {
            List<FlowerPart> toSwap=new List<FlowerPart>();
            for (int j = 0; j < 16; j++)
            {
                if (partsEconomy[i, j].jump())//if reached target val
                {
                    toSwap.Add(partsEconomy[i, j]);
                }
            }
            for (int k = 0; k < toSwap.Count; k++)//find a random part to swap with
            {
                int index = Random.Range(0, 16);
                while (!partsEconomy[i, index].canBePicked())
                {
                    index = Random.Range(0, 16);
                }
                swap(partsEconomy[i, index], toSwap[k]);
            }
        }
        GameJoltAPIHelper.IncDataStoreKey(GameJoltAPIHelper.flowers_sold);
    }

    public int getFlowerValue(uint flowerId)
    {
        if (HomeInventoryData.instance.challangeFlowerIds.Contains(flowerId))
        {
            return HomeInventoryData.instance.challangeFlowerValue;
        }
        string flowerIdS = flowerId.ToString("X8");
        float toReturn = 0;
        int maxColorCount = 0;
        for (int i = 0; i < 4; i++)
        {
            int colorCount = 0;
            char currentColor = flowerIdS[(i * 2) + 1];
            for (int j = 0; j < 4; j++)
            {
                if (flowerIdS[(j * 2) + 1] == currentColor)
                {
                    colorCount++;
                }
            }
            if (colorCount > maxColorCount)
                maxColorCount = colorCount;
            toReturn += partsEconomy[i, System.Uri.FromHex(flowerIdS[i * 2])].currentValue;
        }
        toReturn *= getColorMultRate(maxColorCount);
        return (int)toReturn;
    }

    private float getPartValue(float commonRate)
    {
        float percentile = (commonRate - minPartValue) / (maxPartValue - minPartValue);
        percentile = Mathf.Pow(percentile, 2 * (1 - percentile));
        return (percentile * (maxPartValue - minPartValue)) + minPartValue;
    }

    public float getRandomNormalizedSkewedNumber(float min, float mode, float max)
    {
        float normalHelper = 0;
        for (int i = 0; i < 16; i++)
        {
            normalHelper += Random.Range(0f, 1f);
        }
        normalHelper /= 16f;
        if (normalHelper > 0.5f)
        {
            normalHelper -= .5f;
            normalHelper *= 2;
            return (normalHelper * (max - mode)) + mode;
        }
        else
        {
            normalHelper *= 2;
            return (normalHelper * (mode - min)) + min;
        }
    }

    private float getColorMultRate(int count)
    {
        switch (count)
        {
            case 2:
                return 1.5f;
            case 3:
                return 2f;
            case 4:
                return 4f;
        }
        return .25f;
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        Vector3 camPos = mainCamera.transform.position;
        //Get the squared distance
        float xDifference = pos.x - camPos.x;
        float zDifference = pos.z - camPos.z;
        float distanceFromCamera = (xDifference * xDifference) + (zDifference * zDifference);

        //Scale the item so it slowly transitions into the view
        float maxFlowerDistance = 400;
        float minFlowerDistance = 100;
        float scaleAmount = Mathf.Clamp(1 - ((distanceFromCamera - minFlowerDistance) / (maxFlowerDistance - minFlowerDistance)), .01f, 1);
        transform.transform.localScale = new Vector3((scaleAmount * .7f) + .3f, scaleAmount, (scaleAmount * .7f) + .3f);
    }





    private float getPartValueRebal(float commonRate)
    {
        float percentile = (commonRate - minPartValue) / (maxPartValue - minPartValue);
        percentile = Mathf.Pow(percentile, 2 * (1 - percentile));
        return (percentile * (maxPartValue - minPartValue)) + minPartValue;
    }

    public void swap(FlowerPart a, FlowerPart b)
    {
        int temp = b.targetIndex;
        b.targetIndex = a.targetIndex;
        a.targetIndex = temp;
        a.targetValue= (float)categories[a.targetIndex] + Random.Range(0f, (float)max[a.targetIndex] - categories[a.targetIndex]);
        b.targetValue= (float)categories[b.targetIndex] + Random.Range(0f, (float)max[b.targetIndex] - categories[b.targetIndex]);
        a.updateJumps(Random.Range(lowerJumpRange[(int)jumpIndex.l], lowerJumpRange[(int)jumpIndex.u]), Random.Range(upperJumpRange[(int)jumpIndex.l], upperJumpRange[(int)jumpIndex.u]));
        b.updateJumps(Random.Range(lowerJumpRange[(int)jumpIndex.l], lowerJumpRange[(int)jumpIndex.u]), Random.Range(upperJumpRange[(int)jumpIndex.l], upperJumpRange[(int)jumpIndex.u]));
    }

    public void saveEconomy()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                PlayerPrefs.SetString("economy_" + i + "_" + j, partsEconomy[i, j].save());
            }
        }
    }

    public void loadEconomy()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                partsEconomy[i, j] = new FlowerPart(PlayerPrefs.GetString("economy_" + i + "_" + j));
            }
        }
    }

}