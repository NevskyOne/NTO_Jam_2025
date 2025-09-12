using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using Zenject;

public class DialogueStartSystem : MonoBehaviour
{
    private Transform _playerTf, _characterTf;
    [SerializeField] private BubbleUI _bubble;
    [SerializeField] private DialogueRunner _runner;
    [SerializeField] private TMP_Text _name;

    private string _lastName;

    [Inject]
    public void Construct(Player player, PlayerInput input)
    {
        _playerTf = player.transform;
    }
    
    
    public void StartDialogue(string startNode, Transform charTf)
    {
        _runner.StartDialogue(startNode);
        _characterTf = charTf;
        _bubble.MoveToTarget(_name.text == "Player" ? _playerTf : _characterTf);
        
        if (_playerTf.position.x <= _characterTf.position.x && _name.text != "Player")
        {
            _bubble.SwapImage();
        }
        else if (_playerTf.position.x > _characterTf.position.x && _name.text == "Player")
        {
            _bubble.SwapImage();
        }
        
        _lastName = _name.text;
    }

    public void OnEndLine()
    {
        print(_name.text);
        if (_lastName == _name.text) return;
        
        _bubble.MoveToTarget(_name.text == "Player" ? _playerTf : _characterTf);
        _bubble.SwapImage();
        
        _lastName = _name.text;
    }
}