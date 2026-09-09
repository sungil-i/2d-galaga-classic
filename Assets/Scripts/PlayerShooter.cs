using UnityEngine;

/// <summary>
/// 갤러그 전투기 좌우 이동 및 충돌 처리를 담당하는 플레이어 컨트롤러
/// </summary>
public class PlayerShooter : ShootingEntity
{
    [Header("이동 제한")]
    [Tooltip("화면 좌우 이동 가능 범위(절반 너비)입니다.")]
    [SerializeField] private float screenHalfWidth = 8.0f;

    protected override void Awake()
    {
        base.Awake();
        maxHp = 3;
        currentHp = maxHp;

        // 부모(ShootingEntity)의 0.2f 기본값은 체감 속도가 너무 느리므로, 
        // 키 입력에 즉각 반응하도록 기본 이동 속도를 안전하게 재조정합니다.
        speed = 6.0f;
    }

    private void Update()
    {
        // 1. 키보드 좌우 입력 수신 (A/D 또는 좌우 방향키)
        float moveX = Input.GetAxisRaw("Horizontal");

        // 2. 등속 프레임 독립 이동 및 화면 경계 차단 (Clamping)
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + moveX * speed * Time.deltaTime, -screenHalfWidth, screenHalfWidth);
        transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 3. 적(Enemy 태그)과의 충돌 판정
        if (collider.CompareTag("Enemy"))
        {
            TakeDamage(1);
            Debug.Log($"[{GetType().Name}] Enemy와 충돌했습니다! 남은 HP: {currentHp}/{maxHp}", this);
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (currentHp <= 0)
        {
            Debug.Log($"[{GetType().Name}] 기체가 파괴되었습니다.", this);
            Destroy(gameObject);
        }
    }
}