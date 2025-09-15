using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicControlSystem : MonoBehaviour
{
    [SerializeField] private MusicState _state;
    [SerializeField] private AudioClip[] _barMusic;
    [SerializeField] private AudioClip[] _battleMusic;
    [SerializeField] private AudioSource _source;
    
    public void ChangeMusicState(){}

    public void Update()
    {
        if (_source.isPlaying) return;

        switch (_state)
        {
            case MusicState.Bar:
                _source.clip = _barMusic[Random.Range(0, _barMusic.Length)];
                _source.Play();
                break;
            case MusicState.Battle:
                _source.clip = _battleMusic[Random.Range(0, _battleMusic.Length)];
                break;
        }
    }
}

public enum MusicState{ Bar, Battle}
