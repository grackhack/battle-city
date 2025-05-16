using UnityEngine;
using UnityEngine.UI;

public class MobileControlsUI : MonoBehaviour
{
    [Header("Кнопки управления")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button fireButton;

    [Header("Настройки кнопок")]
    [SerializeField] private float buttonSize = 100f;
    [SerializeField] private float buttonSpacing = 20f;
    [SerializeField] private float fireButtonSize = 120f;
    [SerializeField] private Color buttonColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color pressedColor = new Color(1f, 1f, 1f, 0.8f);

    private void Start()
    {
        CreateControlButtons();
    }

    private void CreateControlButtons()
    {
        // Создаем контейнер для кнопок движения
        GameObject movementContainer = new GameObject("MovementButtons");
        movementContainer.transform.SetParent(transform);
        RectTransform movementRect = movementContainer.AddComponent<RectTransform>();
        movementRect.anchorMin = new Vector2(0, 0);
        movementRect.anchorMax = new Vector2(0, 0);
        movementRect.pivot = new Vector2(0, 0);
        movementRect.anchoredPosition = new Vector2(buttonSpacing, buttonSpacing);
        movementRect.sizeDelta = new Vector2(buttonSize * 3 + buttonSpacing * 2, buttonSize * 3 + buttonSpacing * 2);

        // Создаем кнопки движения
        upButton = CreateButton("UpButton", movementContainer.transform, new Vector2(buttonSize, buttonSize), new Vector2(buttonSize + buttonSpacing, buttonSize * 2 + buttonSpacing));
        rightButton = CreateButton("RightButton", movementContainer.transform, new Vector2(buttonSize, buttonSize), new Vector2(buttonSize * 2 + buttonSpacing * 2, buttonSize + buttonSpacing));
        downButton = CreateButton("DownButton", movementContainer.transform, new Vector2(buttonSize, buttonSize), new Vector2(buttonSize + buttonSpacing, 0));
        leftButton = CreateButton("LeftButton", movementContainer.transform, new Vector2(buttonSize, buttonSize), new Vector2(0, buttonSize + buttonSpacing));

        // Создаем кнопку стрельбы
        GameObject fireContainer = new GameObject("FireButton");
        fireContainer.transform.SetParent(transform);
        RectTransform fireRect = fireContainer.AddComponent<RectTransform>();
        fireRect.anchorMin = new Vector2(1, 0);
        fireRect.anchorMax = new Vector2(1, 0);
        fireRect.pivot = new Vector2(1, 0);
        fireRect.anchoredPosition = new Vector2(-buttonSpacing, buttonSpacing);
        fireRect.sizeDelta = new Vector2(fireButtonSize, fireButtonSize);

        fireButton = CreateButton("FireButton", fireContainer.transform, new Vector2(fireButtonSize, fireButtonSize), Vector2.zero);

        // Настраиваем цвета кнопок
        ColorBlock colors = new ColorBlock();
        colors.normalColor = buttonColor;
        colors.highlightedColor = pressedColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = pressedColor;
        colors.disabledColor = buttonColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;

        upButton.colors = colors;
        rightButton.colors = colors;
        downButton.colors = colors;
        leftButton.colors = colors;
        fireButton.colors = colors;

        // Добавляем иконки на кнопки
        AddButtonIcon(upButton, "↑");
        AddButtonIcon(rightButton, "→");
        AddButtonIcon(downButton, "↓");
        AddButtonIcon(leftButton, "←");
        AddButtonIcon(fireButton, "●");
    }

    private Button CreateButton(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        
        return button;
    }

    private void AddButtonIcon(Button button, string text)
    {
        GameObject textObj = new GameObject("Icon");
        textObj.transform.SetParent(button.transform);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 40;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.black;
    }

    public Button GetUpButton() => upButton;
    public Button GetRightButton() => rightButton;
    public Button GetDownButton() => downButton;
    public Button GetLeftButton() => leftButton;
    public Button GetFireButton() => fireButton;
} 