using System.Collections.Generic;
using Items;
using model;

public class ItemContainer<TBaseItem> where TBaseItem : BaseItem
{
    protected int maxCount;
    private Stack<TBaseItem> itemList = new Stack<TBaseItem>();
    private ItemCategory category;
    public ItemCategory Category => category;

    

    public ItemContainer(int maxCount)
    {
        this.maxCount = maxCount;
    }

    public bool Push(TBaseItem item)
    {
        if (itemList.Count < maxCount)
        {
            itemList.Push(item);
            return true;
        }

        return false;
    }

    public TBaseItem Pop()
    {
        if (itemList.Count > 0)
        {
            return itemList.Pop();
        }

        return null;
    }
}