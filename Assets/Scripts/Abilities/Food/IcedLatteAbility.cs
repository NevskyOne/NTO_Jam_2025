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
    public class IcedLatteAbility : IAttack
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
            _input.actions[_data.InputBinding].performed += OnPerformed;
            if (_player != null)
            {
                foreach (var eff in _data.ApplyOnSelf)
                    _player.AddEffect(eff);
            }
        }

        public void Deactivate()
        {
            if (_input != null)
                _input.actions[_data.InputBinding].performed -= OnPerformed;
            if (_player != null)
            {
                foreach (var eff in _data.ApplyOnSelf)
                    _player.RemoveEffect(eff);
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

            // Выпускаем 3 ледяных осколка в направлении
            for (int i = 0; i < 3; i++)
            {
                float angle = (i - 1) * 15f; // -15°, 0°, +15°
                Vector2 shardDirection = RotateVector(direction.normalized, angle);
                FireIceShard(shardDirection);
            }

            _cooldownRoutine = _player.StartCoroutine(CooldownRoutine());
        }

        private void FireIceShard(Vector2 direction)
        {
            // Проверяем попадание по линии
            RaycastHit2D hit = Physics2D.Raycast(_owner.position, direction, _data.Radius * 2f);

            if (hit.collider != null && hit.collider.transform != _owner)
            {
                var hittable = hit.collider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    int damage = _data.BaseDamage;
                    // 2x урон против огня
                    if (hit.collider.name.Contains("Fire") || hit.collider.name.Contains("Flame"))
                    {
                        damage *= 2;
                    }

                    hittable.TakeDamage(damage);
                    foreach (var eff in _data.ApplyOnTargets)
                        eff.ApplyEffect(hit.collider.gameObject);
                }
            }

            // Визуализация осколка
            DrawDebugRay(_owner.position, direction * _data.Radius * 2f, Color.cyan, 0.4f);
        }

        private Vector2 RotateVector(Vector2 vector, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRadians);
            float sin = Mathf.Sin(angleRadians);
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }

        private IEnumerator CooldownRoutine()
        {
            yield return new WaitForSeconds(_data.AttackCooldown);
            _cooldownRoutine = null;
        }

        private void DrawDebugRay(Vector2 start, Vector2 end, Color color, float duration)
        {
            Debug.DrawLine(start, end, color, duration);
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
