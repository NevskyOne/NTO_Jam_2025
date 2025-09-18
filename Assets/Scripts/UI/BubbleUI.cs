using TMPro;
using UnityEngine;
using Zenject;

public class BubbleUI : MonoBehaviour
{
    [SerializeField] private Transform _bubbleTf;
    [SerializeField] private Transform[] _backgroundImgs;
    [SerializeField] private TMP_Text _text;

    private Camera _cam;
    
    [Inject]
    public void Construct(Camera cam)
    {
        _cam = cam;
    }
    
    public void MoveToTarget(Transform target)
    {
        var screenPos = _cam.WorldToScreenPoint(target.position);
        _bubbleTf.position = screenPos; 
    }

    public void SwapBubble()
    {
        
        if (_text.alignment == TextAlignmentOptions.Left) _text.alignment = TextAlignmentOptions.Right;
        else if (_text.alignment == TextAlignmentOptions.Right) _text.alignment = TextAlignmentOptions.Left;

    }
}
