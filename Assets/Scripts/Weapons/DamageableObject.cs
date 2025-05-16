using UnityEngine;

public class DamageableObject : MonoBehaviour, IDamageable
{
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected GameObject destroyEffectPrefab;
    
    protected int currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} получил урон {damage}. Осталось здоровья: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Создаем эффект уничтожения
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"{gameObject.name} уничтожен!");
        Destroy(gameObject);
    }
} 