using UnityEngine;

// [교육용 스크립트] EnemyShooter
// ------------------------------------------------------------
// 화면 위에서 아래로 천천히 내려오면서 일정 시간마다 총알을 자동
// 발사하는 적 스크립트입니다. ShootingEntity를 상속받아 speed,
// maxHp, currentHp, TakeDamage 같은 공통 기능을 그대로 사용합니다.
public class EnemyShooter : ShootingEntity
{
    // 이 y좌표보다 아래로 내려가면 화면 밖으로 나간 것으로 보고
    // 자기 자신을 파괴합니다(메모리 누수 방지).
    public float destroyBelowY = -6.0f;

    // 적이 발사할 총알 프리팹 에셋입니다.
    public GameObject bulletPrefab;

    // 총알이 발사될 위치입니다. 비워두면(null) 적 자신의 위치에서
    // 발사합니다.
    public Transform firePoint;

    // 총알을 몇 초마다 한 번씩 발사할지 정하는 간격(초)입니다.
    public float fireInterval = 2.0f;

    // fireInterval에 도달했는지 확인하기 위해 시간을 누적하는
    // 타이머입니다.
    public float fireTimer = 0f;

    // override로 부모의 Awake()를 재정의하고, 적의 체력을 1로
    // 명시적으로 설정합니다.
    public override void Awake()
    {
        base.Awake();

        maxHp = 1;
        currentHp = maxHp;
    }

    private void Update()
    {
        // Vector2.down 방향으로 speed * Time.deltaTime 만큼 이동시켜,
        // 프레임 속도(FPS)에 상관없이 항상 일정한 속도로 아래로
        // 내려오게 만듭니다(프레임 독립적 이동).
        transform.Translate(Vector2.down * (speed * Time.deltaTime), Space.World);

        // 화면 아래 경계를 벗어나면 더 이상 필요 없는 적이므로
        // 파괴하여 메모리 누수(계속 쌓이는 오브젝트)를 방지합니다.
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }

        // 매 프레임 지난 시간(Time.deltaTime)을 타이머에 누적합니다.
        fireTimer += Time.deltaTime;

        // 누적 시간이 발사 간격(fireInterval)에 도달하면 총알을
        // 발사하고 타이머를 0으로 초기화하여 다시 카운트를
        // 시작합니다.
        if (fireTimer >= fireInterval)
        {
            FireBullet();
            fireTimer = 0f;
        }
    }

    private void FireBullet()
    {
        // null 가드: 인스펙터에서 bulletPrefab을 연결하지 않았다면
        // 아무 동작도 하지 않고 조용히 종료합니다.
        if (bulletPrefab == null)
        {
            return;
        }

        // firePoint가 지정되어 있으면 그 위치에서, 없으면 적 자신의
        // 위치에서 총알을 생성합니다.
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }

    // 부모(ShootingEntity)의 TakeDamage(int)를 override하여, 체력이
    // 0 이하가 되면 파괴 로그를 남기고 오브젝트를 파괴합니다.
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (currentHp <= 0)
        {
            Debug.Log("[Enemy 파괴] 적이 총알에 맞아 격추되었습니다!");
            Destroy(gameObject);
        }
    }
}
