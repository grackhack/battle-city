using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Сцены")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string levelBuilderSceneName = "LevelBuilder";

    [Header("UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button createLevelButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        ValidateReferences();
        SetupButtons();
    }

    private void ValidateReferences()
    {
        if (playButton == null) 
        {
            Debug.LogError("Play Button не назначен!");
        }
        else
        {
            Text playText = playButton.GetComponentInChildren<Text>();
            if (playText == null)
            {
                Debug.LogError("Play Button не имеет дочернего объекта с компонентом Text!");
            }
        }

        if (createLevelButton == null) 
        {
            Debug.LogError("Create Level Button не назначен!");
        }
        else
        {
            Text createText = createLevelButton.GetComponentInChildren<Text>();
            if (createText == null)
            {
                Debug.LogError("Create Level Button не имеет дочернего объекта с компонентом Text!");
            }
        }

        if (quitButton == null) 
        {
            Debug.LogError("Quit Button не назначен!");
        }
        else
        {
            Text quitText = quitButton.GetComponentInChildren<Text>();
            if (quitText == null)
            {
                Debug.LogError("Quit Button не имеет дочернего объекта с компонентом Text!");
            }
        }
    }

    private void SetupButtons()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (createLevelButton != null) createLevelButton.onClick.AddListener(OnCreateLevelClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        // Начинаем с первого уровня
        PlayerPrefs.SetString("SelectedLevel", "1");
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnCreateLevelClicked()
    {
        SceneManager.LoadScene(levelBuilderSceneName);
    }

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 