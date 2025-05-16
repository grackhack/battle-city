using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int baseDamage = 1; // Базовый урон
    [SerializeField] private GameObject hitEffectPrefab; // Префаб эффекта попадания
    [SerializeField] private float lifetime = 3f; // Время жизни пули
    
    private Rigidbody2D rb;
    private BoxCollider2D bulletCollider;
    private Vector2 direction; // Направление движения пули

    // Публичное свойство для урона
    public int Damage { get; private set; }

    public void Initialize(Vector2 direction)
    {
        this.direction = direction.normalized;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
    }

    private void Awake()
    {
        // Устанавливаем начальный урон равным базовому
        Damage = baseDamage;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Настройка Rigidbody2D
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Настройка коллайдера
        bulletCollider = GetComponent<BoxCollider2D>();
        if (bulletCollider == null)
        {
            bulletCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        bulletCollider.isTrigger = false;
        bulletCollider.size = new Vector2(0.1f, 0.1f);

        // Игнорируем столкновения с танком игрока
        Physics2D.IgnoreCollision(bulletCollider, GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>());
    }

    private void Start()
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            Debug.LogError("Rigidbody2D не найден на пуле!");
        }
        
        // Уничтожаем пулю через заданное время
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Визуализация траектории
        Debug.DrawRay(transform.position, direction * 0.5f, Color.red, 0.1f);
    }

    // Метод для изменения урона
    public void SetDamage(int newDamage)
    {
        if (newDamage > 0)
        {
            Damage = newDamage;
        }
    }

    // Метод для увеличения урона
    public void IncreaseDamage(int amount)
    {
        if (amount > 0)
        {
            Damage += amount;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, может ли объект получать урон
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(Damage); // Используем свойство Damage вместо поля damage
        }

        // Создаем эффект попадания
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
}