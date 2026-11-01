using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Slider progressBar;       // 로딩바 Slider
    [SerializeField] private TextMeshProUGUI progressText; // 퍼센트 표시 텍스트 (TMP)

    [Header("설정")]
    [SerializeField] private float fadeOrMinDelay = 0.5f; // 너무 빨리 넘어가서 화면이 튀는 것을 방지하기 위한 최소 대기 시간

    // 이동할 다음 씬의 이름을 저장하는 정적 변수
    public static string nextSceneName;

    /// <summary>
    /// 다른 매니저에서 씬 전환을 호출할 때 사용하는 정적 메서드
    /// </summary>
    /// <param name="sceneName">이동하고자 하는 씬 이름</param>
    public static void LoadScene(string sceneName)
    {
        nextSceneName = sceneName;
        SceneManager.LoadScene("0.5_Loading"); // 로딩 전용 씬으로 우선 이동
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("로드할 씬 이름이 지정되지 않았습니다!");
            return;
        }

        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // 1. 비동기로 씬 로딩 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);

        // 씬 로딩이 완료되어도 즉시 전환되지 않도록 설정 (연출 및 부드러운 전환용)
        op.allowSceneActivation = false;

        float timer = 0f;
        float shown = 0f; // 화면에 표시되는 진행률

        while (!op.isDone)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            // op.progress는 0.0 ~ 0.9 사이의 값을 반환합니다 (0.9에서 로딩 완료)
            float target = Mathf.Clamp01(op.progress / 0.9f);

            // 게이지가 튀지 않도록 목표값을 향해 부드럽게 채움 (최소 fadeOrMinDelay 동안)
            float fillSpeed = 1f / Mathf.Max(fadeOrMinDelay, 0.01f);
            shown = Mathf.MoveTowards(shown, target, fillSpeed * Time.unscaledDeltaTime);

            if (progressBar != null) progressBar.value = shown;
            if (progressText != null) progressText.text = $"LOADING... {(shown * 100f):F0}%";

            // 실제 로딩 완료 + 게이지 가득 + 최소 대기 시간 충족 시 씬 전환 허용
            if (op.progress >= 0.9f && shown >= 1f && timer >= fadeOrMinDelay)
            {
                op.allowSceneActivation = true;
            }
        }
    }
}