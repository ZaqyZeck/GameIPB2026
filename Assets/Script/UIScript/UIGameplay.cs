using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;
using DG.Tweening;

public class UIGameplay : UIBase
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private SpiritStoneDisplay spiritStoneDisplay;

    public static int Reputation { get; private set; }

    private int displayedReputation;
    private int activeFlyingScores;

    public static void ResetReputation()
    {
        Reputation = 0;
    }

    private void Start()
    {
        displayedReputation = (ReputationManager.Instance != null) ? ReputationManager.Instance.GetScore() : Reputation;
        if (scoreText != null)
        {
            scoreText.text = displayedReputation.ToString();
        }
    }

    private void OnEnable()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OpenPauseMenu);
        else
            Debug.LogWarning("[UIGameplay] Pause button belum di-assign.", this);

        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClicked);
            dropButton.gameObject.SetActive(false);
        }

        GameEventBus.OnTakeDamage += HandleTakeDamage;
        GameEventBus.OnReputationChange += HandleReputationChange;
        GameEventBus.OnReputationDeltaWorld += HandleReputationDeltaWorld;
    }

    private void Update()
    {
        if (dropButton != null)
        {
            bool holding = PlayerInteract.Instance != null && PlayerInteract.Instance.isHoldingObject;
            if (dropButton.gameObject.activeSelf != holding)
            {
                dropButton.gameObject.SetActive(holding);
            }
        }
    }

    private void OnDropButtonClicked()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.TriggerDrop();
        }
        else if (PlayerInteract.Instance != null)
        {
            PlayerInteract.Instance.DropHoldObject();
        }
    }

    private void OnDisable()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OpenPauseMenu);

        GameEventBus.OnTakeDamage -= HandleTakeDamage;
        GameEventBus.OnReputationChange -= HandleReputationChange;
        GameEventBus.OnReputationDeltaWorld -= HandleReputationDeltaWorld;

        if (LevelManager.Instance.IsPause) ResumeGame();
    }

    private void HandleTakeDamage(int before, int after)
    {
        if (spiritStoneDisplay != null)
            spiritStoneDisplay.SetCount(after);
    }

    private void HandleReputationChange(int before, int after)
    {
        Reputation = after;

        if (activeFlyingScores <= 0)
        {
            displayedReputation = after;
            if (scoreText != null)
                scoreText.text = after.ToString();
        }
    }

    private void HandleReputationDeltaWorld(int delta, int totalScore, Vector3 worldPos)
    {
        Reputation = totalScore;
        SpawnFlyingScore(delta, totalScore, worldPos);
    }

    public void SpawnFlyingScore(int delta, int targetTotalScore, Vector3 worldPos)
    {
        activeFlyingScores++;

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform canvasRoot = (canvas != null && canvas.rootCanvas != null) ? canvas.rootCanvas.transform : transform;

        GameObject flyObj = new GameObject("FlyingScoreText");
        flyObj.layer = gameObject.layer;
        RectTransform flyRect = flyObj.AddComponent<RectTransform>();
        flyObj.transform.SetParent(canvasRoot, false);
        flyObj.transform.SetAsLastSibling();

        Vector3 screenPos = Vector3.zero;
        Camera cam = Camera.main;
        if (cam != null && worldPos != Vector3.zero)
        {
            screenPos = cam.WorldToScreenPoint(worldPos);
        }
        else
        {
            screenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        }
        screenPos.z = 0f;

        flyRect.position = screenPos;
        flyRect.sizeDelta = new Vector2(250f, 70f);

        TextMeshProUGUI tmp = flyObj.AddComponent<TextMeshProUGUI>();
        if (scoreText != null)
        {
            tmp.font = scoreText.font;
            tmp.fontSharedMaterial = scoreText.fontSharedMaterial;
        }
        tmp.fontSize = 46;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        bool isPositive = delta >= 0;
        tmp.text = isPositive ? $"+{delta}" : $"{delta}";
        Color scoreColor = isPositive ? new Color(1.0f, 0.88f, 0.20f, 1f) : new Color(1.0f, 0.32f, 0.32f, 1f);
        tmp.color = scoreColor;

        if (isPositive && GameManager.Instance != null)
        {
            GameManager.Instance.PlayAudio(GameManager.Instance.correctNumberPopup);
        }

        Vector3 targetPos = scoreText != null ? scoreText.rectTransform.position : screenPos;
        targetPos.z = 0f;

        flyRect.localScale = Vector3.zero;
        Vector3 popUpPos = screenPos + new Vector3(0f, 40f, 0f);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(flyRect.DOScale(Vector3.one * 1.25f, 0.18f).SetEase(Ease.OutBack));
        seq.Append(flyRect.DOScale(Vector3.one, 0.12f).SetEase(Ease.InOutQuad));
        flyRect.DOMove(popUpPos, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);

        seq.AppendInterval(0.18f);

        Vector3 mid = (popUpPos + targetPos) * 0.5f;
        mid.y += 90f;
        mid.x += (targetPos.x > popUpPos.x) ? -40f : 40f;

        float flyDuration = 0.55f;
        seq.Append(flyRect.DOPath(new Vector3[] { mid, targetPos }, flyDuration, PathType.CatmullRom).SetEase(Ease.InQuad));
        seq.Join(flyRect.DOScale(Vector3.one * 0.85f, flyDuration).SetEase(Ease.InQuad));

        seq.AppendCallback(() =>
        {
            activeFlyingScores = Mathf.Max(0, activeFlyingScores - 1);

            if (isPositive && GameManager.Instance != null)
            {
                GameManager.Instance.PlayAudio(GameManager.Instance.numberBumpCorrect);
            }

            SpawnImpactSparkles(targetPos, scoreColor, canvasRoot);

            if (scoreText != null)
            {
                scoreText.transform.DOKill();
                scoreText.transform.localScale = Vector3.one;
                scoreText.transform.DOPunchScale(new Vector3(0.35f, 0.35f, 0f), 0.28f, 15, 1).SetUpdate(true);

                scoreText.DOKill(false);
                Color origColor = scoreText.color;
                scoreText.color = scoreColor;
                scoreText.DOColor(origColor, 0.35f).SetUpdate(true);

                int startVal = displayedReputation;
                displayedReputation = targetTotalScore;
                DOTween.To(() => startVal, val =>
                {
                    if (scoreText != null) scoreText.text = val.ToString();
                }, targetTotalScore, 0.25f).SetUpdate(true);
            }
            else
            {
                displayedReputation = targetTotalScore;
            }

            ReputationManager.Instance?.UpdateUI(targetTotalScore);
            GameEventBus.OnReputationChange?.Invoke(targetTotalScore - delta, targetTotalScore);

            flyRect.DOScale(Vector3.one * 1.35f, 0.1f).SetUpdate(true);
            tmp.DOFade(0f, 0.1f).SetUpdate(true).OnComplete(() =>
            {
                Destroy(flyObj);
            });
        });
    }

    private void SpawnImpactSparkles(Vector3 targetPos, Color sparkColor, Transform parent)
    {
        int count = 8;
        for (int i = 0; i < count; i++)
        {
            GameObject sObj = new GameObject("Sparkle");
            sObj.layer = gameObject.layer;
            RectTransform sRect = sObj.AddComponent<RectTransform>();
            sObj.transform.SetParent(parent, false);
            sRect.position = targetPos;
            sRect.sizeDelta = new Vector2(12f, 12f);

            Image sImg = sObj.AddComponent<Image>();
            sImg.raycastTarget = false;
            sImg.color = sparkColor;

            float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
            float dist = Random.Range(20f, 50f);
            Vector3 dest = targetPos + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0);

            float life = Random.Range(0.25f, 0.4f);
            sRect.DOMove(dest, life).SetEase(Ease.OutQuad).SetUpdate(true);
            sRect.DOScale(Vector3.zero, life).SetEase(Ease.InQuad).SetUpdate(true);
            sImg.DOFade(0f, life).SetEase(Ease.InQuad).SetUpdate(true);

            Destroy(sObj, life + 0.05f);
        }
    }

    public void OpenPauseMenu()
    {
        if (!LevelManager.Instance.IsPlaying)
        {
            Debug.LogWarning("sudah terpause");
            return;
        }

        if (UIManager.Instance == null)
        {
            Debug.LogError("[UIGameplay] UIManager tidak ditemukan.", this);
            return;
        }

        GameEventBus.OnPause?.Invoke();

        UIManager.Instance.ShowUI<UIPauseMenu>();
    }

    public void ResumeGame()
    {
        if (LevelManager.Instance.IsPlaying) return;

        GameEventBus.OnResume?.Invoke();
    }

    public void TogglePause()
    {
        if (LevelManager.Instance.IsPause) ResumeGame();
        else OpenPauseMenu();
    }

    [ContextMenu("Test Flying Score (+25)")]
    public void TestFlyingScorePositive()
    {
        Vector3 testPos = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 5f : Vector3.zero;
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.Reward(25, testPos);
        else
            SpawnFlyingScore(25, displayedReputation + 25, testPos);
    }

    [ContextMenu("Test Flying Score (-15)")]
    public void TestFlyingScoreNegative()
    {
        Vector3 testPos = Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 5f : Vector3.zero;
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.Penalize(15, testPos);
        else
            SpawnFlyingScore(-15, Mathf.Max(0, displayedReputation - 15), testPos);
    }
}