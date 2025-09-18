using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using Zenject;

public class LevelSystem : MonoBehaviour
{
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

	public void ChangeOneHp(bool active)
	{
		_levelData.OneHp = active;
	}
	
	public void ChangeNoHeal(bool active)
	{
		_levelData.NoHeal = active;
	}

	public void LoadLevel()
	{
		SceneManager.LoadScene(_levelData.SceneId);
	}
}
