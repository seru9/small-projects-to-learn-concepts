using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI timerText;
	[SerializeField] private GameObject gameOverUI;
	public static UI instance;
	// Update is called once per frame
	private void Awake()
	{
		instance = this;
		Time.timeScale = 1f;
	}
	private void Update()
    {
        timerText.text = Time.time.ToString("F2") + "s";
	}
	public void RestartLevel()
	{
		int sceneIndex = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(sceneIndex);
	}
	public void EnableGameOverUI()
	{
		Time.timeScale = .5f;
		gameOverUI.SetActive(true);
		
	}
}
 