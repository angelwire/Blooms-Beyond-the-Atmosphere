//created: 10-20-19
//updated: 11-3-19
//version: .02
//author Vance Howald
//LinkedList node with sorted add
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueueNode<T, U> where T : FlowerObj where U : System.IComparable
{
    public T data;
    public U compareable;
    public PriorityQueueNode<T, U> next;
    public PriorityQueueNode(T data, U compareable)
    {
        this.data = data;
        this.compareable = compareable;
        next = null;
    }

    public void Add(T new_data, U new_compareable)//sorted add
    {
        if (next == null)//is this the end of the list
        {
            next = new PriorityQueueNode<T, U>(new_data, new_compareable);//append data
        }
        else if (next.compareable.CompareTo(new_compareable) < 0) //is the new data larger than next?
        {
            next.Add(new_data, new_compareable);//pass it to the next node to check
        }
        else//current value is smaller than next so needs to be linked in here
        {
            PriorityQueueNode<T, U> temp = new PriorityQueueNode<T, U>(new_data, new_compareable);
            temp.next = next;
            next = temp;
        }
    }

    public GameObject FindAndRemove(T toRemove, U findHelper)//finds and removes
    {
        if (next != null)//if can check next
        {
            if (next.data.CompareTo(toRemove) == 0)//is it it
            {
                GameObject toReturn = next.data.gameObject;
                next = next.next;//remove
                return toReturn;
            }
            if (compareable.CompareTo(findHelper) > 0)//if the findhelper is too large then we have overshot the data and it's not here
            {
                return null;
            }
            return next.FindAndRemove(toRemove, findHelper);//check next
        }
        return null;
    }

    public string Print()//method to help vissualize the list in testing, will be removed
    {
        if (next != null)
        {
            return compareable + " " + next.Print();
        }
        return compareable + "";
    }


}