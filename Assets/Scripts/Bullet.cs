using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    // 총알이 이동하는 속도입니다.
    public float moveSpeed = 10.0f;

    // 명중했을 때 입힐 데미지(피해량)입니다.
    public int damage = 1;

    // 총알이 이동할 방향입니다. 플레이어 총알은 Vector2.up(위쪽),
    // 적 총알은 Vector2.down(아래쪽)으로 설정해서 재사용합니다.
    public Vector2 moveDirection = Vector2.up;

    // 이 총알이 피해를 입힐 대상의 태그입니다. 플레이어 총알은
    // "Enemy", 적 총알은 "Player"로 설정합니다.
    public string targetTag = "Enemy";

    // 원점(0)으로부터 이 값을 벗어나면(위/아래 상관없이) 화면 밖으로
    // 나간 것으로 보고 총알을 자동으로 파괴합니다(메모리 누수 방지).
    public float boundaryY = 6.0f;

    // Awake()는 Start()보다 먼저 호출되는 Unity 생명주기 함수로,
    // 컴포넌트 초기 설정(주로 물리 설정)에 적합합니다.
    private void Awake()
    {
        // TryGetComponent는 GetComponent와 달리 컴포넌트를 찾지
        // 못해도 예외(Exception)를 던지지 않고 false를 반환하므로,
        // null 참조 오류 없이 안전하게 컴포넌트를 가져올 수 있는
        // 방어적 코딩(Defensive Coding) 패턴입니다.
        if (TryGetComponent(out Rigidbody2D rb))
        {
            // 중력의 영향을 받지 않도록 중력 배율을 0으로 만듭니다.
            rb.gravityScale = 0f;

            // Kinematic으로 설정하면 물리 엔진의 힘(중력, 충돌 반발력
            // 등)에 영향을 받지 않고, 스크립트로만 위치를 제어할 수
            // 있습니다. 그래도 Rigidbody2D가 있으면 트리거 이벤트는
            // 정상적으로 감지됩니다.
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    // Update()는 매 프레임마다 호출되는 함수입니다.
    private void Update()
    {
        // Time.deltaTime(이전 프레임과의 시간 간격)을 곱해서 이동
        // 거리를 계산하면, 프레임 속도(FPS)에 상관없이 항상 일정한
        // 속도로 이동하는 "프레임 독립적(Frame-rate Independent)"
        // 이동을 구현할 수 있습니다.
        transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);

        // Mathf.Abs로 절대값을 구해, 위쪽(양수)이든 아래쪽(음수)이든
        // boundaryY를 벗어나면 화면 밖으로 나간 것으로 판단하고
        // 총알을 파괴합니다.
        if (Mathf.Abs(transform.position.y) > boundaryY)
        {
            Destroy(gameObject);
        }
    }

    // OnTriggerEnter2D는 이 오브젝트의 Collider2D가 isTrigger = true로
    // 설정된 상태에서, 다른 Collider2D와 겹쳤을 때 자동으로 호출되는
    // Unity 이벤트 함수입니다.
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // targetTag와 일치하고, 상대방이 ShootingEntity를 상속받은
        // 스크립트를 가지고 있는지 함께 확인하는 안전한(defensive)
        // 충돌 판정입니다. TryGetComponent는 컴포넌트가 없어도
        // 예외를 던지지 않고 false를 반환합니다.
        if (collider.CompareTag(targetTag) && collider.TryGetComponent<ShootingEntity>(out var entity))
        {
            entity.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
