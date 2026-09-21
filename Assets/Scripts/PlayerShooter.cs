using UnityEngine;

public class PlayerShooter : ShootingEntity
{
    // 화면 좌우 이동 가능 범위(절반 너비)입니다. Orthographic 카메라
    // Size 5, 16:9 화면 비율을 기준으로 계산된 값(8.5f)입니다.
    // public으로 선언해 인스펙터에서 바로 값을 확인/수정할 수
    // 있습니다.
    public float screenHalfWidth = 8.5f;

    // 발사할 총알 프리팹 에셋입니다.
    public GameObject bulletPrefab;

    // 총알이 발사될 위치입니다. 비워두면(null) 플레이어 자신의
    // 위치에서 발사합니다.
    public Transform firePoint;

    // override 키워드로 부모(ShootingEntity)의 virtual Awake()를
    // 재정의합니다. base.Awake()를 호출해 부모의 초기화 로직
    // (currentHp = maxHp)도 함께 실행되도록 합니다.
    public override void Awake()
    {
        base.Awake();

        // 플레이어의 최대 체력을 5로 명시적으로 설정합니다.
        maxHp = 5;
        currentHp = maxHp;
    }

    private void Update()
    {
        // GetAxisRaw는 GetAxis와 달리 부드러운 보간(smoothing) 없이
        // -1, 0, 1 값을 즉시 반환하므로, 슈팅 게임처럼 즉각적인
        // 반응이 필요한 조작에 적합합니다.
        float moveX = Input.GetAxisRaw("Horizontal");

        Vector3 pos = transform.position;

        // Mathf.Clamp로 이동 후 위치를 화면 좌우 범위
        // (-screenHalfWidth ~ screenHalfWidth) 안으로 강제 제한하여
        // 플레이어가 화면 밖으로 나가지 못하도록 방어합니다.
        pos.x = Mathf.Clamp(pos.x + moveX * speed * Time.deltaTime, -screenHalfWidth, screenHalfWidth);

        transform.position = pos;

        // GetKeyDown은 키를 누르는 "그 순간" 한 프레임만 true를
        // 반환하므로, 스페이스바를 누를 때마다 총알이 한 발씩만
        // 발사되도록 해 줍니다(누르고 있어도 연사되지 않음).
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireBullet();
        }
    }

    private void FireBullet()
    {
        // null 가드(Null Guard): 인스펙터에서 bulletPrefab을 아직
        // 연결하지 않았다면, 오류를 내지 않고 조용히 함수를
        // 종료하여 예외(Exception) 발생을 방지합니다.
        if (bulletPrefab == null)
        {
            return;
        }

        // firePoint가 지정되어 있으면 그 위치에서, 없으면 플레이어
        // 자신의 위치에서 총알을 생성합니다.
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }

    // 플레이어 본체가 적과 직접 충돌(트리거)했을 때 호출됩니다.
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.CompareTag("Enemy"))
        {
            return;
        }

        // 적과 부딪히면 1의 피해를 입습니다.
        TakeDamage(1);
    }

    // 부모(ShootingEntity)의 TakeDamage(int)를 override하여,
    // 몇 번 맞았는지("1/5" ~ "5/5")를 알려주는 전용 로그와 사망
    // 처리(오브젝트 파괴)를 추가로 수행합니다.
    public override void TakeDamage(int damage)
    {
        // base.TakeDamage(damage)를 호출해 부모 클래스의 공통
        // 체력 감소 로직을 재사용합니다.
        base.TakeDamage(damage);

        // 지금까지 누적으로 몇 대 맞았는지 계산합니다.
        // 예) maxHp=5, currentHp=3 이면 hitsTaken = 2 (두 번 맞음).
        int hitsTaken = maxHp - currentHp;

        Debug.Log($"[Player 피격] HP: {hitsTaken}/{maxHp} (남은 HP: {currentHp})");

        if (currentHp <= 0)
        {
            Debug.Log("[Player 사망] 플레이어가 파괴되었습니다!");
            Destroy(gameObject);
        }
    }
}
