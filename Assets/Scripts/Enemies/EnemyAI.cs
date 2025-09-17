#if false
using System.Collections.Generic;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IHittable, IEffectHandler {
    [SerializeField] private EnemyDataSO _data;
    [SerializeField] private PlayerCheckSystem _playerCheck;

    private List<EffectBase> _activeEffects;
    private EnemyState _state;

    public void OnEnable(){
        //_playerCheck.[любые события по типу PlayerDetected] +=  выбираем атаку или меняем состояние
    }

    public void OnDisable(){
        //отписка
    }

    public void ChangeState(EnemyState state){}

    public void AttackChoise(){}

    public void AddEffect(EffectBase effect){}

    public void RemoveEffect(EffectBase effect){}

    public void TakeDamage(int amount){}

    public void Die(){}
}

public enum EnemyState{ Normal, Attack }
#endif