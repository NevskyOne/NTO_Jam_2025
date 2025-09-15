using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;
using Zenject;

[RequireComponent(typeof(DialogueRunner))]
public class DialogueStartSystem : MonoBehaviour
{
    [Header("Parameter")]
    [SerializeField] private float _offset;
    [Header("Parameter")]
    [SerializeField] private BubbleUI _bubble;
    [SerializeField] private TMP_Text _name;
    [SerializeField] private Image _dialogBack;
    [SerializeField] private Sprite _npcBackSprite;
    [SerializeField] private Sprite _playerBackSprite;
    [SerializeField] private InputActionReference _action;
    [SerializeField] private string _actionMapPlayer;
    [SerializeField] private string _actionMapToSwitch;

    private string _lastName;
    private PlayerInput _playerInput;
    private Transform _playerTf, _characterTf;
    private DialogueRunner Runner => GetComponent<DialogueRunner>();

    [Inject]
    public void Construct(Player player, PlayerInput input)
    {
        _playerTf = player.DialogueBubblePos;
        _playerInput = input;
        _action.action.performed += OnEndLine;
    }
    
    
    public void StartDialogue(string startNode, Transform charTf)
    {
        Runner.StartDialogue(startNode);
        _characterTf = charTf;
        
        if (_playerTf.position.x <= _characterTf.position.x)
        {
            _characterTf.localPosition += new Vector3(_offset,0,0);
            _playerTf.localPosition -= new Vector3(_offset,0,0);
            if (_name.text != "Player")
            {
                _bubble.SwapBubble();
            }
        }
        else if (_playerTf.position.x > _characterTf.position.x )
        {
            _characterTf.localPosition -= new Vector3(_offset,0,0);
            _playerTf.localPosition += new Vector3(_offset,0,0);
            if (_name.text == "Player")
            {
                _bubble.SwapBubble();
            }
        }
        _dialogBack.sprite = _name.text == "Player" ? _playerBackSprite : _npcBackSprite;
        _bubble.MoveToTarget(_name.text == "Player" ? _playerTf : _characterTf);
        
        _lastName = _name.text;
        _playerInput.SwitchCurrentActionMap(_actionMapToSwitch);
    }

    private async void OnEndLine(InputAction.CallbackContext callbackContext)
    {
        await Task.Delay(5);
        if (_lastName == _name.text) return;
        print(_lastName + _name.text);
        
        _dialogBack.sprite = _name.text == "Player" ? _playerBackSprite : _npcBackSprite;
        
        _bubble.MoveToTarget(_name.text == "Player" ? _playerTf : _characterTf);
        _bubble.SwapBubble();
        
        _lastName = _name.text;
    }

    public void EndDialogue()
    {
        _playerInput.SwitchCurrentActionMap(_actionMapPlayer);
        _characterTf.localPosition = new Vector3(0,_characterTf.localPosition.y,_characterTf.localPosition.z);
        _playerTf.localPosition = new Vector3(0,_playerTf.localPosition.y,_playerTf.localPosition.z);
    }
    
    
}
