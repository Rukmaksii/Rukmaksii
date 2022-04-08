using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer<TBaseItem> : MonoBehaviour
{
    protected int maxCount;
    private int currentCount = 0;
    private Stack<TBaseItem> itemList = new Stack<TBaseItem>();

    public ItemContainer(int maxCount)
    {
        this.maxCount = maxCount;
    }

    void Push(TBaseItem item)
    {
        itemList.Push(item);
    }
    
    TBaseItem Pop()
    {
        return itemList.Count > 0 ? itemList.Pop() : default(TBaseItem);
    }
}
