using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using Zenject;

public class LevelSystem : MonoBehaviour
{
	[SerializeField] private GameObject _doorEnd;
	private LevelData _levelData;
	private MainUIInteractions _mainUI;
	private Player _player; 
	
	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}
	
	[Inject]
	public void Construct(MainUIInteractions mainUI, Player player)
	{
		_mainUI = mainUI;
		_player = player;
	}

	[YarnCommand("RequestNewLevel")]
	public void RequestNewLevel(int id)
	{
	    _levelData = new LevelData(id);
		_mainUI.ChangeLevelSelectionState(true);
	}

	[YarnCommand("ShowEnding")]
	public void ShowEnding()
	{
		_doorEnd.SetActive(true);
	}

	public void ChangeOneHp(bool active)
	{
		_levelData.OneHp = active;
	}
	
	public void ChangeNoHeal(bool active)
	{
		_levelData.NoHeal = active;
	}

	[YarnCommand("LoadLevel")]
	public void LoadLevel()
	{
		SceneManager.LoadScene(_levelData.SceneId);
		if(_levelData.SceneId == 0) _player.AddReputation();
	}
}
