using UnityEngine;
using Zenject;

public class Player : MonoBehaviour
{
    private PlayerConfig _config;

    [Inject]
    private void Construct(PlayerConfig config)
    {
        _config = config;
    }
}

