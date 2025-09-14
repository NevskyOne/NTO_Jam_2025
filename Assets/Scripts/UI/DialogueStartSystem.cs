using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using Zenject;

[RequireComponent(typeof(DialogueRunner))]
public class DialogueStartSystem : MonoBehaviour
{
    [SerializeField] private BubbleUI _bubble;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private InputActionReference _action;

    private string _lastName;
    private Transform _playerTf, _characterTf;
    private PlayerInput _input;
    private DialogueRunner Runner => GetComponent<DialogueRunner>();

    [Inject]
    public void Construct(Player player, PlayerInput input)
    {
        _playerTf = player.DialogueBubblePos;
        _input = input;
        _action.action.performed += OnEndLine;
        _input.enabled = false;
    }
    
    
    public void StartDialogue(string startNode, Transform charTf)
    {
        Runner.StartDialogue(startNode);
        _characterTf = charTf;
        _bubble.MoveToTarget(_name.text == "Player" ? _playerTf : _characterTf);
        
        if (_playerTf.position.x <= _characterTf.position.x && _name.text != "Player")
        {
            _bubble.SwapBubble();
        }
        else if (_playerTf.position.x > _characterTf.position.x && _name.text == "Player")
        {
            _bubble.SwapBubble();
        }
        
        _lastName = _name.text;
    }

    private async void OnEndLine(InputAction.CallbackContext callbackContext)
    {
        await Task.Delay(5);
        if (_lastName == _name.text) return;
        print(_lastName + _name.text);
        
        _bubble.MoveToTarget(_name.text == "Player" ? _playerTf : _characterTf);
        _bubble.SwapBubble();
        
        _lastName = _name.text;
    }

    public void EndDialogue()
    {
        _input.enabled = true;
    }
    
    
}
