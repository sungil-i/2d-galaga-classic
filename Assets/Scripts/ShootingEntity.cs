using UnityEngine;

public abstract class ShootingEntity : MonoBehaviour
{
    [Header("전투 스탯")]
    [Tooltip("이동 속도입니다.")]
    [SerializeField] protected float speed = 0.2f;

    [Tooltip("최대 체력입니다.")]
    [SerializeField] protected int maxHp = 1;

    protected int currentHp;

    protected virtual void Awake()
    {
        currentHp = maxHp;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"[{GetType().Name}] 공격 {damage} 받음. HP: {currentHp}/{maxHp}");
    }

    public virtual void TakeDamage(int damage, string attackerName)
    {
        TakeDamage(damage);
        Debug.Log($"[{GetType().Name}] {attackerName}로부터 공격 {damage} 받음. HP: {currentHp}/{maxHp}");
    }
}