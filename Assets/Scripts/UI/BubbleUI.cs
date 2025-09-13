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
        print(target.position);
        var screenPos = _cam.WorldToScreenPoint(target.position);
        _bubbleTf.position = screenPos; //new Vector3(screenPos.x, Screen.height - screenPos.y, screenPos.z);
    }

    public void SwapBubble()
    {
        foreach(var i in _backgroundImgs)
            i.rotation = Quaternion.Euler(0,i.eulerAngles.y + 180,0);
        
        if (_text.alignment == TextAlignmentOptions.Left) _text.alignment = TextAlignmentOptions.Right;
        else if (_text.alignment == TextAlignmentOptions.Right) _text.alignment = TextAlignmentOptions.Left;

    }
}
