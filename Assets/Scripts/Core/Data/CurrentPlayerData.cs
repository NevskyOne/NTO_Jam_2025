
using System.Collections.Generic;

public struct CurrentPlayerData : ISavableData
{
    public int Health;
    public int Money;
    public int Reputation;

    public List<int> UsedFood;
    public List<int> InventoryFood;
}
