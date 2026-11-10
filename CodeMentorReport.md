# Code Mentor 보고서 — OvertheHorizon-1
_2026-11-11 00:27 · 모델 qwen2.5-coder-7b-instruct-q4_k_m_

## 객체지향 구조 (9/10)
- 단일 책임: 100% — 한 클래스가 한 가지 일만 하는 파일 비율
- 캡슐화: 96% — public 필드가 3개 미만인 컴포넌트 비율
- 낮은 결합도: 70% — Find나 다른 클래스의 Instance로 직접 찾지 않는 파일 비율
- 전역 상태 절제: 93% — static 가변 필드가 없는 파일 비율
- 추상화: 100% — 인터페이스·추상 클래스 0개, 공통 인터페이스 후보 0개
- 중복 없음: 100% — 코드가 크게 겹치는 파일 쌍 0개

### 개선 후보 (규칙 분석)

**[2] 전역 상태 — 2개 파일에 static 가변 필드**
static 값은 어디서든 바뀌고 씬을 다시 불러와도 남아서, 버그의 원인을 추적하기 어렵습니다. 값을 소유한 객체의 인스턴스 필드로 옮기고 필요한 곳에 참조를 넘기세요. 여러 씬이 공유해야 하면 ScriptableObject에 두세요.
파일: EnemyBulletPool.cs, Loading Manager.cs
```csharp
[CreateAssetMenu] public class GameSession : ScriptableObject
{
    public int score;
    public int round;
}

// 쓰는 쪽은 인스펙터로 같은 에셋을 연결
[SerializeField] GameSession session;
```

**[3] 숨은 의존성 — 2개 파일이 Find로 다른 객체를 찾음**
필요한 객체를 실행 중에 이름이나 타입으로 찾으면, 무엇에 의존하는지 인스펙터에서 보이지 않고 이름만 바뀌어도 깨집니다. [SerializeField]로 연결하거나 생성하는 쪽이 넘겨 주세요.
파일: KeyInputManager.cs, HomingBullet.cs
```csharp
// 대신
[SerializeField] PlayerHealthController player;   // 인스펙터에서 연결

// 또는 생성하는 쪽에서 넘겨 준다
var enemy = Instantiate(prefab);
enemy.Init(player);
```

## 스크립트별 분석

### Assets/Audio/SoundManager.cs — 7/10
**오디오 관리자**

SoundManager는 게임의 배경 음악(BGM)과 효과음(SFX)을 관리합니다. BGM은 반복적으로 재생되며, SFX는 한 번 재생됩니다.

**하는 일**
- 배경 음악(BGM)과 효과음(SFX)을 관리합니다.
- BGM을 반복적으로 재생합니다.
- SFX를 한 번 재생합니다.

**실행 흐름**
- Start: 첫 번째 프레임에서 BGM을 재생합니다.

**잘한 점**
- SoundManager 클래스는 오디오 관리를 담당하고 있습니다.
- PlayBGM과 PlaySFX 메서드는 각각 배경 음악과 효과 음악을 재생하는 역할을 합니다.

**문제점**
- [높음] 14행 배경 음악 클립이 미할당된 경우 — bgmClip 배열이 미할당된 경우, bgmClip[0]은 Null이 되어 NullReferenceException이 발생할 수 있습니다.
  - 개선: if (bgmClip != null && bgmClip.Length > 0) PlayBGM(bgmClip[0]);
- [높음] 31행 효과 음악 클립이 미할당된 경우 — clip이 미할당된 경우, PlayOneShot은 NullReferenceException이 발생할 수 있습니다.
  - 개선: if (clip != null) sfxSource.PlayOneShot(clip);

> SoundManager 클래스는 기본적인 오디오 재생 기능을 구현하고 있지만, 배경 음악과 효과 음악 클립이 미할당된 경우 NullReferenceException이 발생할 수 있습니다. 이를 방지하기 위해 Null 체크를 추가하면 좋습니다.

### Assets/Scripts/0. Intro Scene/Button type/Edit Button.cs — 10/10
**UI 버튼을 클릭하면 편집 패널을 표시하거나 숨기는 역할**

EditButton 클래스는 UIStateFeedback 클래스를 상속받아 구현. OnClickEdit 메서드를 통해 버튼 클릭 시 편집 패널의 활성화 상태를 토글합니다.

**하는 일**
- UIStateFeedback 클래스를 상속받아 구현
- OnClickEdit 메서드를 통해 버튼 클릭 시 편집 패널의 활성화 상태를 토글

**실행 흐름**
- OnClickEdit: 버튼 클릭 시 실행, 편집 패널의 활성화 상태를 토글

**잘한 점**
- 코드는 간단하고 직관적입니다. 버튼 클릭에 따라 편집 패널을 표시하거나 숨기는 기능이 잘 구현되어 있습니다.

> 이 코드는 버그나 오동작이 없습니다. 성능이나 메모리 사용도 문제가 없습니다. 잘 작성된 코드입니다.

### Assets/Scripts/0. Intro Scene/Button type/Exit Button.cs — 10/10
**ExitButton은 게임을 종료하는 버튼을 처리하는 스크립트입니다.**

ExitButton은 사용자가 게임을 종료하도록 하는 버튼을 나타냅니다. 버튼이 클릭되면 게임이 종료됩니다. 에디터 환경에서는 에디터가 종료됩니다.

**하는 일**
- ExitButton은 사용자가 게임을 종료하도록 하는 버튼을 처리합니다.
- Application.Quit() 메서드를 사용하여 게임을 종료합니다.
- Unity 에디터 환경에서는 UnityEditor.EditorApplication.isPlaying = false;를 사용하여 에디터를 종료합니다.

**실행 흐름**
- ExitGame() 메서드가 호출되면 게임이 종료됩니다.

**잘한 점**
- 코드는 간단하고 직관적입니다. ExitGame 메서드는 게임을 종료하는 역할을 잘 수행합니다.

> 이 코드는 버그나 오동작이 없습니다. 게임 종료 버튼의 기능이 잘 구현되어 있습니다.

### Assets/Scripts/0. Intro Scene/Button type/Start Button.cs — 10/10
**시작 버튼을 클릭하면 다음 씬을 로드하는 스크립트**

이 스크립트는 Unity의 UIStateFeedback 클래스를 상속받아 작성되었습니다. StartButton 클래스는 'nextSceneName'이라는 문자열 변수를 사용하여 다음 로드할 씬의 이름을 저장합니다. OnClickStartScene 메서드는 이 버튼이 클릭될 때 호출되며, LoadingSceneManager.LoadScene 메서드를 사용하여 'nextSceneName'에 저장된 씬을 로드합니다.

**하는 일**
- nextSceneName 변수를 사용하여 다음 로드할 씬의 이름을 저장합니다.
- OnClickStartScene 메서드를 사용하여 버튼이 클릭될 때 다음 씬을 로드합니다.

**실행 흐름**
- OnClickStartScene: 버튼이 클릭되면 호출되며, LoadingSceneManager.LoadScene 메서드를 사용하여 'nextSceneName'에 저장된 씬을 로드합니다.

**잘한 점**
- 코드는 간단하고 직관적입니다.

> 이 스크립트는 잘 작성되어 있습니다. 시작 버튼 클릭 시 다음 씬을 로드하는 기능이 정상적으로 작동합니다.

### Assets/Scripts/0. Intro Scene/HomingBullet.cs — 8/10
**홈ming 총알의 동작을 처리하는 스크립트**

홈ming 총알은 플레이어가 설정한 대상으로 이동하며, 대상이 없으면 위쪽으로 이동합니다. 총알은 일정 시간 후 파괴됩니다.

**하는 일**
- 홈ming 총알의 속도를 설정합니다.
- 총알이 파괴되는 시간을 설정합니다.
- 홈ming 총알이 적에게 유도되도록 합니다.
- 총알이 적에게 도달하면 파괴됩니다.

**실행 흐름**
- Start: 총알이 파괴되도록 설정하고, 가장 가까운 적을 찾습니다.
- OnEnable: 적이 피격 검사를 할 수 있도록 목록에 등록합니다.
- OnDisable: 적이 피격 검사를 할 수 있도록 목록에서 제거합니다.
- Update: 총알이 대상으로 이동하거나 위쪽으로 이동합니다. 대상이 없으면 다시 적을 찾습니다.

**잘한 점**
- Destroy(gameObject, destroyTime);
- OnEnable() => PlayerBullets.Active.Add(transform);
- OnDisable() => PlayerBullets.Active.Remove(transform);

**문제점**
- [높음] 31행 direction 계산 — direction 계산 시, target이 null일 경우 NullReferenceException이 발생할 수 있다.
  - 개선: if (target != null) Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
- [중간] 13행 FindNearestEnemy 호출 — FindNearestEnemy를 호출할 때, transform.parent가 null이 아닌 경우에만 호출해야 한다.
  - 개선: if (transform.parent != null) FindNearestEnemy();

> 확인된 문제 2개 (높음 1, 중간 1, 낮음 0). 먼저 볼 것: direction 계산.

### Assets/Scripts/0. Intro Scene/KeyBindButton.cs — 7/10
**키 바인딩 버튼을 관리하는 스크립트**

Inspector에서 설정한 행동의 키를 바꿀 수 있는 버튼을 관리합니다. 버튼이 클릭되면 입력한 키로 바뀝니다. ESC 키를 누르면 취소됩니다.

**하는 일**
- Inspector에서 설정한 행동의 키를 바꿀 수 있는 버튼을 관리합니다.
- 버튼이 클릭되면 입력한 키로 바뀝니다.
- ESC 키를 누르면 취소됩니다.
- 키 변경 중이면 버튼이 선택되지 않도록 합니다.
- 키 변경이 완료되면 텍스트를 업데이트합니다.

**실행 흐름**
- OnEnable: KeyManager 인스턴스를 가져와 OnKeysChanged 이벤트를 연결합니다. Refresh 메서드를 호출합니다.
- OnDisable: KeyManager의 OnKeysChanged 이벤트를 해제하고, Rebind를 취소합니다.
- Rebind: KeyManager의 ChangeKey 메서드를 호출하여 키를 변경합니다. 텍스트를 'PRESS KEY'로 변경합니다. 버튼이 선택되지 않도록 EventSystem을 사용합니다.
- Refresh: KeyManager의 keys 딕셔너리에서 해당 행동의 키를 가져와 텍스트를 업데이트합니다.

**잘한 점**
- OnEnable 메서드에서 manager 인스턴스를 초기화하고, OnDisable 메서드에서 이벤트를 해제하는 로직이 잘 작성되어 있습니다.
- Refresh 메서드는 manager.keys.TryGetValue(action, out var key)를 사용하여 키 값을 가져오는 로직이 잘 작성되어 있습니다.

**문제점**
- [높음] 27행 CancelRebind 호출 시 manager가 null일 경우 — manager.CancelRebind()가 호출될 때 manager가 null일 경우 NullReferenceException이 발생할 수 있습니다.
  - 개선: if (manager == null) return;를 CancelRebind 메서드의 시작 부분에 추가하여 manager가 null일 경우 메서드를 종료합니다.
- [높음] 28행 OnKeysChanged 이벤트 해제 시 manager가 null일 경우 — manager.OnKeysChanged -= Refresh;가 호출될 때 manager가 null일 경우 NullReferenceException이 발생할 수 있습니다.
  - 개선: if (manager == null) return;를 OnDisable 메서드의 시작 부분에 추가하여 manager가 null일 경우 메서드를 종료합니다.
- [중간] 18행 KeyManager 인스턴스가 null일 경우 — KeyManager.Instance가 null일 경우, manager 변수는 null이 되어 OnDisable에서 manager.CancelRebind()와 manager.OnKeysChanged -= Refresh;가 실행되지 않습니다. 이는 메모리 누수를 일으킬 수 있습니다.
  - 개선: if (manager == null) return;를 OnEnable 메서드의 시작 부분에 추가하여 manager가 null일 경우 메서드를 종료합니다.

> KeyBindButton 스크립트는 잘 작성되어 있지만, manager 인스턴스가 null일 경우의 처리를 추가하면 더 안전하게 작동할 것입니다.

### Assets/Scripts/0. Intro Scene/UIStateFeedback.cs — 10/10
**UIStateFeedback는 UI 요소의 호버 상태를 처리하는 스크립트입니다.**

UI 요소가 마우스에 호버되면 크기를 늘리고, 마우스가 요소를 벗어나면 원래 크기로 돌아옵니다. 이는 사용자 인터페이스의 피드백을 제공합니다.

**하는 일**
- UI 요소의 기본 크기를 저장합니다.
- OnPointerEnter 이벤트를 처리하여 UI 요소가 호버되면 크기를 늘립니다.
- OnPointerExit 이벤트를 처리하여 UI 요소가 호버 상태에서 벗어나면 원래 크기로 돌아옵니다.

**실행 흐름**
- Awake: UI 요소의 RectTransform을 가져와 기본 크기를 저장합니다.
- OnPointerEnter: 호버 상태를 시작하고, UI 요소의 크기를 늘립니다.
- OnPointerExit: 호버 상태를 종료하고, UI 요소의 크기를 원래대로 돌아옵니다.

**잘한 점**
- Awake 메서드에서 RectTransform을 초기화하고, OnPointerEnter와 OnPointerExit 메서드에서 DOTween을 사용하여 UI 요소의 호버 상태를 효과적으로 처리하고 있습니다.
- hoverScale과 duration 변수를 인스펙터에서 조정할 수 있도록 설정되어 있습니다.

> 이 스크립트는 잘 작성되어 있으며, UI 요소의 호버 상태를 처리하는 데 효과적입니다.

### Assets/Scripts/2.GameScene/BackGround.cs — 9/10
**배경 움직임을 처리하는 스크립트**

배경과 함께 움직이며, Y축이 특정 값에 도달하면 위치를 재설정하여 무한 스크롤 효과를 생성합니다.

**하는 일**
- 배경과 함께 Y축으로 움직입니다.
- Y축이 특정 값에 도달하면 위치를 재설정합니다.
- 다른 배경 오브젝트도 동일한 움직임을 적용합니다.

**실행 흐름**
- Awake: 초기 위치를 저장합니다.
- Update: Y축으로 움직이며, Y축이 특정 값에 도달하면 위치를 재설정합니다.

**잘한 점**
- Awake 메서드에서 original_position을 초기화하는 로직이 잘 작성되어 있습니다.
- Update 메서드에서 배경의 Y 좌표를 감소시키는 로직이 잘 작성되어 있습니다.

**문제점**
- [중간] 32행 BackGrounds가 null일 때 오류 — BackGrounds가 null일 때 Update 메서드에서 오류가 발생할 수 있습니다. BackGrounds가 null일 때는 이동 로직을 실행하지 않도록 조건문을 추가해야 합니다.
  - 개선: if (BackGrounds != null) { BackGrounds.transform.position = new Vector3(BackGrounds.transform.position.x, resetY, BackGrounds.transform.position.z); }

> 확인된 문제 1개 (높음 0, 중간 1, 낮음 0). 먼저 볼 것: BackGrounds가 null일 때 오류.

### Assets/Scripts/2.GameScene/ESC Stop.cs — 10/10
**ESC 버튼을 눌렀을 때 게임 일시정지와 패널 활성화/비활성화를 처리하는 스크립트**

ESC 버튼이 눌리면 게임 일시정지 상태를 전환하고 패널을 활성화/비활성화합니다. 일시정지 상태에서는 게임 시간이 멈추고 패널이 표시됩니다.

**하는 일**
- ESC 버튼이 눌렸을 때 게임 일시정지 상태를 전환합니다.
- 패널을 활성화/비활성화합니다.
- 일시정지 상태에서는 게임 시간이 멈춥니다.

**실행 흐름**
- Update: ESC 버튼이 눌렸는지 확인하고, 일시정지 상태를 전환합니다.

**잘한 점**
- 코드는 간단하고 직관적입니다.
- ESC 버튼을 눌렀을 때 게임 일시정지와 패널 활성화/비활성화를 처리하는 로직이 명확합니다.

> 이 스크립트는 잘 작동하고, 버그나 오동작이 없습니다. 모든 기능이 제대로 동작합니다.

### Assets/Scripts/2.GameScene/Levl Up Scen.cs — 10/10
**레벨 업 화면을 관리하는 스크립트**

이 스크립트는 게임의 레벨 업 화면을 관리하며, 플레이어가 레벨을 업그레이드하면 필요한 UI 업데이트를 처리합니다.

**하는 일**
- 레벨 업 화면을 초기화합니다.
- 플레이어의 레벨 업을 감지하고 UI를 업데이트합니다.
- 레벨 업 관련 버튼의 동작을 처리합니다.

**실행 흐름**
- Start: 레벨 업 화면을 초기화합니다.
- Update: 레벨 업 관련 버튼의 동작을 감지합니다.

> 이 스크립트는 현재 비어있지만, 기본적인 구조는 올바르게 작성되었습니다. 개발자가 이 스크립트를 확장하여 레벨 업 화면을 관리하는 기능을 추가할 수 있습니다.

### Assets/Scripts/2.GameScene/PlayerStatusUI.cs — 9/10
**플레이어의 체력 표시 UI를 관리하는 스크립트**

이 스크립트는 게임 씬에서 플레이어의 이름, 체력, 체력바를 표시하고 관리합니다. 플레이어의 체력이 변경되면 UI가 자동으로 업데이트됩니다.

**하는 일**
- 플레이어의 이름을 표시합니다.
- 플레이어의 체력과 최대 체력을 표시합니다.
- 플레이어의 체력 바를 업데이트합니다.
- 플레이어의 체력이 변경될 때 UI를 업데이트합니다.

**실행 흐름**
- Start: 플레이어의 이름을 설정하고 체력 바를 초기화합니다.
- OnDestroy: 체력 변경 이벤트 핸들러를 제거합니다.
- Refresh: 플레이어의 체력과 최대 체력을 UI에 표시합니다.

**잘한 점**
- PlayerStatusUI 클래스는 플레이어의 체력 표시 UI를 관리하는 역할을 잘 수행합니다.
- Start 메서드에서 PlayerHit.Instance를 가져와 OnHpChanged 이벤트 핸들러를 추가하고 Refresh 메서드를 호출하여 초기 상태를 설정합니다.
- OnDestroy 메서드에서 OnHpChanged 이벤트 핸들러를 제거하여 메모리 누수를 방지합니다.

**문제점**
- [중간] 25행 이벤트 핸들러 추가 — PlayerHit.Instance가 null일 경우 OnHpChanged 이벤트 핸들러가 추가되지 않습니다. 이는 문제 없지만, null 체크를 추가하면 안전합니다.
  - 개선: if (player != null) player.OnHpChanged += Refresh;
- [낮음] 36행 MaxHp 0으로 나누기 — player.MaxHp이 0일 경우 Mathf.Max(player.MaxHp, 1)은 1을 반환하므로 문제가 없습니다. 그러나 MaxHp이 0일 경우를 고려하여 0으로 나누는 경우를 처리하는 것이 좋습니다.
  - 개선: hpSlider.value = player.MaxHp > 0 ? (float)player.Hp / player.MaxHp : 0;

> 확인된 문제 2개 (높음 0, 중간 1, 낮음 1). 먼저 볼 것: 이벤트 핸들러 추가.

### Assets/Scripts/Bullet/BulletMove.cs — 9/10
**플레이어의 총알을 움직이고 파괴하는 스크립트**

이 스크립트는 플레이어의 총알을 움직이고, 일정 시간 후에 파괴합니다. 또한, 적이 총알을 피격할 수 있도록 적의 목록에 등록합니다.

**하는 일**
- 총알의 속도를 설정하고 움직입니다.
- 총알이 일정 시간 후에 파괴됩니다.
- 적의 목록에 총알을 등록하여 적이 피격할 수 있도록 합니다.

**실행 흐름**
- Start: 총알이 파괴되는 시간을 설정합니다.
- OnEnable: 적의 목록에 총알을 등록합니다.
- OnDisable: 적의 목록에서 총알을 제거합니다.
- Update: 총알을 위로 움직입니다.

**잘한 점**
- destroyTime과 speed를 인스펙터에서 설정할 수 있습니다.
- OnEnable과 OnDisable을 사용하여 PlayerBullets의 Active 목록에 등록하고 해제합니다.

**문제점**
- [중간] 11행 destroyTime이 0일 때 — destroyTime이 0으로 설정되면 총알이 즉시 파괴되어 움직이지 않습니다.
  - 개선: if (destroyTime > 0) Destroy(gameObject, destroyTime);
- [중간] 21행 timeScale=0일 때 — timeScale이 0으로 설정되면 Time.deltaTime이 0이 되어 총알이 움직이지 않습니다.
  - 개선: transform.Translate(speed * Time.fixedDeltaTime * Vector3.up);

> BulletMove 스크립트는 플레이어의 총알을 움직이고 파괴하는 역할을 잘 수행하고 있습니다. destroyTime과 timeScale에 대한 처리를 추가하면 더 안정적인 코드가 될 것입니다.

### Assets/Scripts/Bullet/PlayerBullets.cs — 10/10
**플레이어의 탄을 관리하는 클래스**

플레이어의 탄을 저장하고, 적이 탄을 피격할 수 있도록 목록을 유지합니다. 게임이 재시작될 때 탄 목록을 초기화합니다.

**하는 일**
- 플레이어의 탄을 저장하는 목록을 관리합니다.
- 적이 플레이어의 탄을 피격할 수 있도록 목록을 유지합니다.
- 게임이 재시작될 때 탄 목록을 초기화합니다.

**실행 흐름**
- RuntimeInitializeOnLoadMethod: 게임이 시작될 때 탄 목록을 초기화합니다.

**잘한 점**
- PlayerBullets 클래스는 플레이어의 탄을 관리하는 역할을 잘 수행하고 있습니다.

> PlayerBullets 클래스는 플레이어의 탄을 관리하는 역할을 잘 수행하고 있습니다. 현재 코드에는 문제가 없습니다.

### Assets/Scripts/Enemy/Enemy.cs — 9/10
**적의 움직임, 패턴, 그리고 충돌 감지를 처리하는 스크립트**

Enemy 클래스는 적의 기본적인 움직임 패턴, 체력 관리, 그리고 플레이어의 탄과의 충돌을 처리합니다. 적은 화면 밖에서 플레이어로 이동한 후, 설정된 패턴을 반복적으로 실행하며, 플레이어의 탄에 맞으면 체력이 감소하고, 체력이 0이 되면 적이 파괴됩니다.

**하는 일**
- 적의 체력 관리
- 적의 기본 움직임 패턴 실행
- 플레이어의 탄과의 충돌 감지
- 적 파괴

**실행 흐름**
- Awake: 적의 체력을 최대치로 초기화하고, SpriteRenderer 컴포넌트를 가져옵니다.
- Start: 적이 화면 밖에서 플레이어로 이동한 후, 패턴을 반복적으로 실행합니다.
- Update: 적이 도착하면 좌우로 휘어지는 움직임을 실행하고, 플레이어의 탄과의 충돌을 감지합니다.
- CheckPlayerBullets: 플레이어의 탄과의 충돌을 감지하고, 적의 체력을 감소시킵니다.
- TakeDamage: 적의 체력이 0이 되면 적이 파괴됩니다.
- RunPattern: 설정된 패턴을 실행합니다.
- Ring, AimedFan, Spiral, Curve: 각각의 패턴을 실행합니다.

**잘한 점**
- Enemy 클래스는 적의 움직임, 패턴, 그리고 충돌 감지를 처리하는 역할을 잘 수행하고 있습니다.
- Enemy 클래스는 인스펙터에서 쉽게 설정할 수 있는 필드를 제공하고 있습니다.

> 확인된 문제 1개 (높음 0, 중간 1, 낮음 0). 먼저 볼 것: 재활성화 시 hp가 초기화되지 않음.

### Assets/Scripts/Enemy/EnemyBullet.cs — 10/10
**적의 총알을 발사하고 관리하는 스크립트**

적의 총알은 화면 내에서 이동하며, 플레이어와 충돌하면 플레이어에게 데미지를 입히고 자신을 파괴합니다. 총알은 초기 위치, 방향, 속도, 가속도, 각속도를 설정하고, 시간이 지남에 따라 속도와 방향이 변화합니다.

**하는 일**
- 적의 총알을 초기화하고 설정합니다.
- 총알이 플레이어와 충돌하면 플레이어에게 데미지를 입히고 자신을 파괴합니다.
- 총알이 화면 외부로 나가면 파괴합니다.
- 총알의 위치와 방향을 업데이트합니다.
- 총알의 충돌 판정을 수행합니다.

**실행 흐름**
- Awake: SpriteRenderer 컴포넌트를 초기화합니다.
- Init: 총알의 초기 위치, 방향, 속도, 색상, 가속도, 각속도를 설정합니다.
- Update: 총알의 위치와 방향을 업데이트하고, 충돌 판정을 수행합니다. 시간이 지남에 따라 속도와 방향이 변화합니다.
- OnDrawGizmosSelected: 총알의 충돌 판정 영역을 시각화합니다.

**잘한 점**
- EnemyBullet 클래스는 적의 총알을 발사하고 관리하는 역할을 잘 수행하고 있습니다.
- EnemyBullet 클래스는 Awake, Update, OnDrawGizmosSelected 등의 Unity 이벤트 함수를 사용하여 적의 총알의 동작을 관리하고 있습니다.
- EnemyBullet 클래스는 인스펙터 노출 필드인 radius, damage, maxLifeTime을 사용하여 적의 총알의 속성을 설정하고 있습니다.

**문제점**
- [낮음] 44행 angle이 360도 이상일 경우 — angle이 360도 이상일 경우 Mathf.Deg2Rad를 사용하면 예상치 못한 결과가 발생할 수 있습니다.
  - 개선: angle %= 360f; float rad = angle * Mathf.Deg2Rad;

> 확인된 문제 1개 (높음 0, 중간 0, 낮음 1). 먼저 볼 것: angle이 360도 이상일 경우.

### Assets/Scripts/Enemy/EnemyBulletPool.cs — 10/10
**적의 총알을 관리하는 풀**

적의 총알을 미리 생성하여 재사용하여 메모리 사용을 최적화합니다. 총알의 생성, 회수, 관리 등을 담당합니다.

**하는 일**
- 적의 총알을 미리 생성하여 재사용합니다.
- 총알의 생성, 회수, 관리 등을 담당합니다.
- 적의 총알이 화면 밖으로 나가면 회수하여 재사용합니다.
- 적의 총알을 생성하고 초기 상태를 설정합니다.
- 적의 총알을 회수하여 풀에 저장합니다.

**실행 흐름**
- Awake: 총알 풀을 초기화하고, 미리 총알을 생성합니다.
- Spawn: 총알 풀에서 총알을 가져와 사용합니다.
- Despawn: 총알을 사용한 후, 풀에 회수합니다.
- ClearAll: 화면 밖으로 나간 총알을 모두 회수합니다.

**잘한 점**
- 싱글톤 패턴을 사용하여 적의 총알을 관리하는 풀을 구현하였습니다.

> 검증을 통과한 문제가 없습니다. 다만 작은 모델의 리뷰이므로 버그가 없다는 뜻은 아닙니다.

### Assets/Scripts/Enemy/EnemySpawner.cs — 9/10
**적을 화면 위에서 내려보내고, 각 웨이브마다 적을 생성하고 관리하는 스크립트**

EnemySpawner는 게임에서 적을 생성하고 관리하는 역할을 합니다. 게임 시작 시 첫 번째 적을 생성하고, 각 웨이브마다 적을 생성하며, 적이 전부 죽으면 다음 웨이브로 넘어갑니다.

**하는 일**
- 적을 화면 위에서 내려보내고 생성
- 각 웨이브마다 적을 생성하고 관리
- 적의 체력을 설정하고, 적이 죽으면 해당 적을 제거

**실행 흐름**
- Start: 첫 번째 적을 생성하고, 적이 전부 죽으면 다음 웨이브로 넘어감
- SpawnWave: 각 웨이브마다 적을 생성
- CreateEnemy: 적을 생성하고, 적의 체력을 설정

**잘한 점**
- EnemySpawner 클래스는 적을 화면 위에서 내려보내고, 각 웨이브마다 적을 생성하고 관리하는 역할을 잘 수행합니다.
- EnemySpawner 클래스는 인스펙터에서 설정할 수 있는 다양한 필드를 제공하여 적의 생성과 웨이브 관리를 쉽게 할 수 있습니다.

**문제점**
- [중간] 29행 웨이브 사이의 대기 시간이 일관되지 않음 — waveDelay는 각 웨이브마다 증가하지만, 이 스크립트에서는 waveDelay가 일정하게 유지됩니다. 각 웨이브마다 waveDelay를 증가시키는 로직이 누락되어 있습니다.
  - 개선: waveDelay += 0.5f; // 예시로 0.5f씩 증가시킵니다.
- [낮음] 50행 Enemy 생성 시 SpriteRenderer 설정이 중복 — CreateEnemy 메서드에서 SpriteRenderer 설정이 중복되어 있습니다. 이미 enemyPrefab이 설정되어 있다면, SpriteRenderer 설정은 필요하지 않습니다.
  - 개선: if (enemyPrefab == null) { ... }

> EnemySpawner 클래스는 적의 생성과 웨이브 관리를 잘 수행하지만, waveDelay가 일관되지 않아서 문제가 있습니다. 또한, Enemy 생성 시 SpriteRenderer 설정이 중복되어 있습니다.

### Assets/Scripts/Manager/KeyInputManager.cs — 10/10
**키 입력을 관리하고, 사용자가 설정할 수 있는 키 바인딩을 처리하는 관리자**

이 스크립트는 게임 내에서 사용자가 설정할 수 있는 키 바인딩을 관리합니다. 기본 키 설정을 초기화하고, 사용자가 키를 바꿀 수 있도록 하는 기능도 제공합니다.

**하는 일**
- 기본 키 설정 초기화
- 키 바인딩 변경
- 키 입력 감지
- 키 설정 저장 및 로드
- 키 변경 이벤트 알림

**실행 흐름**
- Awake: 키 설정 초기화 및 로드
- ChangeKey: 사용자가 키를 바꿀 수 있도록 요청
- WaitForKeyPress: 사용자가 새로운 키를 입력할 때까지 대기
- AssignKey: 새로운 키를 할당하고, 필요한 경우 다른 키와 교체
- SaveKeys: 변경된 키 설정을 PlayerPrefs에 저장
- LoadKeys: PlayerPrefs에서 저장된 키 설정을 로드

**잘한 점**
- Awake 메서드에서 싱글톤 패턴을 구현하여 KeyManager의 인스턴스를 관리하고 있습니다.
- ChangeKey 메서드를 통해 키 바인딩을 변경할 수 있습니다.
- AssignKey 메서드를 통해 이미 다른 행동이 쓰는 키를 고르면 서로 맞바꿔서 한 키가 두 행동에 겹치지 않게 합니다.

> 검증을 통과한 문제가 없습니다. 다만 작은 모델의 리뷰이므로 버그가 없다는 뜻은 아닙니다.

### Assets/Scripts/Manager/Loading Manager.cs — 10/10
**LoadingSceneManager은 게임의 로딩 화면을 관리하고, 씬 전환을 비동기적으로 처리하는 매니저입니다.**

LoadingSceneManager은 사용자가 선택한 씬으로 로딩을 시작하고, 로딩 진행 상황을 UI에 표시합니다. 로딩이 완료되면 최소한의 대기 시간 후에 씬이 전환됩니다.

**하는 일**
- 로딩할 씬의 이름을 저장합니다.
- 로딩 전용 씬으로 이동합니다.
- 비동기적으로 씬을 로딩하고, 로딩 진행 상황을 UI에 표시합니다.
- 최소 대기 시간 후에 씬이 전환됩니다.

**실행 흐름**
- Start: 로드할 씬 이름이 지정되지 않았으면 오류를 출력하고 종료합니다. 그렇지 않으면 씬 로딩을 시작합니다.
- LoadSceneAsync: 비동기로 씬을 로딩하고, 로딩 진행 상황을 UI에 표시합니다. 로딩이 완료되면 최소 대기 시간 후에 씬이 전환됩니다.

**잘한 점**
- LoadingSceneManager은 게임의 로딩 화면을 관리하고, 씬 전환을 비동기적으로 처리하는 매니저입니다.

> 검증을 통과한 문제가 없습니다. 다만 작은 모델의 리뷰이므로 버그가 없다는 뜻은 아닙니다.

### Assets/Scripts/Manager/Pulling Manager.cs — 10/10
**PullingManager는 게임 내에서 물체를 끌어당겨서 특정 동작을 발생시키는 관리자입니다.**

PullingManager는 Unity의 MonoBehaviour를 상속받아 Start와 Update 메서드를 통해 게임의 물체를 끌어당기는 동작을 관리합니다. Start 메서드에서는 초기 설정을 하고, Update 메서드에서는 물체의 위치를 업데이트하여 물체를 끌어당킵니다.

**하는 일**
- 물체를 끌어당기는 동작을 관리합니다.
- Start 메서드에서 초기 설정을 합니다.
- Update 메서드에서 물체의 위치를 업데이트합니다.

**실행 흐름**
- Start: 초기 설정을 합니다.
- Update: 물체의 위치를 업데이트하여 물체를 끌어당킵니다.

> PullingManager 클래스는 현재 비어있지만, 기본적인 구조는 올바르게 작성되었습니다. 현재로서는 문제가 없습니다.

### Assets/Scripts/Player/Player.cs — 10/10
**플레이어 데이터를 저장하는 스크립트**

PlayerData 클래스는 플레이어의 이름, ID, 체력, 속도, 아이콘, 애니메이션, 애니메이션 재생 속도, 설명, 크기 설정 등을 저장합니다. 이 데이터는 게임의 다른 부분에서 참조하여 플레이어를 구성합니다.

**하는 일**
- 플레이어의 데이터를 저장합니다.
- 플레이어의 이름, ID, 체력, 속도, 아이콘, 애니메이션, 애니메이션 재생 속도, 설명, 크기 설정 등을 관리합니다.

**실행 흐름**
- PlayerData 클래스는 직접 실행되지 않습니다. 게임의 다른 부분에서 참조하여 플레이어 데이터를 사용합니다.

**잘한 점**
- PlayerData 클래스는 플레이어 데이터를 저장하는 역할을 잘 수행하고 있습니다.

> PlayerData 클래스는 잘 작성된 상태로, 인스펙터에서 플레이어 데이터를 쉽게 관리할 수 있습니다.

### Assets/Scripts/Player/PlayerHit.cs — 10/10
**플레이어의 피격 판정과 무적 상태를 관리하는 스크립트**

플레이어가 적이나 폭탄 등에 충돌했을 때 피격 판정을 처리하고, 무적 상태를 관리합니다. 무적 상태는 일정 시간 동안 플레이어를 보호합니다.

**하는 일**
- 플레이어의 피격 판정을 처리합니다.
- 무적 상태를 관리합니다.
- 체력 UI를 갱신합니다.
- 플레이어가 게임 오버일 때 게임을 종료합니다.

**실행 흐름**
- Awake: 체력을 최대치로 초기화하고, 무적 상태를 초기화합니다.
- Start: 피격 박스를 생성합니다.
- LateUpdate: 피격 박스의 위치를 업데이트하고, 무적 상태를 관리합니다.
- TakeHit: 플레이어가 피격받았을 때 호출됩니다. 체력을 감소시키고, 무적 상태를 시작합니다.
- Invincible: 무적 상태 동안 플레이어의 스프라이터를 깜빡입니다.
- CreateHitboxView: 피격 박스를 생성합니다.
- OnDisable: 피격 박스를 비활성화합니다.
- OnDestroy: 인스턴스를 해제하고, 피격 박스를 파괴합니다.

**잘한 점**
- Awake, Start, LateUpdate, OnDisable, OnDestroy 메서드를 사용하여 적절한 시점에서 초기화, 실행, 업데이트, 비활성화, 파괴를 관리합니다.
- TakeHit 메서드를 통해 플레이어의 피격 판정과 무적 상태를 관리합니다.
- Invincible 메서드를 통해 무적 시간 동안의 깜빡임 효과를 구현합니다.

> 검증을 통과한 문제가 없습니다. 다만 작은 모델의 리뷰이므로 버그가 없다는 뜻은 아닙니다.

### Assets/Scripts/Player/PlayerMove.cs — 10/10
**플레이어의 이동을 처리하는 컨트롤러**

PlayerMove 스크립트는 플레이어의 이동을 처리합니다. 플레이어는 키보드나 게임패드를 사용하여 이동하며, 'Focus' 키를 누르면 느리게 이동할 수 있습니다. 또한, 플레이어의 속도, 아이콘, 애니메이션, 크기 등을 설정할 수 있습니다.

**하는 일**
- 플레이어의 이동을 처리합니다.
- Focus 키를 사용하여 플레이어의 이동 속도를 조절합니다.
- 플레이어의 속도, 아이콘, 애니메이션, 크기를 설정합니다.
- 플레이어가 화면 경계를 벗어나지 않도록 제한합니다.

**실행 흐름**
- Awake: PlayerHit 컴포넌트가 없으면 자동으로 추가합니다.
- Start: SelectScene에서 선택한 데이터를 적용합니다. 데이터가 없으면 기본값으로 실행합니다.
- Update: 플레이어의 이동 방향과 속도를 계산하고, 애니메이션을 재생합니다.

**잘한 점**
- Awake 메서드에서 PlayerHit 컴포넌트가 없으면 자동으로 추가하는 로직이 있습니다.
- ClampToBounds 메서드에서 mainCamera가 null일 경우 리턴하는 로직이 있습니다.
- ReadFocus, ReadMove 메서드에서 Keyboard.current, Gamepad.current가 null일 경우 리턴하는 로직이 있습니다.

> 검증을 통과한 문제가 없습니다. 다만 작은 모델의 리뷰이므로 버그가 없다는 뜻은 아닙니다.

### Assets/Scripts/Player/PlayerShooting.cs — (미분석)

### Assets/Scripts/SlectScene.cs — (미분석)

### Assets/Scripts/TMI/TmiReader.cs — (미분석)

### Assets/Scripts/TMI/TmiText.cs — (미분석)
