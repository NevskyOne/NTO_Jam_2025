using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public new static Bootstrap Instantiate { get; private set; }

    private void Start()
    {
        Instantiate = this;
    }
}

