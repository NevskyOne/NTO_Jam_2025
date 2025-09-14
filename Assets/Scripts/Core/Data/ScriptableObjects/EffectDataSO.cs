using UnityEngine;

namespace Core.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EffectData", menuName = "Game Data/Effect Data")]
    public class EffectDataSO : ScriptableObject
    {
        [Header("Основные параметры")]
        [SerializeField] private string _effectName;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _intensity = 1f;
        
        [Header("Визуальные эффекты")]
        [SerializeField] private GameObject _visualEffectPrefab;
        [SerializeField] private Color _effectColor = Color.white;
        
        [Header("Звуковые эффекты")]
        [SerializeField] private AudioClip _applySound;
        [SerializeField] private AudioClip _removeSound;

        public string EffectName => _effectName;
        public float Duration => _duration;
        public float Intensity => _intensity;
        public GameObject VisualEffectPrefab => _visualEffectPrefab;
        public Color EffectColor => _effectColor;
        public AudioClip ApplySound => _applySound;
        public AudioClip RemoveSound => _removeSound;
    }
}
