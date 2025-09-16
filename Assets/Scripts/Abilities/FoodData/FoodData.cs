using System.Collections.Generic;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "Game Data/Food Data")]
public class FoodData : AttackDataSO
{
    [field: SerializeField] public int Duration { get; private set; } = 5;

    [Header("Attack Shape")]
    [SerializeField] private float _radius = 1.2f;
    [SerializeField] private float _forwardOffset = 0.7f;
    [SerializeField] private float _attackCooldown = 0.8f;

    public float Radius => _radius;
    public float ForwardOffset => _forwardOffset;
    public float AttackCooldown => _attackCooldown;

    [Header("Effects")] 
    [SerializeField] private List<EffectBase> _applyOnSelf = new();
    [SerializeField] private List<EffectBase> _applyOnTargets = new();

    public IReadOnlyList<EffectBase> ApplyOnSelf => _applyOnSelf;
    public IReadOnlyList<EffectBase> ApplyOnTargets => _applyOnTargets;
}
