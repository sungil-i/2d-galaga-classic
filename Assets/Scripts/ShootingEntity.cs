using UnityEngine;

public class ShootingEntity : MonoBehaviour
{
    // 이동 속도. 자식 클래스(PlayerShooter, EnemyShooter)가 이동
    // 계산에 공통으로 사용합니다.
    public float speed = 3.0f;

    // 최대 체력(Max HP). 자식 클래스가 Awake()에서 자신에게 맞는
    // 값으로 다시 설정(재정의)할 수 있습니다.
    public int maxHp = 1;

    // 현재 체력(Current HP). Awake()에서 maxHp 값으로 채워집니다.
    public int currentHp;

    // virtual(가상) 메서드: 자식 클래스가 override 키워드로 이
    // 메서드의 동작을 자신만의 방식으로 재정의할 수 있습니다.
    // Awake()는 Start()보다 먼저, 오브젝트가 생성된 직후 호출되는
    // Unity 생명주기(Lifecycle) 함수입니다.
    public virtual void Awake()
    {
        // 게임(오브젝트)이 시작될 때 현재 체력을 최대 체력으로
        // 채워 줍니다.
        currentHp = maxHp;
    }

    // 데미지를 입었을 때 호출되는 공통 메서드입니다.
    // damage(피해량)만큼 현재 체력을 감소시키고, 확인용 디버그
    // 로그를 출력합니다. 자식 클래스는 이 메서드를 override하여
    // 자신만의 추가 동작(사망 처리, 전용 로그 등)을 덧붙일 수
    // 있습니다.
    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;

        Debug.Log($"[{name}] 이(가) 피해를 입었습니다. 데미지: {damage}, 남은 HP: {currentHp}/{maxHp}", this);
    }
}
