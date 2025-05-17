using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 16f; // Скорость в пикселях в секунду
    [SerializeField] private int baseDamage = 1; // Базовый урон
    [SerializeField] private GameObject hitEffectPrefab; // Префаб эффекта попадания
    [SerializeField] private float lifetime = 3f; // Время жизни пули
    [SerializeField] private AudioClip hitSound; // Звук попадания
    [SerializeField] private float hitVolume = 0.3f; // Громкость звука попадания
    
    private Rigidbody2D rb;
    private BoxCollider2D bulletCollider;
    private Vector2 direction; // Направление движения пули
    private const float PIXELS_PER_UNIT = 16f; // 16 пикселей = 1 юнит Unity
    private const float BULLET_SIZE_PIXELS = 4f; // Размер пули в пикселях
    private const float PIXEL_SIZE = 1f / PIXELS_PER_UNIT; // Размер одного пикселя в юнитах

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
        bulletCollider.size = new Vector2(BULLET_SIZE_PIXELS/PIXELS_PER_UNIT, BULLET_SIZE_PIXELS/PIXELS_PER_UNIT);

        // Игнорируем столкновения с танком игрока
        Physics2D.IgnoreCollision(bulletCollider, GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>());
    }

    private void Start()
    {
        if (rb != null)
        {
            // Устанавливаем скорость в пикселях в секунду
            rb.velocity = direction * speed * PIXEL_SIZE;
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
        // Выравниваем позицию по пиксельной сетке
        Vector2 pos = transform.position;
        pos.x = Mathf.Round(pos.x * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
        pos.y = Mathf.Round(pos.y * PIXELS_PER_UNIT) / PIXELS_PER_UNIT;
        transform.position = pos;

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
            damageable.TakeDamage(Damage);
        }

        // Создаем эффект попадания
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Проигрываем звук попадания
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
        }
        
        Debug.Log($"Bullet collision with: {collision.gameObject.name} on layer {collision.gameObject.layer}");
        
        Destroy(gameObject);
    }
}