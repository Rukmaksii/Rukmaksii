using System.Collections.Generic;
using Items;

public class ItemContainer<TBaseItem> where TBaseItem : BaseItem
{
    protected int maxCount;
    private int currentCount = 0;
    private Stack<TBaseItem> itemList = new Stack<TBaseItem>();
    private ItemCategory category;
    public ItemCategory Category => category;

    public enum ItemCategory
    {
        Attack,
        Heal,
        Defense,
        Other
    }

    public ItemContainer(int maxCount)
    {
        this.maxCount = maxCount;
    }

    public bool Push(TBaseItem item)
    {
        if (currentCount < maxCount)
        {
            itemList.Push(item);
            currentCount++;
            return true;
        }

        return false;
    }

    public TBaseItem Pop()
    {
        if (itemList.Count > 0)
        {
            currentCount--;
            return itemList.Pop();
        }

        return null;
    }
}