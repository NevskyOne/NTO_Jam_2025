using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using Zenject;

public class MainUIInteractions : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _paper;
    private UIState _state = UIState.CutScene;

    [Inject]
    private void Construct(PlayerInput input)
    {
        input.actions["Escape"].performed += CloseUI;
    }

    private void CloseUI(InputAction.CallbackContext obj)
    {
        switch (_state)
        {
            case UIState.Gameplay:
                _pauseMenu.SetActive(true);
                _state = UIState.Pause;
                break;
            case UIState.Pause:
                _pauseMenu.SetActive(false);
                _state = UIState.Gameplay;
                break;
            case UIState.ClosablePaper:
                _paper.SetActive(false);
                _state = UIState.Gameplay;
                break;
        }
    }

    public void ChangePaperState(bool active)
    {
        
    }
    
    
    [YarnCommand("OpenShop")]
    public void ChangeShopState(bool active)
    {
        
    }
}

public enum UIState
{
    Gameplay,
    Pause,
    CutScene,
    ClosablePaper,
    NotClosablepaper,
    Shop
}
