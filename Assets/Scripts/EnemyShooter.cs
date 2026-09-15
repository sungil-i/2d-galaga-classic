using UnityEngine;

// [교육용 스크립트] EnemyShooter
// ------------------------------------------------------------
// 화면 위쪽에서 아주 천천히 아래로 내려오면서(마치 공중에 떠 있는
// 느낌) 동시에 좌우로 불규칙하게 흔들리며 이동하고, 일정 시간마다
// 자신만의(Player를 겨냥하는) 총알을 자동 발사하는 적 스크립트입니다.
// ShootingEntity를 상속받아 speed, maxHp, currentHp, TakeDamage 같은
// 공통 기능을 그대로 사용합니다.
public class EnemyShooter : ShootingEntity
{
    // 아래로 내려가는 속도입니다. 값이 매우 작아 "거의 떠 있는 듯한"
    // 느낌의 완만한 하강을 표현합니다.
    public float descentSpeed = 0.3f;

    // 좌우로 흔들리며(랜덤 워크) 이동할 때 사용하는 속도입니다.
    public float horizontalSpeed = 2.0f;

    // 화면 좌우 경계(16:9, Orthographic Size 5 기준)입니다. 이 값을
    // 벗어나려고 하면 방향을 반대로 튕겨내(bounce) 화면 밖으로 나가지
    // 않도록 방어합니다.
    public float screenHalfWidth = 8.0f;

    // 새로운 좌우 이동 방향을 얼마나 자주 다시 뽑을지 정하는 시간
    // 간격(초)의 기준값입니다.
    public float changeDirInterval = 1.2f;

    // changeDirInterval에 도달했는지 확인하기 위해 시간을 누적하는
    // 타이머입니다.
    public float changeDirTimer = 0f;

    // 현재 좌우 이동 방향(-1.0 ~ 1.0)입니다. 이 값이 랜덤하게 계속
    // 바뀌면서 "불규칙하게 흔들리는" 움직임을 만들어 냅니다.
    public float moveDirectionX = 0f;

    // 이 y좌표보다 아래로 내려가면 화면 밖으로 나간 것으로 보고
    // 자기 자신을 파괴합니다(메모리 누수 방지).
    public float destroyBelowY = -6.0f;

    // 적이 발사할 총알 프리팹 에셋입니다(Player를 겨냥하는 전용
    // EnemyBullet 프리팹을 연결합니다).
    public GameObject bulletPrefab;

    // 총알이 발사될 위치입니다. 비워두면(null) 적 자신의 위치에서
    // 발사합니다.
    public Transform firePoint;

    // 총알을 몇 초마다 한 번씩 발사할지 정하는 간격(초)입니다.
    public float fireInterval = 2.0f;

    // fireInterval에 도달했는지 확인하기 위해 시간을 누적하는
    // 타이머입니다.
    public float fireTimer = 0f;

    // override로 부모의 Awake()를 재정의하고, 적의 체력과 초기
    // 이동 방향/타이머 값을 무작위로 설정합니다.
    public override void Awake()
    {
        base.Awake();

        maxHp = 1;
        currentHp = maxHp;

        // Random.Range(-1.0f, 1.0f)는 -1.0과 1.0 사이의 무작위
        // 실수를 반환합니다. 적마다 처음부터 서로 다른 방향으로
        // 움직이기 시작하도록 만들어 줍니다.
        moveDirectionX = Random.Range(-1.0f, 1.0f);

        // 방향 전환 간격도 무작위로 뽑아, 모든 적이 똑같은 타이밍에
        // 방향을 바꾸지 않고 제각각 자연스럽게 움직이도록 합니다.
        changeDirInterval = Random.Range(0.8f, 1.6f);
    }

    private void Update()
    {
        // --- 1. 무작위 방향 전환 루틴 ---
        // 매 프레임 지난 시간을 누적하다가, 정해진 간격에 도달하면
        // 새로운 좌우 이동 방향을 다시 뽑고 타이머와 다음 간격을
        // 초기화합니다. 이렇게 하면 "랜덤 워크(random walk)"처럼
        // 예측하기 어려운 흔들림이 만들어집니다.
        changeDirTimer += Time.deltaTime;
        if (changeDirTimer >= changeDirInterval)
        {
            moveDirectionX = Random.Range(-1.0f, 1.0f);
            changeDirTimer = 0f;
            changeDirInterval = Random.Range(0.8f, 1.6f);
        }

        // --- 2. 화면 좌우 경계 튕김(bounce) 처리 ---
        // 왼쪽 경계를 넘어가려는데 방향도 왼쪽이면 오른쪽으로,
        // 오른쪽 경계를 넘어가려는데 방향도 오른쪽이면 왼쪽으로
        // 방향을 강제로 뒤집어, 화면 밖으로 나가지 않도록 합니다.
        if (transform.position.x <= -screenHalfWidth && moveDirectionX < 0f)
        {
            moveDirectionX = Mathf.Abs(moveDirectionX);
        }
        if (transform.position.x >= screenHalfWidth && moveDirectionX > 0f)
        {
            moveDirectionX = -Mathf.Abs(moveDirectionX);
        }

        // --- 3. 좌우 흔들림 + 완만한 하강을 하나의 이동 벡터로 결합 ---
        // x축은 moveDirectionX * horizontalSpeed로 좌우 흔들림을,
        // y축은 -descentSpeed로 아주 느린 하강을 표현합니다.
        // Time.deltaTime을 곱해 프레임 속도(FPS)에 상관없이 항상
        // 일정한 속도로 움직이게 만듭니다(프레임 독립적 이동).
        Vector3 movement = new Vector3(moveDirectionX * horizontalSpeed, -descentSpeed, 0f) * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // --- 4. 화면 아래 경계 이탈 시 파괴 ---
        if (transform.position.y < destroyBelowY)
        {
            Destroy(gameObject);
        }

        // --- 5. 자동 발사 루틴 ---
        fireTimer += Time.deltaTime;
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
            Debug.Log("[Enemy 파괴] 적이 격추되었습니다!", this);
            Destroy(gameObject);
        }
    }
}
