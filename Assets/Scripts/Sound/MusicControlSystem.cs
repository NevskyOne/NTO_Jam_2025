using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicControlSystem : MonoBehaviour
{
    [SerializeField] private MusicState _state;
    [SerializeField] private List<MusicStateData> _musicData;
    [SerializeField] private AudioSource _source;
    
    public void ChangeMusicState(){}

    public void Update()
    {
        if (_source.isPlaying) return;

        var musArray = _musicData.Find(x => x.State == _state).Music;
        _source.clip = musArray[Random.Range(0, musArray.Length)];
        _source.Play();
    }
}

public enum MusicState{ Bar, Battle}
