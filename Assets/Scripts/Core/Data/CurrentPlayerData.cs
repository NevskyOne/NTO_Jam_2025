
using System.Collections.Generic;

public class CurrentPlayerData : ISavableData
{
    public int Health;
    public int Money;
    public int Reputation;

    public List<int> UsedFood = new();
    public List<int> InventoryFood = new();
}
