[System.Serializable]
public class ItemStack
{
    public Item item;
    public int amount;

    public bool IsEmpty => item == null;

    public ItemStack()
    {
        item = null;
        amount = 0;
    }

    public ItemStack(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}