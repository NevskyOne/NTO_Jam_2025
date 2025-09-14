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
    public class DumplingAbility : IAttack
    {
        [field: SerializeReference] AttackDataSO IAttack.Data { get; set; }
        private FoodData Data => (FoodData)((IAttack)this).Data;

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
            _input.actions[Data.InputBinding].performed += OnPerformed;
            if (_player != null)
            {
                foreach (var eff in Data.ApplyOnSelf)
                    _player.AddEffect(eff);
            }
        }

        public void Deactivate()
        {
            if (_input != null)
                _input.actions[Data.InputBinding].performed -= OnPerformed;
            if (_player != null)
            {
                foreach (var eff in Data.ApplyOnSelf)
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

            Vector2 center = (Vector2)_owner.position + direction.normalized * Data.Radius * Data.ForwardOffset;
            float radius = Data.Radius;
            var hits = Physics2D.OverlapCircleAll(center, radius);

            foreach (var col in hits)
            {
                if (col.transform == _owner) continue;
                var h = col.GetComponent<IHittable>();
                if (h != null)
                {
                    h.TakeDamage(Data.BaseDamage);
                    foreach (var eff in Data.ApplyOnTargets)
                        eff.ApplyEffect(col.gameObject);
                }
            }

            DrawDebugCircle(center, radius, Color.yellow, 0.4f);
            _cooldownRoutine = _player.StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            yield return new WaitForSeconds(Data.AttackCooldown);
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
