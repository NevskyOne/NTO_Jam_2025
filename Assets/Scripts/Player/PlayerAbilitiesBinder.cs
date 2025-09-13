using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using Abilities.Food;

public enum FoodType
{
    None,
    Tea,
    IcedLatte,
    DragonFruit,
    Dumpling,
    KoreanCarrot,
    Ratatouille,
    Burger,
    ExplosiveCaramel,
    PoisonPotato
}

public class PlayerAbilitiesBinder : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private AttackDataSO _attackData;

    [Header("Loadout")]
    [SerializeField] private FoodType _lmbOverride = FoodType.None;
    [SerializeField] private FoodType _qAbility = FoodType.None;
    [SerializeField] private FoodType _eAbility = FoodType.None;

    [Header("Passive/Always On")]
    [SerializeField] private FoodType _passive = FoodType.None; // Например, KoreanCarrot (дабл-джамп)

    private void Awake()
    {
        if (_player == null) _player = GetComponent<Player>();
        if (_player == null || _attackData == null)
        {
            Debug.LogWarning("PlayerAbilitiesBinder: Missing refs (Player/AttackDataSO)");
            return;
        }

        _player.EquipLmbOverride(CreateAbility(_lmbOverride));
        _player.EquipQ(CreateAbility(_qAbility));
        _player.EquipE(CreateAbility(_eAbility));
        ApplyPassive(_passive);
    }

    private IAttack CreateAbility(FoodType type)
    {
        switch (type)
        {
            case FoodType.Tea: return new TeaAbility(_attackData, _player.transform);
            case FoodType.IcedLatte: return new IcedLatteAbility(_attackData, _player.transform);
            case FoodType.DragonFruit: return new DragonFruitAbility(_attackData, _player.transform);
            case FoodType.Dumpling: return new DumplingAbility(_attackData, _player.transform);
            case FoodType.KoreanCarrot: return new KoreanCarrotAbility(_attackData, _player.transform);
            case FoodType.Ratatouille: return new RatatouilleAbility(_attackData, _player.transform);
            case FoodType.Burger: return new BurgerAbility(_attackData, _player.transform);
            case FoodType.ExplosiveCaramel: return new ExplosiveCaramelAbility(_attackData, _player.transform);
            case FoodType.PoisonPotato: return new PoisonPotatoAbility(_attackData, _player.transform);
            default: return null;
        }
    }

    private void ApplyPassive(FoodType type)
    {
        if (type == FoodType.KoreanCarrot)
        {
            _player.SetExtraJumps(1);
        }
        // TODO: DragonFruit — активируем/включаем щит-коллайдер при движении
    }
}
