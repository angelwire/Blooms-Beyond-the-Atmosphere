using UnityEngine;

//Author Vance Howald

public abstract class Placeable : MonoBehaviour
{
    public bool isHovered;
    public bool isFlower;
    public bool growable;
    public float uiScale;
    public uint id;
    public int[] flowerGridPos={ -1,-1};

    public abstract Vector3 posClamp(RaycastHit hit);
    public abstract GameObject cloneObj(GameObject model, Placeable data, RaycastHit hit);
    public abstract bool canBePlaced(RaycastHit hit);
    public abstract void setHover(bool hover);
    public abstract void clearPos();

    public bool isEqual(Placeable other)
    {
        return isFlower == other.isFlower && id == other.id;
    }
    
    public virtual bool isLivingFlower()
    {
        return false;
    }

    public virtual bool isPlaceable()//can be overridden if needed by some decorations
    {
        return isFlower && !isLivingFlower();
    }
    public virtual string save()
    {
        return "1|" + id + "|" + transform.position.x + "|" + transform.position.y + "|" + transform.position.z + "|" + transform.eulerAngles.x + "|" + transform.eulerAngles.y + "|" + transform.eulerAngles.z;
    }

    public virtual bool additionalLoad(string s)
    { return true; }

    public virtual void init(int x, int y)
    {
        flowerGridPos[0] = x;
        flowerGridPos[1] = y;
    }
}
