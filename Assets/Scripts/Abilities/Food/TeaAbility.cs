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
    public class TeaAbility : IAttack
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
            Debug.Log(" [TeaAbility] Activate() вызван!");
            
            // ПРИНУДИТЕЛЬНО сбрасываем кулдаун при активации
            if (_cooldownRoutine != null && _player != null)
            {
                Debug.Log(" [TeaAbility] Принудительно сбрасываем старый кулдаун");
                _player.StopCoroutine(_cooldownRoutine);
                _cooldownRoutine = null;
            }
            
            if (_input == null) 
            {
                Debug.LogError(" [TeaAbility] _input is null!");
                return;
            }
            
            if (_data == null)
            {
                Debug.LogError(" [TeaAbility] _data is null!");
                return;
            }
            
            string inputBinding = _data.InputBinding;
            Debug.Log($" [TeaAbility] InputBinding: '{inputBinding}'");
            
            var action = _input.actions[inputBinding];
            if (action == null)
            {
                Debug.LogError($" [TeaAbility] Действие '{inputBinding}' не найдено в Input System!");
                return;
            }
            
            Debug.Log($" [TeaAbility] Действие '{inputBinding}' найдено, подписываемся на performed");
            action.performed += OnPerformed;
            
            // Применяем эффекты на себя через новую систему
            if (_player != null && _data.ApplyOnSelf != null)
            {
                Debug.Log($" [TeaAbility] Применяем {_data.ApplyOnSelf.Count} эффектов на игрока");
                foreach (var effect in _data.ApplyOnSelf)
                {
                    if (effect != null)
                    {
                        Debug.Log($" [TeaAbility] Применяем эффект: {effect.name}");
                        effect.ApplyEffect(_player.gameObject);
                    }
                    else
                    {
                        Debug.LogError(" [TeaAbility] Эффект в ApplyOnSelf равен null!");
                    }
                }
            }
            else
            {
                Debug.LogError($" [TeaAbility] Player={_player != null}, ApplyOnSelf={_data.ApplyOnSelf != null}");
            }
        }

        public void Deactivate()
        {
            Debug.Log(" [TeaAbility] Deactivate() вызван!");
            
            if (_input != null && _data != null)
            {
                string inputBinding = _data.InputBinding;
                var action = _input.actions[inputBinding];
                if (action != null)
                {
                    action.performed -= OnPerformed;
                    Debug.Log($" [TeaAbility] Отписались от действия '{inputBinding}'");
                }
            }
                
            // Убираем эффекты с себя через новую систему
            if (_player != null && _data != null && _data.ApplyOnSelf != null)
            {
                Debug.Log($" [TeaAbility] Убираем {_data.ApplyOnSelf.Count} эффектов с игрока");
                foreach (var effect in _data.ApplyOnSelf)
                {
                    if (effect != null)
                    {
                        Debug.Log($" [TeaAbility] Убираем эффект: {effect.name}");
                        effect.RemoveEffect(_player.gameObject);
                    }
                }
            }
            
            // ИСПРАВЛЕНИЕ: Сбрасываем кулдаун при деактивации
            if (_cooldownRoutine != null && _player != null)
            {
                Debug.Log(" [TeaAbility] Принудительно сбрасываем кулдаун");
                _player.StopCoroutine(_cooldownRoutine);
                _cooldownRoutine = null;
            }
        }

        private void OnPerformed(InputAction.CallbackContext _)
        {
            Debug.Log(" [TeaAbility] OnPerformed вызван! LeftMouse нажата!");
            var dir = _player != null ? _player.Movement.LastDirection : Vector2.right;
            Debug.Log($" [TeaAbility] Направление атаки: {dir}");
            PerformAttack(dir);
        }

        public void PerformAttack(Vector2 direction)
        {
            Debug.Log(" [TeaAbility] PerformAttack начата!");
            
            if (_owner == null || _cooldownRoutine != null) 
            {
                Debug.LogError($" [TeaAbility] Owner={_owner != null}, Cooldown={_cooldownRoutine == null}");
                return;
            }

            Vector2 center = _owner.position;
            float radius = _data.Radius;
            Debug.Log($" [TeaAbility] Атака в центре {center} с радиусом {radius}");
            
            var hits = Physics2D.OverlapCircleAll(center, radius);
            Debug.Log($" [TeaAbility] Найдено {hits.Length} объектов в радиусе");

            foreach (var col in hits)
            {
                if (col.transform == _owner) continue;
                var h = col.GetComponent<IHittable>();
                if (h != null)
                {
                    Debug.Log($" [TeaAbility] Атакуем {col.name} с уроном {_data.BaseDamage}");
                    h.TakeDamage(_data.BaseDamage);
                    
                    // Применяем эффекты на цели через новую систему
                    if (_data.ApplyOnTargets != null)
                    {
                        foreach (var effect in _data.ApplyOnTargets)
                        {
                            if (effect != null)
                            {
                                Debug.Log($" [TeaAbility] Применяем эффект на цель: {effect.name}");
                                effect.ApplyEffect(col.gameObject);
                            }
                        }
                    }
                }
            }

            // Визуализация для отладки
            DrawDebugCircle(center, radius, Color.yellow, 0.4f);
            _cooldownRoutine = _player.StartCoroutine(CooldownRoutine());
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
