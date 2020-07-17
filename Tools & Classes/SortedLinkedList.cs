//created: 10-20-19
//updated: 11-2-19
//version: .01
//author Vance Howald
//Linked list for flowers that are waiting to spawn
//Ideally this should be replaced with a binary heap priority queue
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PriorityQueue<T, U> where T : FlowerObj where U : System.IComparable
{
    internal PriorityQueueNode<T, U> head;

    public void Add(T new_data, U new_comparable)//sorted add
    {
        if (head == null || head.compareable.CompareTo(new_comparable) > 0)//is this the first node in the list or will the flower spawn before current
        {
            InsertFront(new_data, new_comparable);
        }
        else//append
        {
            head.Add(new_data, new_comparable);
        }
    }
    internal void InsertFront(T new_data, U new_comparable)//if data needs to be put at front
    {
        PriorityQueueNode<T, U> new_node = new PriorityQueueNode<T, U>(new_data, new_comparable);
        new_node.next = this.head;
        this.head = new_node;
    }
    public void RemoveFront()
    {
        head = this.head.next;
    }
    public GameObject FindAndRemove(T toRemove, U findHelper)//finds a flower with the same pos and re
    {
        if (head == null)//if there's no data
        {
            return null;
        }
        if (head.data.CompareTo(toRemove) == 0)//is this the data
        {
            if (head.data == null)
            {
                int a = 9;
            }
            GameObject toReturn = head.data.gameObject;
            head = this.head.next;
            return toReturn;
        }
        return head.FindAndRemove(toRemove, findHelper);//pass
    }

    public string Print()
    {
        return head.Print();
    }
}