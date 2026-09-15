using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 이 클래스는 UnityEditor 네임스페이스를 사용하므로 반드시 "Assets/Scripts/Editor" 폴더
// 아래에 있어야 합니다. Editor 폴더 안의 스크립트는 최종 빌드(게임 실행 파일)에는 포함되지
// 않고, 에디터에서만(그리고 -batchmode 헤드리스 실행에서도) 동작하는 "제작 도구용" 코드입니다.
// -executeMethod SceneSetup.BuildScene 처럼 Unity를 커맨드라인에서 배치 모드로 실행하면
// 에디터 창을 띄우지 않고도 이 정적(static) 메서드를 그대로 호출할 수 있습니다.
//
// 이 프로젝트(2d-galaga-classic)는 Player와 PlayerShooter의 인스펙터 값(screenHalfWidth,
// speed)을 사용자가 이미 수동으로 조정해 두었기 때문에, 다른 프로젝트의 SceneSetup처럼
// 씬을 통째로 새로 만들지 않습니다. 대신 "기존 씬(Assets/Scenes/Main.unity)을 그대로 열어서"
// Prefab만 새로 만들고, 이미 있는 Player는 bulletPrefab 필드만 조심스럽게 연결합니다.
/// <summary>
/// 2d-galaga-classic 씬을 헤드리스로 구성하는 에디터 유틸리티: Bullet/Enemy Prefab을
/// 생성하고, 기존 씬의 Player와 새로 배치한 EnemySpawner에 Prefab 참조를 와이어링합니다.
/// </summary>
public static class SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string ScenesFolder = "Assets/Scenes";
    private const string BulletPrefabPath = PrefabFolder + "/Bullet.prefab";
    private const string EnemyPrefabPath = PrefabFolder + "/Enemy.prefab";
    private const string EnemyBulletPrefabPath = PrefabFolder + "/EnemyBullet.prefab";
    private const string EnemyTag = "Enemy";
    private const string PlayerTag = "Player";

    private const string CapsuleSpritePath =
        "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Capsule.png";
    private const string FlightsSpriteSheetPath = "Assets/Sprites/Flights.png";
    private const string EnemySpriteName = "Flights_17";

    public static void BuildScene()
    {
        // 1. 폴더 준비: Assets/Prefabs, Assets/Scenes가 없으면 새로 만듭니다.
        EnsureFolderExists(PrefabFolder);
        EnsureFolderExists(ScenesFolder);

        // 2. "Enemy"와 "Player" 태그가 ProjectSettings/TagManager.asset에 등록되어
        //    있는지 확인하고, 없을 때만 새로 추가합니다(이미 있으면 아무 것도 하지
        //    않는 안전한 방어 코드). EnemyBullet이 Player를 명중시키려면
        //    Bullet.OnTriggerEnter2D의 targetTag("Player")와 실제 Player
        //    GameObject의 태그가 일치해야 합니다.
        EnsureTagExists(EnemyTag);
        EnsureTagExists(PlayerTag);

        // 3. 기존 씬을 그대로 엽니다. 씬 파일이 아직 없다면(초기 세팅이 안 된 경우에 한해서만)
        //    빈 새 씬을 만듭니다. NewScene으로 "덮어쓰기"하지 않는 이유는, 이미 씬 안에 있는
        //    Player(사용자가 인스펙터 값을 손으로 맞춰 둔 상태)를 그대로 보존하기 위해서입니다.
        Scene scene = OpenOrCreateScene(ScenePath);

        SetupCamera();

        // 4. Prefab 3종(Bullet, EnemyBullet, Enemy)을 코드로 생성/저장합니다. SceneSetup을
        //    다시 실행해도 항상 같은 결과가 나오도록(멱등성, idempotent), 매번 새로 만들어
        //    같은 경로에 덮어쓰기 저장합니다. EnemyBullet은 Player 전용(아래로 이동,
        //    "Player" 태그 타격) 총알로, 기존 Player용 Bullet.prefab과는 별도의 에셋입니다
        //    — 같은 Bullet.prefab을 공유하면 적이 자기 자신의 "Enemy" 태그를 맞고 즉시
        //    파괴되는 자기 자신 격추 버그가 발생하기 때문입니다.
        GameObject bulletPrefab = CreateBulletPrefab();
        GameObject enemyBulletPrefab = CreateEnemyBulletPrefab();
        GameObject enemyPrefab = CreateEnemyPrefab(enemyBulletPrefab);

        // 5. 씬에 이미 있는 Player는 건드리지 않고, bulletPrefab 필드와 "Player" 태그만
        //    안전하게 연결합니다.
        WireExistingPlayer(bulletPrefab);

        // 6. EnemySpawner는 씬에 없으면 새로 만들고, 있으면 그대로 재사용하면서 Prefab
        //    참조와 스폰 범위만 다시 채워 넣습니다.
        SetupEnemySpawner(enemyPrefab);

        // 7. 씬 파일(.unity)을 디스크에 저장하고, 빌드 설정(Build Settings > Scenes In
        //    Build)에 등록해야 실제 빌드에도 포함됩니다.
        EditorSceneManager.SaveScene(scene, ScenePath);
        RegisterSceneInBuildSettings(ScenePath);

        Debug.Log($"[SceneSetup] '{ScenePath}' 씬 구성이 완료되었습니다. Prefab: {BulletPrefabPath}, {EnemyBulletPrefabPath}, {EnemyPrefabPath}");
    }

    // 씬 파일이 디스크에 이미 있으면 그 내용을 그대로 열고(OpenScene), 없을 때만 완전히
    // 빈 새 씬을 만듭니다. 이렇게 하면 사용자가 손으로 배치/수정해 둔 Player 등의 내용을
    // 실수로 지우지 않습니다.
    private static Scene OpenOrCreateScene(string path)
    {
        if (File.Exists(path))
        {
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    // "MainCamera" 태그를 가진 카메라를 찾아서(없으면 새로 만들어서) 2D 슈팅 게임에 맞는
    // 직교(Orthographic) 카메라로 설정합니다.
    private static void SetupCamera()
    {
        GameObject cameraObj = GameObject.FindWithTag("MainCamera");
        if (cameraObj == null)
        {
            cameraObj = new GameObject("Main Camera", typeof(Camera));
            cameraObj.tag = "MainCamera";
        }

        if (cameraObj.TryGetComponent(out Camera cam))
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        cameraObj.transform.position = new Vector3(0f, 0f, -10f);
    }

    // 총알 Prefab: 얇고 긴 미사일 모양을 표현하기 위해 Scale을 (0.15, 0.45, 1.0)으로 눌러
    // Capsule 스프라이트를 슬림하게 만듭니다. BoxCollider2D는 isTrigger = true로 설정해
    // 물리적으로 밀어내지 않고 "닿았는지 여부"만 감지하도록 하고, Rigidbody2D는 Kinematic +
    // 중력 0으로 설정해 트리거 이벤트가 안정적으로 발생하도록 합니다.
    private static GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("Bullet");
        bullet.transform.localScale = new Vector3(0.15f, 0.45f, 1.0f);

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CapsuleSpritePath);
        sr.color = Color.white;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        bullet.AddComponent<Bullet>();

        return SaveAndCleanupPrefab(bullet, BulletPrefabPath);
    }

    // 적 전용 총알 Prefab: 모양은 Player용 Bullet.prefab과 비슷하지만(얇은 미사일 형태의
    // Capsule 스프라이트), 색은 연한 빨강으로 구분하고 아래쪽으로 이동하며 "Player" 태그를
    // 타격하도록 Bullet 컴포넌트 값을 다르게 설정합니다. 같은 Bullet 스크립트를 재사용하되
    // moveDirection/targetTag 값만 반대로 두어, 하나의 스크립트로 양방향 총알을 표현하는
    // 구조를 그대로 활용합니다.
    private static GameObject CreateEnemyBulletPrefab()
    {
        GameObject enemyBullet = new GameObject("EnemyBullet");
        enemyBullet.transform.localScale = new Vector3(0.15f, 0.45f, 1.0f);

        SpriteRenderer sr = enemyBullet.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CapsuleSpritePath);
        sr.color = new Color(1f, 0.4f, 0.4f, 1f);

        BoxCollider2D col = enemyBullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = enemyBullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        Bullet bulletComponent = enemyBullet.AddComponent<Bullet>();
        bulletComponent.moveDirection = Vector2.down;
        bulletComponent.targetTag = PlayerTag;
        bulletComponent.moveSpeed = 8.0f;
        bulletComponent.boundaryY = 6.0f;
        bulletComponent.damage = 1;

        return SaveAndCleanupPrefab(enemyBullet, EnemyBulletPrefabPath);
    }

    // 적 Prefab: 기존에는 내장 삼각형 스프라이트를 Scale Y = -1로 뒤집어 사용했지만,
    // 이제는 'Assets/Sprites/Flights.png' 스프라이트 시트 안에서 이름이 "Flights_17"인
    // 조각(sub-asset)을 직접 찾아 사용합니다. 텍스처의 원래 색이 그대로 보이도록
    // SpriteRenderer.color는 Color.white로 두고(이전의 회색 틴트 제거), 스프라이트
    // 자체의 방향이 맞으므로 Scale도 Vector3.one으로 되돌립니다.
    private static GameObject CreateEnemyPrefab(GameObject enemyBulletPrefab)
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.tag = EnemyTag;
        enemy.transform.localScale = Vector3.one;

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSpriteByName(FlightsSpriteSheetPath, EnemySpriteName);
        sr.color = Color.white;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        EnemyShooter shooter = enemy.AddComponent<EnemyShooter>();
        shooter.descentSpeed = 0.3f;
        shooter.horizontalSpeed = 2.0f;
        shooter.screenHalfWidth = 8.0f;
        shooter.destroyBelowY = -6.0f;
        shooter.fireInterval = 2.0f;
        shooter.bulletPrefab = enemyBulletPrefab;

        return SaveAndCleanupPrefab(enemy, EnemyPrefabPath);
    }

    // 스프라이트 시트(예: Flights.png)는 하나의 텍스처 파일 안에 여러 장의 스프라이트가
    // "Sliced" 상태로 잘려 들어있습니다. AssetDatabase.LoadAllAssetsAtPath는 그 파일 안의
    // 모든 서브 에셋(Texture2D 본체 + 잘려진 Sprite 조각들)을 배열로 돌려주므로, 그중
    // 이름이 정확히 일치하는 Sprite 하나를 찾아서 반환합니다. 이름을 못 찾으면(슬라이스
    // 이름이 바뀌었거나 텍스처가 아직 Sprite로 잘리지 않은 경우) null을 그대로 대입하지
    // 않고 에러 로그를 남겨 원인을 바로 알 수 있게 합니다.
    private static Sprite LoadSpriteByName(string sheetPath, string spriteName)
    {
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
        foreach (Object asset in subAssets)
        {
            if (asset is Sprite sprite && sprite.name == spriteName)
            {
                return sprite;
            }
        }

        Debug.LogError(
            $"[SceneSetup] Failed to find sprite '{spriteName}' in {sheetPath}. Check sprite slice name.",
            null);
        return null;
    }

    // PrefabUtility.SaveAsPrefabAsset은 씬에 있는 GameObject의 현재 상태를 그대로 복사해
    // 지정한 경로에 .prefab 에셋 파일로 저장합니다(이미 같은 경로에 파일이 있으면 덮어씁니다).
    // 저장이 끝나면 씬에 남아있는 임시 원본 GameObject는 더 이상 필요 없으므로
    // DestroyImmediate로 즉시 제거합니다 — Prefab은 "에셋(디스크의 설계도)"이지, 씬에
    // 계속 남아있어야 하는 인스턴스가 아닙니다.
    private static GameObject SaveAndCleanupPrefab(GameObject source, string path)
    {
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(source, path, out bool success);
        if (!success)
        {
            Debug.LogError($"[SceneSetup] Prefab 저장 실패: {path}");
        }

        Object.DestroyImmediate(source);
        return savedPrefab;
    }

    // 씬에서 이름이 "Player"인 GameObject를 찾아 "Player" 태그를 보장하고,
    // PlayerShooter의 bulletPrefab 필드만 연결합니다. TryGetComponent를 사용해 Player가
    // 없거나 PlayerShooter가 없어도 예외 없이 안전하게 넘어갑니다. screenHalfWidth,
    // speed 등 사용자가 인스펙터에서 이미 손으로 맞춰 둔 값은 절대 건드리지 않습니다.
    private static void WireExistingPlayer(GameObject bulletPrefab)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("[SceneSetup] 씬에서 'Player' 오브젝트를 찾지 못했습니다. bulletPrefab 연결을 건너뜁니다.");
            return;
        }

        // EnemyBullet의 targetTag("Player")와 실제로 맞물리려면 Player GameObject의
        // 태그가 "Player"로 지정되어 있어야 합니다.
        if (player.tag != PlayerTag)
        {
            player.tag = PlayerTag;
        }

        if (!player.TryGetComponent(out PlayerShooter shooter))
        {
            Debug.LogWarning("[SceneSetup] 'Player'에 PlayerShooter 컴포넌트가 없습니다. bulletPrefab 연결을 건너뜁니다.");
            return;
        }

        shooter.bulletPrefab = bulletPrefab;
    }

    // 씬에 이름이 "EnemySpawner"인 GameObject가 있으면 그대로 재사용하고, 없으면 새로
    // 만듭니다. enemyPrefab 참조와 스폰 범위(minX/maxX/spawnY)를 매번 다시 채워 넣어,
    // 이 메서드를 여러 번 실행해도 항상 같은 설정으로 맞춰집니다.
    private static void SetupEnemySpawner(GameObject enemyPrefab)
    {
        GameObject spawner = GameObject.Find("EnemySpawner");
        if (spawner == null)
        {
            spawner = new GameObject("EnemySpawner");
        }

        if (!spawner.TryGetComponent(out EnemySpawner spawnerComponent))
        {
            spawnerComponent = spawner.AddComponent<EnemySpawner>();
        }

        spawnerComponent.enemyPrefab = enemyPrefab;
        spawnerComponent.minX = -8.0f;
        spawnerComponent.maxX = 8.0f;
        spawnerComponent.spawnY = 5.5f;
        spawnerComponent.spawnInterval = 2.0f;
    }

    private static void RegisterSceneInBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path))
        {
            return;
        }

        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parentFolder = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string newFolderName = Path.GetFileName(folderPath);
        AssetDatabase.CreateFolder(parentFolder, newFolderName);
    }

    // ProjectSettings/TagManager.asset을 SerializedObject로 직접 열어 "Enemy" 태그가
    // 이미 등록되어 있는지 확인합니다. 이미 있으면 그대로 반환하고, 없을 때만 배열 끝에
    // 새 항목을 추가합니다(중복 등록 방지).
    private static void EnsureTagExists(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
            {
                return;
            }
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }
}
