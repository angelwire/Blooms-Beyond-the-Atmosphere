using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPart : MonoBehaviour
{
    public int currentIndex;//only used in startup
    public int targetIndex;//where does the part want to go
    public float currentValue;//current value of the part
    public float targetValue;
    public float lowerJump;//minium the part will jump up
    public float upperJump;//max the part will jump up
    public float baseJump;//to help parts reach their goal in timely manners, this is needed to prevent all parts from mashing into the 8-15 range and to help parts drop fast
    

    public bool canBePicked()//because parts are more likely to exist in $8-$15 range, parts that are trying to rise need a little bit more help in getting there
    {                        //this prevents them from being picked when a part gets to target. They can be picked if a part is sold because a sold part will allways have a higher target 
        if (targetIndex > 10 && currentValue < targetValue)
        {
            return false;
        }
        return true;
    }
    public FlowerPart(string toLoad)
    {
        load(toLoad);
    }
    public FlowerPart(int currentIndex, int targetIndex, float currentValue, float targetValue, float lowerJump, float upperJump)
    {
        this.currentIndex = currentIndex;
        this.targetIndex = targetIndex;
        this.currentValue = currentValue;
        this.targetValue = targetValue;
        updateJumps(lowerJump, upperJump);
    }

    public void updateJumps(float lowerJump, float upperJump)//jumps need to be checked because if a part is decreasing lower and upper need to be switched, also used for baseJump
    {
        baseJump = 0;
        if (Mathf.Abs(targetValue - currentValue) / upperJump > 6)
        {
            int maxSteps = Random.Range(3, 6);
            baseJump = (Mathf.Abs(targetValue - currentValue)-(maxSteps*upperJump)) / maxSteps;
            upperJump *= 2;//spice up the jumps a lil because having a base jump will control the jump too much otherwise
            lowerJump *= 1.5f;
        }
        if (targetIndex < currentIndex)//if going down the two should be swapped along with the base jump
        {
            this.lowerJump = -upperJump;
            this.upperJump = -lowerJump;
            baseJump *= -1;
        }
        else
        {
            this.lowerJump = lowerJump;
            this.upperJump = upperJump;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>True if part has reached target value</returns>
    public bool jump()
    {
        float toJump = Random.Range(lowerJump, upperJump) + baseJump;
        if (Mathf.Abs(currentValue - targetValue) < Mathf.Abs(currentValue + toJump - targetValue))//if the jump will pass target
        {
            currentValue = targetValue;//jump to target
            return true;
        }
        currentValue += toJump;
        return false;
    }

    public string save()
    {
        return currentIndex + "|" + targetIndex + "|" + currentValue + "|" + targetValue + "|" + lowerJump + "|" + upperJump + "|" + baseJump;
    }   

    public void load(string toLoad)
    {
        string[] processed = toLoad.Split('|');
        currentIndex = int.Parse(processed[0]);
        targetIndex = int.Parse(processed[1]);
        currentValue = float.Parse(processed[2]);
        targetValue = float.Parse(processed[3]);
        updateJumps(float.Parse(processed[4]), float.Parse(processed[5]));
    }

}
