using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;
using DG.Tweening;
using Ami.BroAudio;

public class UIGameplay : UIBase
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private SpiritStoneDisplay spiritStoneDisplay;

    [Header("Damage Screen Blink")]
    [SerializeField] private Image damageBlinkImage;
    [SerializeField] private Color blinkColor = new Color(0.85f, 0.12f, 0.12f, 0.55f);
    [SerializeField] private float blinkInDuration = 0.06f;
    [SerializeField] private float blinkOutDuration = 0.24f;
    [SerializeField] private float soulStoneAnimationDelay = 0.12f;

    public static UIGameplay Instance { get; private set; }
    public static int Reputation { get; private set; }

    private int displayedReputation;
    private int activeFlyingScores;
    private readonly System.Collections.Generic.List<GameObject> activeFlyingObjects = new();

    public void CompleteAllFlyingScores()
    {
        for (int i = activeFlyingObjects.Count - 1; i >= 0; i--)
        {
            if (activeFlyingObjects[i] != null)
            {
                activeFlyingObjects[i].transform.DOKill();
                Destroy(activeFlyingObjects[i]);
            }
        }
        activeFlyingObjects.Clear();
        activeFlyingScores = 0;
        RefreshDisplay();
    }

    public static void ResetReputation()
    {
        Reputation = 0;
        if (Instance != null)
        {
            Instance.CompleteAllFlyingScores();
            Instance.displayedReputation = 0;
            Instance.activeFlyingScores = 0;
            if (Instance.scoreText != null)
            {
                Instance.scoreText.transform.DOKill();
                Instance.scoreText.DOKill();
                Instance.scoreText.text = "0";
            }
        }
        if (ReputationManager.Instance != null)
        {
            ReputationManager.Instance.ResetReputation();
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshDisplay();
    }

    private void OnEnable()
    {
        Instance = this;

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

        RefreshDisplay();

        if (spiritStoneDisplay != null)
        {
            spiritStoneDisplay.ResetDisplay();
        }
    }

    public void RefreshDisplay()
    {
        displayedReputation = (ReputationManager.Instance != null) ? ReputationManager.Instance.GetScore() : Reputation;
        activeFlyingScores = 0;
        if (scoreText != null)
        {
            scoreText.transform.DOKill();
            scoreText.DOKill();
            scoreText.text = displayedReputation.ToString();
        }
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

        if (damageBlinkImage != null)
        {
            damageBlinkImage.DOKill();
            damageBlinkImage.gameObject.SetActive(false);
        }

        CompleteAllFlyingScores();

        if (LevelManager.Instance.IsPause) ResumeGame();
    }

    private static Sprite _vignetteSprite;

    private static Sprite GetVignetteSprite()
    {
        if (_vignetteSprite != null) return _vignetteSprite;

        int res = 128;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float center = (res - 1) * 0.5f;
        Color[] colors = new Color[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = Mathf.SmoothStep(0.25f, 1.0f, dist);
                colors[y * res + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        _vignetteSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        return _vignetteSprite;
    }

    private void EnsureDamageBlinkOverlay()
    {
        if (damageBlinkImage != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform canvasRoot = (canvas != null && canvas.rootCanvas != null) ? canvas.rootCanvas.transform : transform;

        GameObject blinkObj = new GameObject("DamageBlinkOverlay");
        blinkObj.layer = gameObject.layer;
        blinkObj.transform.SetParent(canvasRoot, false);
        blinkObj.transform.SetAsLastSibling();

        RectTransform rect = blinkObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        damageBlinkImage = blinkObj.AddComponent<Image>();
        damageBlinkImage.sprite = GetVignetteSprite();
        damageBlinkImage.type = Image.Type.Simple;
        damageBlinkImage.raycastTarget = false;

        Color c = blinkColor;
        c.a = 0f;
        damageBlinkImage.color = c;
        blinkObj.SetActive(false);
    }

    public void TriggerDamageBlink()
    {
        EnsureDamageBlinkOverlay();
        if (damageBlinkImage == null) return;

        damageBlinkImage.DOKill();
        damageBlinkImage.gameObject.SetActive(true);

        Color startCol = blinkColor;
        startCol.a = 0f;
        damageBlinkImage.color = startCol;

        Sequence blinkSeq = DOTween.Sequence().SetUpdate(true);
        blinkSeq.Append(damageBlinkImage.DOFade(blinkColor.a, blinkInDuration).SetEase(Ease.OutQuad));
        blinkSeq.Append(damageBlinkImage.DOFade(0f, blinkOutDuration).SetEase(Ease.InQuad));
        blinkSeq.OnComplete(() =>
        {
            if (damageBlinkImage != null)
                damageBlinkImage.gameObject.SetActive(false);
        });
    }

    private void HandleTakeDamage(int before, int after)
    {
        if (after < before)
        {
            TriggerDamageBlink();
            if (soulStoneAnimationDelay > 0f)
            {
                DOVirtual.DelayedCall(soulStoneAnimationDelay, () =>
                {
                    if (spiritStoneDisplay != null)
                        spiritStoneDisplay.SetCount(after);
                }).SetUpdate(true);
            }
            else
            {
                if (spiritStoneDisplay != null)
                    spiritStoneDisplay.SetCount(after);
            }
        }
        else
        {
            if (spiritStoneDisplay != null)
                spiritStoneDisplay.SetCount(after);
        }
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
        activeFlyingObjects.Add(flyObj);
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

        if (GameManager.Instance != null)
        {
            if (isPositive)
            {
                if (GameManager.Instance.correctNumberPopup.IsValid())
                    GameManager.Instance.PlayAudio(GameManager.Instance.correctNumberPopup);
            }
            else
            {
                if (GameManager.Instance.numberPopupWrong.IsValid())
                    GameManager.Instance.PlayAudio(GameManager.Instance.numberPopupWrong);
            }
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
            activeFlyingObjects.Remove(flyObj);
            activeFlyingScores = Mathf.Max(0, activeFlyingScores - 1);

            if (GameManager.Instance != null)
            {
                if (isPositive)
                {
                    if (GameManager.Instance.numberBumpCorrect.IsValid())
                        GameManager.Instance.PlayAudio(GameManager.Instance.numberBumpCorrect);
                }
                else
                {
                    if (GameManager.Instance.numberBumpWrong.IsValid())
                    {
                        GameManager.Instance.PlayAudio(GameManager.Instance.numberBumpWrong);
                    }
                    else
                    {
                        Debug.LogWarning("[UIGameplay] numberBumpWrong is not assigned on GameManager in Inspector!");
                    }
                }
            }
            else if (!isPositive)
            {
                Debug.LogWarning("[UIGameplay] Cannot play numberBumpWrong because GameManager.Instance is null (game must be started from MainMenu).");
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