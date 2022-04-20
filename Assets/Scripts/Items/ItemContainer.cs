using System.Collections.Generic;
using model;

namespace Items
{
    public class ItemContainer<TBaseItem> where TBaseItem : BaseItem
    {
        public readonly int MaxCount;
        private Stack<TBaseItem> items = new Stack<TBaseItem>();
        public int Count => items.Count;
        private ItemCategory category;
        public ItemCategory Category => category;

        private readonly Inventory inventory;

    

        public ItemContainer(int maxCount, Inventory inventory)
        {
            this.MaxCount = maxCount;
            this.inventory = inventory;
        }

        public bool Push(TBaseItem item, bool present = false)
        {
            if (items.Count < MaxCount)
            {
                items.Push(item);
                if (!present)
                    inventory.AddItem(item);
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
}