using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // 생성할 적 프리팹 에셋입니다.
    public GameObject enemyPrefab;

    // 적을 몇 초마다 한 번씩 생성할지 정하는 간격(초)입니다.
    public float spawnInterval = 2.0f;

    // 적이 생성될 수 있는 x좌표의 최소값입니다(화면 왼쪽 경계).
    public float minX = -8.0f;

    // 적이 생성될 수 있는 x좌표의 최대값입니다(화면 오른쪽 경계).
    public float maxX = 8.0f;

    // 적이 생성될 고정 y좌표입니다(화면 위쪽).
    public float spawnY = 5.5f;

    // spawnInterval에 도달했는지 확인하기 위해 시간을 누적하는
    // 타이머입니다.
    public float timer = 0f;

    private void Update()
    {
        // 매 프레임 지난 시간(Time.deltaTime)을 타이머에 누적합니다.
        timer += Time.deltaTime;

        // 누적 시간이 생성 간격(spawnInterval)에 도달하면 새로운
        // 적을 하나 생성합니다.
        if (timer >= spawnInterval)
        {
            // null 가드: 인스펙터에서 enemyPrefab을 아직 연결하지
            // 않았다면 생성을 시도하지 않고 넘어갑니다.
            if (enemyPrefab != null)
            {
                // Random.Range(min, max)로 minX~maxX 사이의 임의의
                // x좌표를 뽑아, 매번 다른 위치에서 적이 등장하도록
                // 합니다.
                float randomX = Random.Range(minX, maxX);

                Instantiate(enemyPrefab, new Vector3(randomX, spawnY, 0f), Quaternion.identity);
            }

            // 타이머를 0으로 초기화하여 다음 생성까지 다시 시간을
            // 세기 시작합니다.
            timer = 0f;
        }
    }
}
