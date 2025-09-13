using System.Collections.Generic;

public static class Constants
{
    public const string SNotFirstPlay = "NotFirstPlay";

    public const string SPlayerHealth = "PlayerHealth";
    public const string SPlayerMoney = "PlayerMoney";
    public const string SPlayerreputation = "Playerreputation";

    public const string SMainVolume = "MainVolume";
    public const string SMusicVolume = "MusicVolume";
    public const string SSFXVolume = "SFXVolume";
    public static List<string> MusicData = new(){
        SMainVolume, SMusicVolume, SSFXVolume
    };
}
