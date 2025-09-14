
using Core.Data.ScriptableObjects;
using UnityEngine;

public class FoodData : AttackDataSO
{
    [field:SerializeReference] public int Duration {get; private set; }
}
