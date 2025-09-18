using System;
using System.Collections;
using UnityEngine;
using Core.Data.ScriptableObjects;
using Core.Interfaces;
using UnityEngine.InputSystem;
using Zenject;

namespace Abilities.Food
{
    [Serializable]
    public class RatatouilleAbility : IAttack
    {
        [SerializeField] private FoodData _data;

        AttackDataSO IAttack.Data
        {
            get => _data;
            set => _data = (FoodData)value;
        }

        private Transform _owner;
        private Player _player;
        private PlayerInput _input;
        private Coroutine _cooldownRoutine;

        [Inject]
        private void Construct(Player player, PlayerInput input)
        {
            _owner = player.transform;
            _player = player;
            _input = input;
        }

        public void Activate()
        {
            if (_input == null) return;
            if (_data == null)
            {
                Debug.LogError("[RatatouilleAbility] Data is null");
                return;
            }
            _input.actions[_data.InputBinding].performed += OnPerformed;
            
            // Применяем эффекты на себя через новую систему
            if (_player != null && _data.ApplyOnSelf != null)
            {
                foreach (var effect in _data.ApplyOnSelf)
                {
                    if (effect != null)
                        effect.ApplyEffect(_player.gameObject);
                }
            }
        }

        public void Deactivate()
        {
            if (_input != null && _data != null)
                _input.actions[_data.InputBinding].performed -= OnPerformed;
                
            // Убираем эффекты с себя через новую систему
            if (_player != null && _data != null && _data.ApplyOnSelf != null)
            {
                foreach (var effect in _data.ApplyOnSelf)
                {
                    if (effect != null)
                        effect.RemoveEffect(_player.gameObject);
                }
            }
        }

        private void OnPerformed(InputAction.CallbackContext _)
        {
            var dir = _player != null ? _player.Movement.LastDirection : Vector2.right;
            PerformAttack(dir);
        }

        public void PerformAttack(Vector2 direction)
        {
            if (_owner == null || _cooldownRoutine != null) return;

            Vector2 center = (Vector2)_owner.position + direction.normalized * _data.Radius * _data.ForwardOffset;
            float radius = _data.Radius;
            var hits = Physics2D.OverlapCircleAll(center, radius);

            for (int i = 1; i <= 3; i++)
            {
                Vector2 p = center + direction.normalized * (_data.Radius * 0.75f * i);
                var cols = Physics2D.OverlapCircleAll(p, radius * 0.8f);
                foreach (var col in cols)
                {
                    if (col.transform == _owner) continue;
                    var h = col.GetComponent<IHittable>();
                    if (h != null)
                    {
                        int damage = (int)_data.BaseDamage;
                        // 2x урон по жиру
                        if (HasGreaseEffect(col.gameObject))
                        {
                            damage *= 2;
                            Debug.Log($"Ratatouille: bonus damage to greasy enemy {col.name}");
                        }
                        h.TakeDamage(damage);
                        
                        // Применяем эффекты на цели через новую систему
                        if (_data.ApplyOnTargets != null)
                        {
                            foreach (var effect in _data.ApplyOnTargets)
                            {
                                if (effect != null)
                                    effect.ApplyEffect(col.gameObject);
                            }
                        }
                        Debug.Log($"Ratatouille vine hit {col.name} for {damage} damage");
                    }
                }
            }

            DrawDebugCircle(center, radius, Color.green, 0.4f);
            _cooldownRoutine = _player.StartCoroutine(CooldownRoutine());
        }

        private bool HasGreaseEffect(GameObject target)
        {
            var runner = target.GetComponent<Core.Interfaces.EffectRunner>();
            return runner != null && runner.HasEffectByName("Grease");
        }

        private IEnumerator CooldownRoutine()
        {
            yield return new WaitForSeconds(_data.AttackCooldown);
            _cooldownRoutine = null;
        }

        private void DrawDebugCircle(Vector2 center, float radius, Color color, float duration)
        {
            int segments = 24;
            float angle = 2 * Mathf.PI / segments;
            Vector2 prev = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;
            for (int i = 1; i <= segments; i++)
            {
                float a = i * angle;
                Vector2 cur = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                Debug.DrawLine(prev, cur, color, duration);
                prev = cur;
            }
        }
    }
}
