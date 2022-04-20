using System.Collections.Generic;
using Items;
using model;

public class ItemContainer<TBaseItem> where TBaseItem : BaseItem
{
    public readonly int MaxCount;
    private Stack<TBaseItem> items = new Stack<TBaseItem>();
    private ItemCategory category;
    public ItemCategory Category => category;

    

    public ItemContainer(int maxCount)
    {
        this.MaxCount = maxCount;
    }

    public bool Push(TBaseItem item)
    {
        if (items.Count < MaxCount)
        {
            items.Push(item);
            return true;
        }

        return false;
    }

    public TBaseItem Pop()
    {
        if (items.Count > 0)
        {
            return items.Pop();
        }

        return null;
    }
}