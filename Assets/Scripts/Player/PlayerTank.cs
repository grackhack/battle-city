using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerTank : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 16f; // Скорость в пикселях в секунду
    
    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.5f;

    // Публичные свойства для UI
    public float MoveSpeed => moveSpeed;
    public int CurrentDirection => currentDirection;
    public bool IsMoving => isMoving;
    public Vector2 TargetPosition => targetPosition;
    
    private const float PIXELS_PER_UNIT = 16f; // 16 пикселей = 1 юнит Unity
    private const float TANK_SIZE_PIXELS = 16f; // Размер спрайта танка в пикселях
    private const float TANK_COLLIDER_SIZE_PIXELS = 14f; // Размер хитбокса танка в пикселях
    private const float WALL_SIZE_PIXELS = 16f; // Размер стены в пикселях (полное перекрытие клетки)
    private const float PIXEL_SIZE = 1f / PIXELS_PER_UNIT; // Размер одного пикселя в юнитах
    private const float SAFE_DISTANCE_PIXELS = 1f; // Безопасное расстояние в пикселях
    
    private float nextFireTime;
    private bool isMoving;
    private Vector2 moveDirection;
    private int currentDirection; // 0: вверх, 1: вправо, 2: вниз, 3: влево
    private BoxCollider2D tankCollider;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction fireAction;
    private Vector2 targetPosition;
    private bool wasBlocked; // Флаг, что танк был заблокирован стеной
    private Vector2 lastBlockedDirection; // Последнее направление, в котором был заблокирован танк
    private bool canMove;
    private Vector2 frontLeft;
    private Vector2 frontLeftMid;
    private Vector2 frontLeftCenter;
    private Vector2 frontLeftCenterNear;
    private Vector2 frontCenter;
    private Vector2 frontRightCenterNear;
    private Vector2 frontRightCenter;
    private Vector2 frontRightMid;
    private Vector2 frontRight;
    private bool[] collisionPoints;
    private Vector2 lastLoggedPosition;
    private int lastLoggedColliderCount;
    private Dictionary<string, Vector2> lastColliderSizes = new Dictionary<string, Vector2>();
    private float lastMinEdgeDistance = float.MaxValue;
    
    // Массивы для хранения логов
    private List<string> positionLogs = new List<string>();
    private List<string> collisionLogs = new List<string>();
    private List<string> stateLogs = new List<string>();
    private const int MAX_LOGS = 10; // Максимальное количество сохраняемых логов

    private Vector2 lastWallPosition;
    private float lastWallSize;

    private void AddLog(List<string> logList, string log)
    {
        logList.Add(log);
        if (logList.Count > MAX_LOGS)
        {
            logList.RemoveAt(0);
        }
    }

    private void PrintLogs()
    {
        string allLogs = "=== TANK LOGS ===\n";
        
        allLogs += "POS: ";
        allLogs += string.Join(" | ", positionLogs);
        allLogs += "\n";
        
        allLogs += "COL: ";
        allLogs += string.Join(" | ", collisionLogs);
        allLogs += "\n";
        
        allLogs += "STA: ";
        allLogs += string.Join(" | ", stateLogs);
        
        Debug.Log(allLogs);
    }

    private void Awake()
    {
        // Проверяем и добавляем необходимые компоненты
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        if (GetComponent<PlayerInput>() == null)
        {
            gameObject.AddComponent<PlayerInput>();
        }

        if (GetComponent<SpriteRenderer>() == null)
        {
            gameObject.AddComponent<SpriteRenderer>();
        }

        // Получаем компоненты
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            fireAction = playerInput.actions["Fire"];
        }
        else
        {
            Debug.LogError("PlayerInput компонент не найден!");
        }

        tankCollider = GetComponent<BoxCollider2D>();
        if (tankCollider == null)
        {
            Debug.LogError("BoxCollider2D компонент не найден!");
        }
        else
        {
            // Настраиваем коллайдер для пиксельной игры
            tankCollider.size = new Vector2(TANK_COLLIDER_SIZE_PIXELS/PIXELS_PER_UNIT, TANK_COLLIDER_SIZE_PIXELS/PIXELS_PER_UNIT);
            tankCollider.offset = Vector2.zero;
            tankCollider.isTrigger = false; // Включаем физические коллизии
            
            // Устанавливаем слой Player
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        Debug.Log($"TANK_INIT: Position={transform.position}, Collider size={tankCollider.size}, Layer={gameObject.layer}");

        // Инициализация точек коллизии
        collisionPoints = new bool[9];
        UpdateCollisionPoints();
    }

    private void Start()
    {
        // Устанавливаем начальную позицию и целевую позицию
        targetPosition = transform.position;
        Debug.Log($"PlayerTank Start: позиция = {transform.position}, целевая = {targetPosition}");
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.Enable();
        if (fireAction != null) fireAction.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (fireAction != null) fireAction.Disable();
    }

    private void Update()
    {
        HandleInput();
        HandleShooting();
        DrawDebugInfo(); // Отрисовка отладочной информации каждый кадр
    }

    private void HandleInput()
    {
        if (moveAction == null) return;

        // 1. Получение ввода
        Vector2 input = moveAction.ReadValue<Vector2>();
        float horizontal = input.x;
        float vertical = input.y;

        // Сохраняем предыдущее состояние
        bool wasMoving = isMoving;
        Vector2 oldMoveDirection = moveDirection;
        int oldDirection = currentDirection;

        // 2. Определение направления движения
        if (horizontal != 0)
        {
            moveDirection = new Vector2(horizontal, 0);
            currentDirection = horizontal > 0 ? 1 : 3;
            transform.rotation = Quaternion.Euler(0, 0, -currentDirection * 90);
            isMoving = true;
        }
        else if (vertical != 0)
        {
            moveDirection = new Vector2(0, vertical);
            currentDirection = vertical > 0 ? 0 : 2;
            transform.rotation = Quaternion.Euler(0, 0, -currentDirection * 90);
            isMoving = true;
        }
        else
        {
            isMoving = false;
            moveDirection = Vector2.zero;
        }

        // Логируем только при изменении состояния
        if (wasMoving != isMoving || oldMoveDirection != moveDirection)
        {
            string directionName = currentDirection switch
            {
                0 => "UP",
                1 => "RIGHT",
                2 => "DOWN",
                3 => "LEFT",
                _ => "UNKNOWN"
            };
            string log = $"dir={directionName} moving={isMoving.ToString().ToLower()} dir=({moveDirection.x.ToString("F1", CultureInfo.InvariantCulture)},{moveDirection.y.ToString("F1", CultureInfo.InvariantCulture)})";
            AddLog(stateLogs, log);
        }
    }

    private void HandleShooting()
    {
        if (fireAction != null && fireAction.triggered)
        {
            Fire();
        }
    }

    private void FixedUpdate()
    {
        if (isMoving && moveDirection != Vector2.zero)
        {
            // 1. Расчёт новой позиции (1 пиксель за шаг)
            Vector2 currentPos = (Vector2)transform.position;
            
            // Выравниваем текущую позицию по сетке 16x16
            currentPos.x = Mathf.Round(currentPos.x * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
            currentPos.y = Mathf.Round(currentPos.y * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
            
            // Вычисляем следующую позицию
            Vector2 nextPos = currentPos + (moveDirection * PIXEL_SIZE);
            
            // Выравниваем следующую позицию по сетке 16x16
            nextPos.x = Mathf.Round(nextPos.x * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
            nextPos.y = Mathf.Round(nextPos.y * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;

            // 2. Проверка AABB коллизии
            if (!CheckAABBCollision(nextPos))
            {
                transform.position = nextPos;
            }
            else
            {
                // 3. Если есть коллизия - блокируем движение и выравниваем по сетке
                isMoving = false;
                
                // Выравниваем по сетке в направлении движения
                Vector2 alignedPos = currentPos;
                float tankHalfSize = TANK_COLLIDER_SIZE_PIXELS / 2f / PIXELS_PER_UNIT;
                float wallPos = 0f;
                float distanceToWall = 0f;
                float wallEdge = 0f;

                if (moveDirection.x != 0)
                {
                    // Выравниваем по X с учетом направления движения
                    wallPos = lastWallPosition.x;
                    float wallHalfSize = lastWallSize / 2f;
                    wallEdge = wallPos - (moveDirection.x * wallHalfSize);
                    distanceToWall = Mathf.Abs(currentPos.x - wallEdge);
                    
                    // Выравниваем танк по сетке 16x16 с учетом безопасного расстояния
                    float safeDistance = SAFE_DISTANCE_PIXELS * PIXEL_SIZE;
                    float alignedX = wallEdge - (moveDirection.x * (tankHalfSize + safeDistance));
                    alignedPos.x = Mathf.Round(alignedX * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
                    alignedPos.y = Mathf.Round(currentPos.y * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
                }
                else if (moveDirection.y != 0)
                {
                    // Выравниваем по Y с учетом направления движения
                    wallPos = lastWallPosition.y;
                    float wallHalfSize = lastWallSize / 2f;
                    wallEdge = wallPos - (moveDirection.y * wallHalfSize);
                    distanceToWall = Mathf.Abs(currentPos.y - wallEdge);
                    
                    // Выравниваем танк по сетке 16x16 с учетом безопасного расстояния
                    float safeDistance = SAFE_DISTANCE_PIXELS * PIXEL_SIZE;
                    float alignedY = wallEdge - (moveDirection.y * (tankHalfSize + safeDistance));
                    alignedPos.x = Mathf.Round(currentPos.x * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
                    alignedPos.y = Mathf.Round(alignedY * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
                }
                transform.position = alignedPos;
                
                string log = $"stop: pos=({alignedPos.x.ToString("F3", CultureInfo.InvariantCulture)},{alignedPos.y.ToString("F3", CultureInfo.InvariantCulture)}) " +
                           $"from=({currentPos.x.ToString("F3", CultureInfo.InvariantCulture)},{currentPos.y.ToString("F3", CultureInfo.InvariantCulture)}) " +
                           $"wall={wallPos.ToString("F3", CultureInfo.InvariantCulture)} " +
                           $"edge={wallEdge.ToString("F3", CultureInfo.InvariantCulture)} " +
                           $"dist={distanceToWall.ToString("F3", CultureInfo.InvariantCulture)} " +
                           $"size={tankHalfSize.ToString("F3", CultureInfo.InvariantCulture)} " +
                           $"dir=({moveDirection.x.ToString("F1", CultureInfo.InvariantCulture)},{moveDirection.y.ToString("F1", CultureInfo.InvariantCulture)})";
                AddLog(collisionLogs, log);
                
                PrintLogs();
            }
        }
    }

    private bool CheckAABBCollision(Vector2 position)
    {
        // Создаем AABB для танка в пикселях (14x14)
        float halfSizePixels = TANK_COLLIDER_SIZE_PIXELS / 2f;
        Vector2 halfSize = new Vector2(halfSizePixels * PIXEL_SIZE, halfSizePixels * PIXEL_SIZE);
        
        // Выравниваем позицию по сетке 16x16
        position.x = Mathf.Round(position.x * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
        position.y = Mathf.Round(position.y * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
        
        // Создаем AABB с учетом направления движения и безопасного расстояния
        Vector2 min, max;
        float safeDistance = SAFE_DISTANCE_PIXELS * PIXEL_SIZE;
        
        if (moveDirection.x != 0)
        {
            // Движение по горизонтали
            float centerY = position.y;
            float checkX = moveDirection.x > 0 ? position.x + halfSize.x : position.x - halfSize.x;
            min = new Vector2(checkX, centerY - halfSize.y);
            max = new Vector2(checkX + (moveDirection.x * (PIXEL_SIZE + safeDistance)), centerY + halfSize.y);
        }
        else if (moveDirection.y != 0)
        {
            // Движение по вертикали
            float centerX = position.x;
            float checkY = moveDirection.y > 0 ? position.y + halfSize.y : position.y - halfSize.y;
            min = new Vector2(centerX - halfSize.x, checkY);
            max = new Vector2(centerX + halfSize.x, checkY + (moveDirection.y * (PIXEL_SIZE + safeDistance)));
        }
        else
        {
            // Нет движения
            min = position - halfSize;
            max = position + halfSize;
        }

        // Проверяем коллизии только со стенами
        int wallLayer = LayerMask.NameToLayer("Wall");
        if (wallLayer == -1)
        {
            Debug.LogError("Layer 'Wall' не найден! Создайте слой Wall в настройках проекта.");
            return false;
        }

        // Проверяем AABB коллизии
        Collider2D[] colliders = Physics2D.OverlapAreaAll(min, max, 1 << wallLayer);

        if (colliders.Length > 0)
        {
            // Находим минимальное расстояние до края стены
            float minEdgeDistance = float.MaxValue;
            Vector2 closestWallCenter = Vector2.zero;
            float closestWallSize = WALL_SIZE_PIXELS / PIXELS_PER_UNIT; // Размер стены в юнитах

            foreach (var collider in colliders)
            {
                Vector2 colliderCenter = collider.bounds.center;
                float distance;
                if (moveDirection.x != 0)
                {
                    distance = Mathf.Abs(position.x - colliderCenter.x);
                }
                else if (moveDirection.y != 0)
                {
                    distance = Mathf.Abs(position.y - colliderCenter.y);
                }
                else
                {
                    distance = Vector2.Distance(position, colliderCenter);
                }
                
                // Учитываем безопасное расстояние при расчете
                float edgeDistance = distance - (halfSize.x + closestWallSize/2 + safeDistance);
                
                if (edgeDistance < minEdgeDistance)
                {
                    minEdgeDistance = edgeDistance;
                    closestWallCenter = colliderCenter;
                }
            }

            // Сохраняем информацию о стене
            lastWallPosition = closestWallCenter;
            lastWallSize = closestWallSize;

            // Проверяем безопасное расстояние до стены
            if (minEdgeDistance < 0)
            {
                // Логируем только при значительном изменении расстояния
                bool shouldLog = Mathf.Abs(minEdgeDistance - lastMinEdgeDistance) > 0.001f;
                if (shouldLog)
                {
                    string log = $"collision: pos=({position.x.ToString("F3", CultureInfo.InvariantCulture)},{position.y.ToString("F3", CultureInfo.InvariantCulture)}) " +
                               $"wall=({closestWallCenter.x.ToString("F3", CultureInfo.InvariantCulture)},{closestWallCenter.y.ToString("F3", CultureInfo.InvariantCulture)}) " +
                               $"dist={minEdgeDistance.ToString("F3", CultureInfo.InvariantCulture)} " +
                               $"size={closestWallSize.ToString("F3", CultureInfo.InvariantCulture)} " +
                               $"dir=({moveDirection.x.ToString("F1", CultureInfo.InvariantCulture)},{moveDirection.y.ToString("F1", CultureInfo.InvariantCulture)}) " +
                               $"min=({min.x.ToString("F3", CultureInfo.InvariantCulture)},{min.y.ToString("F3", CultureInfo.InvariantCulture)}) " +
                               $"max=({max.x.ToString("F3", CultureInfo.InvariantCulture)},{max.y.ToString("F3", CultureInfo.InvariantCulture)}) " +
                               $"tankSize={halfSize.x.ToString("F3", CultureInfo.InvariantCulture)} " +
                               $"safeDist={safeDistance.ToString("F3", CultureInfo.InvariantCulture)}";
                    AddLog(collisionLogs, log);
                    lastMinEdgeDistance = minEdgeDistance;
                }
                return true;
            }
        }
        else
        {
            if (lastLoggedColliderCount > 0)
            {
                Debug.Log("TANK_COLLISION: No colliders found");
                lastMinEdgeDistance = float.MaxValue;
            }
        }
        
        lastLoggedColliderCount = colliders.Length;
        return false;
    }

    private void SnapToGrid()
    {
        // Выравниваем позицию по сетке в пикселях
        Vector2 currentPos = transform.position;
        currentPos.x = Mathf.Round(currentPos.x * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
        currentPos.y = Mathf.Round(currentPos.y * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
        transform.position = currentPos;
    }

    private void DrawDebugInfo()
    {
        // Визуализация текущей позиции танка
        Vector2 currentCenter = (Vector2)transform.position;
        float halfSizePixels = TANK_COLLIDER_SIZE_PIXELS / 2f;
        Vector2 halfSize = new Vector2(halfSizePixels * PIXEL_SIZE, halfSizePixels * PIXEL_SIZE);
        
        // Рисуем AABB танка
        Vector2 topLeft = currentCenter - halfSize;
        Vector2 topRight = currentCenter + new Vector2(halfSize.x, -halfSize.y);
        Vector2 bottomRight = currentCenter + halfSize;
        Vector2 bottomLeft = currentCenter + new Vector2(-halfSize.x, halfSize.y);
        
        // Функция для рисования пунктирной линии
        void DrawDottedLine(Vector2 start, Vector2 end, Color color)
        {
            float dashLength = 0.1f; // Длина штриха
            float gapLength = 0.1f;  // Длина промежутка
            Vector2 direction = (end - start).normalized;
            float distance = Vector2.Distance(start, end);
            float currentDistance = 0f;
            Vector2 currentPoint = start;

            while (currentDistance < distance)
            {
                Vector2 dashEnd = currentPoint + direction * Mathf.Min(dashLength, distance - currentDistance);
                Debug.DrawLine(currentPoint, dashEnd, color, Time.deltaTime);
                currentPoint = dashEnd + direction * gapLength;
                currentDistance += dashLength + gapLength;
            }
        }
        
        // Желтый квадрат - визуальный коллайдер танка
        DrawDottedLine(topLeft, topRight, Color.yellow);
        DrawDottedLine(topRight, bottomRight, Color.yellow);
        DrawDottedLine(bottomRight, bottomLeft, Color.yellow);
        DrawDottedLine(bottomLeft, topLeft, Color.yellow);

        // Рисуем область проверки коллизий
        if (moveDirection != Vector2.zero)
        {
            Vector2 collisionMin, collisionMax;
            if (moveDirection.x != 0)
            {
                // Движение по горизонтали
                float centerY = currentCenter.y;
                collisionMin = new Vector2(
                    moveDirection.x > 0 ? currentCenter.x + halfSize.x : currentCenter.x - halfSize.x,
                    centerY - halfSize.y
                );
                collisionMax = new Vector2(
                    moveDirection.x > 0 ? currentCenter.x + halfSize.x : currentCenter.x - halfSize.x,
                    centerY + halfSize.y
                );
            }
            else
            {
                // Движение по вертикали
                float centerX = currentCenter.x;
                collisionMin = new Vector2(
                    centerX - halfSize.x,
                    moveDirection.y > 0 ? currentCenter.y + halfSize.y : currentCenter.y - halfSize.y
                );
                collisionMax = new Vector2(
                    centerX + halfSize.x,
                    moveDirection.y > 0 ? currentCenter.y + halfSize.y : currentCenter.y - halfSize.y
                );
            }

            // Красный прямоугольник - область проверки коллизий
            Vector2 cTopLeft = new Vector2(collisionMin.x, collisionMax.y);
            Vector2 cTopRight = collisionMax;
            Vector2 cBottomRight = new Vector2(collisionMax.x, collisionMin.y);
            Vector2 cBottomLeft = collisionMin;

            DrawDottedLine(cTopLeft, cTopRight, Color.red);
            DrawDottedLine(cTopRight, cBottomRight, Color.red);
            DrawDottedLine(cBottomRight, cBottomLeft, Color.red);
            DrawDottedLine(cBottomLeft, cTopLeft, Color.red);
        }

        // Логируем позицию только при значительном изменении
        if (Vector2.Distance(currentCenter, lastLoggedPosition) > 0.1f)
        {
            string log = $"pos=({currentCenter.x.ToString("F3", CultureInfo.InvariantCulture)},{currentCenter.y.ToString("F3", CultureInfo.InvariantCulture)})";
            AddLog(positionLogs, log);
            lastLoggedPosition = currentCenter;
        }
    }

    private bool CheckCollisionAtPoint(Vector2 point)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(point);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }

    private void DrawCollisionPoint(Vector2 point, bool hasCollision)
    {
        float crossSize = 4f/PIXELS_PER_UNIT; // Размер крестика в юнитах
        Color color = hasCollision ? Color.red : Color.green;
        Debug.DrawLine(point + new Vector2(-crossSize, 0), point + new Vector2(crossSize, 0), color, Time.fixedDeltaTime);
        Debug.DrawLine(point + new Vector2(0, -crossSize), point + new Vector2(0, crossSize), color, Time.fixedDeltaTime);
    }

    private bool IsMovingToTarget()
    {
        return Vector2.Distance(transform.position, targetPosition) > 0.01f;
    }

    private void Fire()
    {
        if (Time.time >= nextFireTime)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void UpdateCollisionPoints()
    {
        Vector2 currentPos = (Vector2)transform.position;
        float halfSize = 8f / PIXELS_PER_UNIT; // Конвертируем 8 пикселей в юниты

        // Обновляем позиции точек коллизии в зависимости от направления
        switch (currentDirection)
        {
            case 0: // Вверх
                frontLeft = currentPos + new Vector2(-halfSize, halfSize);
                frontLeftMid = currentPos + new Vector2(-halfSize/2, halfSize);
                frontLeftCenter = currentPos + new Vector2(-halfSize/4, halfSize);
                frontLeftCenterNear = currentPos + new Vector2(-halfSize/8, halfSize);
                frontCenter = currentPos + new Vector2(0, halfSize);
                frontRightCenterNear = currentPos + new Vector2(halfSize/8, halfSize);
                frontRightCenter = currentPos + new Vector2(halfSize/4, halfSize);
                frontRightMid = currentPos + new Vector2(halfSize/2, halfSize);
                frontRight = currentPos + new Vector2(halfSize, halfSize);
                break;
            case 1: // Вправо
                frontLeft = currentPos + new Vector2(halfSize, halfSize);
                frontLeftMid = currentPos + new Vector2(halfSize, halfSize/2);
                frontLeftCenter = currentPos + new Vector2(halfSize, halfSize/4);
                frontLeftCenterNear = currentPos + new Vector2(halfSize, halfSize/8);
                frontCenter = currentPos + new Vector2(halfSize, 0);
                frontRightCenterNear = currentPos + new Vector2(halfSize, -halfSize/8);
                frontRightCenter = currentPos + new Vector2(halfSize, -halfSize/4);
                frontRightMid = currentPos + new Vector2(halfSize, -halfSize/2);
                frontRight = currentPos + new Vector2(halfSize, -halfSize);
                break;
            case 2: // Вниз
                frontLeft = currentPos + new Vector2(halfSize, -halfSize);
                frontLeftMid = currentPos + new Vector2(halfSize/2, -halfSize);
                frontLeftCenter = currentPos + new Vector2(halfSize/4, -halfSize);
                frontLeftCenterNear = currentPos + new Vector2(halfSize/8, -halfSize);
                frontCenter = currentPos + new Vector2(0, -halfSize);
                frontRightCenterNear = currentPos + new Vector2(-halfSize/8, -halfSize);
                frontRightCenter = currentPos + new Vector2(-halfSize/4, -halfSize);
                frontRightMid = currentPos + new Vector2(-halfSize/2, -halfSize);
                frontRight = currentPos + new Vector2(-halfSize, -halfSize);
                break;
            case 3: // Влево
                frontLeft = currentPos + new Vector2(-halfSize, -halfSize);
                frontLeftMid = currentPos + new Vector2(-halfSize, -halfSize/2);
                frontLeftCenter = currentPos + new Vector2(-halfSize, -halfSize/4);
                frontLeftCenterNear = currentPos + new Vector2(-halfSize, -halfSize/8);
                frontCenter = currentPos + new Vector2(-halfSize, 0);
                frontRightCenterNear = currentPos + new Vector2(-halfSize, halfSize/8);
                frontRightCenter = currentPos + new Vector2(-halfSize, halfSize/4);
                frontRightMid = currentPos + new Vector2(-halfSize, halfSize/2);
                frontRight = currentPos + new Vector2(-halfSize, halfSize);
                break;
        }

        // Проверяем коллизии в каждой точке
        collisionPoints[0] = CheckCollisionAtPoint(frontLeft);
        collisionPoints[1] = CheckCollisionAtPoint(frontLeftMid);
        collisionPoints[2] = CheckCollisionAtPoint(frontLeftCenter);
        collisionPoints[3] = CheckCollisionAtPoint(frontLeftCenterNear);
        collisionPoints[4] = CheckCollisionAtPoint(frontCenter);
        collisionPoints[5] = CheckCollisionAtPoint(frontRightCenterNear);
        collisionPoints[6] = CheckCollisionAtPoint(frontRightCenter);
        collisionPoints[7] = CheckCollisionAtPoint(frontRightMid);
        collisionPoints[8] = CheckCollisionAtPoint(frontRight);

        // Определяем, может ли танк двигаться
        canMove = !collisionPoints.Any(x => x);
    }
}