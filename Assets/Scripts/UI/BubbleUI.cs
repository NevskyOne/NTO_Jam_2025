using TMPro;
using UnityEngine;

public class BubbleUI : MonoBehaviour
{
    [SerializeField] private Transform _bubbleTf;
    [SerializeField] private Transform[] _backgroundImgs;
    [SerializeField] private TMP_Text[] _texts;

    public void MoveToTarget(Transform target)
    {
        var screenPos = Camera.main.WorldToScreenPoint(target.position);
        _bubbleTf.position = screenPos; //new Vector3(screenPos.x, Screen.height - screenPos.y, screenPos.z);
    }

    public void SwapImage()
    {
        foreach(var i in _backgroundImgs)
            i.rotation = Quaternion.Euler(0,i.eulerAngles.y + 180,0);

        foreach (var i in _texts)
        {
            if (i.alignment == TextAlignmentOptions.Left) i.alignment = TextAlignmentOptions.Right;
            else if (i.alignment == TextAlignmentOptions.Right) i.alignment = TextAlignmentOptions.Left;
        }
    }
}
