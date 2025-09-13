using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image HPBar;
    [SerializeField] private List<Sprite> HPConditions;

    private Player _player;
    
    private void Construct(Player player)
    {
        _player = player;
    }
    
    public void UpdateHP()
    {
        
    }
}
