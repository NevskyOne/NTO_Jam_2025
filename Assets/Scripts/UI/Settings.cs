using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private Slider[] _mainSliders;
    [SerializeField] private Slider[] _musicSliders;
    [SerializeField] private Slider[] _sfxSliders;
    private SoundData _data = new();

    private void Start() => RestoreData();
    
    public void ChangeMainSound(float value)
    {
        _mixer.SetFloat("MainVolume", Mathf.Log10(value) * 20);
        _data.MainSound = (int)(value * 100);
        SavingSystem.Save(Constants.SMainVolume, _data.MainSound);
        UpdateSliders();
    }
    
    public void ChangeMusicSound(float value)
    {
        _mixer.SetFloat("MainVolume", Mathf.Log10(value) * 20);
        _data.Music = (int)(value * 100);
        SavingSystem.Save(Constants.SMusicVolume, _data.Music);
        UpdateSliders();
    }
    
    public void ChangeSFXSound(float value)
    {
        _mixer.SetFloat("MainVolume", Mathf.Log10(value) * 20);
        _data.SFX = (int)(value * 100);
        SavingSystem.Save(Constants.SSFXVolume, _data.SFX);
        UpdateSliders();
    }

    private void UpdateSliders()
    {
        foreach (var slider in _mainSliders)
        {
            slider.value = _data.MainSound / 100f;
        }
        
        foreach (var slider in _musicSliders)
        {
            slider.value = _data.Music / 100f;
        }
        
        foreach (var slider in _sfxSliders)
        {
            slider.value = _data.SFX / 100f;
        }
    }

    private void RestoreData()
    {
        if (SavingSystem.Load(Constants.SNotFirstPlay) == 0)
        {
            _data.MainSound = 50;
            _data.Music = 50;
            _data.SFX = 50;
            SavingSystem.Save(Constants.MusicData, _data);
        }
        else
        {
            List<int> l = SavingSystem.Load(Constants.MusicData);
            _data.MainSound = l[0];
            _data.Music = l[1];
            _data.SFX = l[2];
        }

        UpdateSliders();
    }
}
