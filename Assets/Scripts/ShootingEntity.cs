using UnityEngine;

// [교육용 스크립트] ShootingEntity
// ------------------------------------------------------------
// 플레이어(PlayerShooter), 적(EnemyShooter) 등 "총알을 쏘고 맞는
// 개체"들이 공통으로 사용할 기능을 모아 둔 부모(기반) 클래스입니다.
//
// 이 프로젝트는 초보자 학습용이므로 abstract(추상) 키워드를 사용하지
// 않는 일반 class로 선언했습니다. abstract 클래스는 직접 게임
// 오브젝트에 붙일 수 없어 개념이 다소 어렵게 느껴질 수 있기 때문에,
// 여기서는 이해하기 쉬운 "공통 부모 클래스(공용 설계도)"로만
// 사용합니다.
//
// 모든 필드를 public으로 선언한 이유도 같습니다. private나
// [SerializeField] 같은 접근 제한자는 캡슐화(정보 은닉) 개념이
// 필요한데, 처음 배우는 단계에서는 인스펙터(Inspector) 창에서 값을
// 바로 보고 수정할 수 있도록 public으로 단순화했습니다.
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
