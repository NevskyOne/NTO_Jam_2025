using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using Zenject;

public class LevelSystem : MonoBehaviour
{
	private LevelData _levelData;
	private MainUIInteractions _mainUI;
	
	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}
	
	[Inject]
	public void Construct(MainUIInteractions mainUI)
	{
		_mainUI = mainUI;
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
