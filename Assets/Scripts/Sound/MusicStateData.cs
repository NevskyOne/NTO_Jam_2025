using System;
using UnityEngine;

[Serializable]
public struct MusicStateData
{
    [field: SerializeField] public MusicState State { get; private set; }
    [field: SerializeField] public AudioClip[] Music { get; private set; }
}
