using UnityEngine;

public class MobileControlsSetup : MonoBehaviour
{
    [SerializeField] private PlayerTank playerTank;
    
    private void Awake()
    {
        // Временно отключаем мобильное управление
        gameObject.SetActive(false);
        return;

        /* Оригинальный код закомментирован
        #if !UNITY_ANDROID && !UNITY_IOS
        gameObject.SetActive(false);
        return;
        #endif

        // Создаем Canvas если его нет
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
        }

        // Добавляем CanvasScaler если его нет
        UnityEngine.UI.CanvasScaler scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Добавляем GraphicRaycaster если его нет
        UnityEngine.UI.GraphicRaycaster raycaster = GetComponent<UnityEngine.UI.GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Создаем UI управления
        MobileControlsUI controlsUI = GetComponent<MobileControlsUI>();
        if (controlsUI == null)
        {
            controlsUI = gameObject.AddComponent<MobileControlsUI>();
        }

        // Подключаем кнопки к танку
        if (playerTank != null)
        {
            playerTank.SetupMobileControls(
                controlsUI.GetUpButton(),
                controlsUI.GetRightButton(),
                controlsUI.GetDownButton(),
                controlsUI.GetLeftButton(),
                controlsUI.GetFireButton()
            );
        }
        */
    }
} 