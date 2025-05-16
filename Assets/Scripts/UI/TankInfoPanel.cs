using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TankInfoPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI currentPositionText;
    [SerializeField] private TextMeshProUGUI targetPositionText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI directionText;
    [SerializeField] private TextMeshProUGUI movingText;

    private PlayerTank playerTank;

    private void Start()
    {
        playerTank = FindFirstObjectByType<PlayerTank>();
        if (playerTank == null)
        {
            Debug.LogError("PlayerTank не найден на сцене!");
            return;
        }

        // Подписываемся на обновление информации
        UpdateInfo();
    }

    private void Update()
    {
        if (playerTank == null) return;

        UpdateInfo();
    }

    private void UpdateInfo()
    {
        UpdatePositionTexts();
        UpdateSpeedText();
        UpdateDirectionText();
        UpdateMovementText();
    }

    private void UpdatePositionTexts()
    {
        if (currentPositionText != null)
        {
            Vector2 currentPos = playerTank.transform.position;
            currentPositionText.text = $"Текущая позиция: X: {currentPos.x:F2}, Y: {currentPos.y:F2}";
        }

        if (targetPositionText != null)
        {
            Vector2 targetPos = playerTank.TargetPosition;
            targetPositionText.text = $"Целевая позиция: X: {targetPos.x:F2}, Y: {targetPos.y:F2}";
        }
    }

    private void UpdateSpeedText()
    {
        if (speedText != null)
        {
            speedText.text = $"Скорость: {playerTank.MoveSpeed:F1}";
        }
    }

    private void UpdateDirectionText()
    {
        if (directionText != null)
        {
            string direction = playerTank.CurrentDirection switch
            {
                0 => "Вверх",
                1 => "Вправо",
                2 => "Вниз",
                3 => "Влево",
                _ => "Неизвестно"
            };
            directionText.text = $"Направление: {direction}";
        }
    }

    private void UpdateMovementText()
    {
        if (movingText != null)
        {
            movingText.text = $"Движение: {(playerTank.IsMoving ? "Да" : "Нет")}";
        }
    }
} 