using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Yarn.Unity;
using Zenject;

public class MainUIInteractions : MonoBehaviour, IDisposable
{
    [Header("UI")]
    [SerializeField] private GameObject _gameplay;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _shop;
    [SerializeField] private GameObject _inventory;
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private GameObject _levelSelectionMenu;
    [SerializeField] private GameObject _levelCompleteMenu;
    [Header("ShopCam")]
    [SerializeField] private float _lerpSpeed;
    [SerializeField] private Transform _newCamPos;
    [SerializeField] private LayerMask _layerNoCharacters;
    [SerializeField] private int _targetPixelRes;
    [Header("text")] 
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _repText;
    
    private Vector2 _lastCamPos;
    private LayerMask _defaultLayer;
    private int _lastPixelRes;
    private PlayerInput _playerInput;
    private Player _player;
    private Camera _cam;
    private PixelPerfectCamera _pixelCam;

    private Coroutine _zoomRoutine;
    
    private UIState _state = UIState.Gameplay;

    [Inject]
    private void Construct(PlayerInput input, Camera cam, Player player, PixelPerfectCamera pixelCam)
    {
        _playerInput = input;
        _player = player;
        _cam = cam;
        _pixelCam = pixelCam;

        _playerInput.actions["Escape"].performed += CloseUI;
        _playerInput.actions["I"].performed += _ => ChangeInventoryState(true);
    }

    public void Dispose()
    {
        _playerInput.actions["Escape"].performed -= CloseUI;
        _playerInput.actions["I"].performed += _ => ChangeInventoryState(true);
    }

    private void CloseUI(InputAction.CallbackContext obj)
    {
        switch (_state)
        {
            case UIState.Gameplay:
                ChangePauseState(true);
                break;
            case UIState.Pause:
                ChangePauseState(false);
                break;
            case UIState.Shop:
                ChangeShopState(false);
                break;
            case UIState.Inventory:
                ChangeInventoryState(false);
                break;
        }
    }
    
    public void ChangePauseState(bool active)
    {
        _pauseMenu.SetActive(active);
        _gameplay.SetActive(!active);
        _state = active? UIState.Pause : UIState.Gameplay;
    }
    
    public void ChangeLevelSelectionState(bool active)
    {
        _levelSelectionMenu.SetActive(active);
        _gameplay.SetActive(!active);
        _state = active? UIState.LevelSelection : UIState.Gameplay;
    }
    
    public void ChangeInventoryState(bool active)
    {
        _inventory.SetActive(active);
        _state = active? UIState.Inventory : UIState.Gameplay;
    }
    
    public void ChangeDeathState(bool active)
    {
        _deathScreen.SetActive(active);
        _gameplay.SetActive(!active);
        _state = active? UIState.Dead : UIState.Gameplay;
    }
    
    [YarnCommand("OpenShop")]
    public void ChangeShopState(bool active)
    {
        if (active)
        {
            _lastCamPos = _player.CameraTarget.position;
            _defaultLayer = _cam.cullingMask;
            _lastPixelRes = _pixelCam.assetsPPU;
            
            _player.CameraTarget.position = _newCamPos.position;
            _cam.cullingMask = _layerNoCharacters;
            if(_zoomRoutine != null) StopCoroutine(_zoomRoutine);
            _zoomRoutine = StartCoroutine(CamZoomRoutine(_pixelCam.assetsPPU, _targetPixelRes));
        }
        else
        {
            _player.CameraTarget.position = _lastCamPos;
            _cam.cullingMask = _defaultLayer;
            if(_zoomRoutine != null) StopCoroutine(_zoomRoutine);
            _zoomRoutine = StartCoroutine(CamZoomRoutine(_pixelCam.assetsPPU, _lastPixelRes));
        }
        _shop.SetActive(active);
        _state = active? UIState.Shop : UIState.Gameplay;
    }

    private IEnumerator CamZoomRoutine(float from, float to)
    {
        var lerpProgress = 0f;
        while (_pixelCam.assetsPPU != (int)to)
        {
            yield return new WaitForFixedUpdate();
            lerpProgress += Time.fixedDeltaTime * _lerpSpeed;
            _pixelCam.assetsPPU = (int)Mathf.Lerp(from, to, lerpProgress);
        }

        _zoomRoutine = null;
    }

    public void UpdateText()
    {
        _moneyText.text = _player.Data.Money.ToString();
        _repText.text = _player.Data.Reputation.ToString();
    }
}

public enum UIState
{
    Gameplay,
    Pause,
    CutScene,
    Inventory,
    LevelSelection,
    Shop,
    Dead
}
