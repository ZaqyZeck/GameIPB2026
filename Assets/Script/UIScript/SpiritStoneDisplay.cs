using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpiritStoneDisplay : MonoBehaviour
{
    [Tooltip("Urutkan dari kiri ke kanan")]
    [SerializeField] private Image[] stones;

    [Header("Warna Normal & Bekas")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color usedColor = new Color(0.25f, 0.25f, 0.35f, 1f);

    [Header("Opsional: ganti sprite saat terpakai")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite usedSprite;

    [Header("Thanos Dust Settings")]
    [Tooltip("Durasi disintegrasi menjadi debu dalam detik.")]
    [SerializeField] private float dissolveDuration = 0.95f;
    [Tooltip("Warna kilau api/bara (Ember) di tepian saat batu pecah menjadi abu.")]
    [ColorUsage(true, true)]
    [SerializeField] private Color emberColor = new Color(0.35f, 0.85f, 1.0f, 1f);
    [Tooltip("Animasi getar kecil saat batu mulai hancur.")]
    [SerializeField] private bool punchOnBreak = true;
    [Tooltip("Setelah hancur menjadi debu, tampilkan siluet batu gelap/slot kosong.")]
    [SerializeField] private bool preserveDarkSilhouette = true;
    [Tooltip("Jumlah partikel debu yang terbang melayang keluar dari batu.")]
    [SerializeField] private int dustParticleCount = 45;

    private int currentCount = -1;
    private Coroutine[] activeCoroutines;

    private static Sprite _emberParticleSprite;
    private static Sprite _ashParticleSprite;

    private void Awake()
    {
        InitializeArrays();
    }

    private void Start()
    {
        if (currentCount == -1 && stones != null && stones.Length > 0)
        {
            ResetDisplay();
        }
    }

    public void ResetDisplay()
    {
        InitializeArrays();
        if (activeCoroutines != null)
        {
            for (int i = 0; i < activeCoroutines.Length; i++)
            {
                if (activeCoroutines[i] != null)
                {
                    StopCoroutine(activeCoroutines[i]);
                    activeCoroutines[i] = null;
                }
            }
        }

        currentCount = -1;
        if (stones != null && stones.Length > 0)
        {
            for (int i = 0; i < stones.Length; i++)
            {
                if (stones[i] != null)
                {
                    stones[i].transform.DOKill();
                    stones[i].DOKill();
                    stones[i].transform.localScale = Vector3.one;
                }
            }
            int initialCount = (HealthManager.Instance != null) ? HealthManager.Instance.GetHealth() : stones.Length;
            SetCount(initialCount);
        }
    }

    private void InitializeArrays()
    {
        if (stones == null) return;
        if (activeCoroutines == null || activeCoroutines.Length != stones.Length)
        {
            activeCoroutines = new Coroutine[stones.Length];
        }
    }

    public void SetCount(int remaining)
    {
        InitializeArrays();
        if (stones == null || stones.Length == 0) return;

        if (currentCount == -1)
        {
            currentCount = remaining;
            for (int i = 0; i < stones.Length; i++)
            {
                if (stones[i] == null) continue;
                bool active = i < remaining;
                stones[i].color = active ? activeColor : usedColor;
                stones[i].preserveAspect = true;
                if (usedSprite != null && activeSprite != null)
                    stones[i].sprite = active ? activeSprite : usedSprite;
            }
            return;
        }

        if (remaining < currentCount)
        {
            for (int i = remaining; i < currentCount; i++)
            {
                if (i >= 0 && i < stones.Length && stones[i] != null)
                {
                    AnimateThanosDissolve(i);
                }
            }
        }
        else if (remaining > currentCount)
        {
            for (int i = currentCount; i < remaining; i++)
            {
                if (i >= 0 && i < stones.Length && stones[i] != null)
                {
                    RestoreStone(i);
                }
            }
        }

        currentCount = remaining;
    }

    private void AnimateThanosDissolve(int index)
    {
        if (activeCoroutines[index] != null)
        {
            StopCoroutine(activeCoroutines[index]);
            activeCoroutines[index] = null;
        }

        activeCoroutines[index] = StartCoroutine(DissolveRoutine(index));
    }

    private IEnumerator DissolveRoutine(int index)
    {
        Image stone = stones[index];
        if (stone == null) yield break;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayAudio(GameManager.Instance.burn);
        }

        stone.transform.DOKill();
        stone.DOKill();
        stone.material = null;
        stone.preserveAspect = true;
        stone.transform.localScale = Vector3.one;
        stone.color = activeColor;
        if (activeSprite != null) stone.sprite = activeSprite;

        if (punchOnBreak)
        {
            stone.transform.DOShakePosition(0.38f, strength: new Vector3(5f, 5f, 0f), vibrato: 25, randomness: 90)
                .SetUpdate(true);
        }

        stone.DOColor(new Color(0.65f, 0.92f, 1.0f, 1f), 0.12f).SetUpdate(true).OnComplete(() =>
        {
            stone.DOColor(usedColor, dissolveDuration * 0.7f).SetUpdate(true);
        });

        stone.transform.DOScale(new Vector3(0.78f, 0.78f, 1f), dissolveDuration * 0.45f)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                stone.transform.DOScale(Vector3.one, 0.28f).SetEase(Ease.OutBack).SetUpdate(true);
            });

        SpawnStoneShards(stone, index);
        SpawnThanosDustBurst(stone, index);

        yield return new WaitForSecondsRealtime(dissolveDuration);

        if (stone != null)
        {
            stone.transform.localScale = Vector3.one;
            stone.preserveAspect = true;

            if (preserveDarkSilhouette)
            {
                stone.color = usedColor;
                if (usedSprite != null) stone.sprite = usedSprite;
            }
            else
            {
                stone.color = Color.clear;
            }
        }

        activeCoroutines[index] = null;
    }

    private void SpawnStoneShards(Image stone, int index)
    {
        if (stone == null) return;

        Canvas canvas = stone.canvas;
        if (canvas == null) canvas = stone.GetComponentInParent<Canvas>();
        Transform root = (canvas != null && canvas.rootCanvas != null) ? canvas.rootCanvas.transform : stone.transform.root;

        GameObject container = new GameObject($"ThanosShards_Burst_{index}");
        container.layer = stone.gameObject.layer;
        RectTransform containerRect = container.AddComponent<RectTransform>();
        container.transform.SetParent(root, false);
        containerRect.position = stone.rectTransform.position;
        containerRect.sizeDelta = stone.rectTransform.sizeDelta;
        container.transform.SetAsLastSibling();

        int shardCount = 6;
        for (int i = 0; i < shardCount; i++)
        {
            GameObject shardObj = new GameObject($"Shard_{i}");
            shardObj.layer = stone.gameObject.layer;
            RectTransform sRect = shardObj.AddComponent<RectTransform>();
            shardObj.transform.SetParent(container.transform, false);

            float angle = (i / (float)shardCount) * Mathf.PI * 2f + Random.Range(-0.25f, 0.25f);
            float dist = Random.Range(30f, 70f);
            Vector2 burstDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 targetPos = burstDir * dist + new Vector2(Random.Range(20f, 50f), Random.Range(30f, 70f));

            float shardScale = Random.Range(0.35f, 0.55f);
            sRect.sizeDelta = stone.rectTransform.sizeDelta * shardScale;
            sRect.anchoredPosition = Random.insideUnitCircle * 8f;

            Image sImg = shardObj.AddComponent<Image>();
            sImg.sprite = stone.sprite;
            sImg.preserveAspect = true;
            sImg.raycastTarget = false;
            sImg.color = Color.Lerp(emberColor, activeColor, 0.5f);

            float life = Random.Range(0.6f, 0.9f);
            sRect.localScale = Vector3.one;

            sRect.DOAnchorPos(targetPos, life).SetEase(Ease.OutQuad).SetUpdate(true);
            sRect.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), life, RotateMode.FastBeyond360).SetUpdate(true);
            sRect.DOScale(Vector3.zero, life * 0.5f).SetDelay(life * 0.5f).SetEase(Ease.InQuad).SetUpdate(true);
            sImg.DOColor(usedColor, life * 0.5f).SetUpdate(true).OnComplete(() =>
            {
                sImg.DOFade(0f, life * 0.5f).SetUpdate(true);
            });
        }

        Destroy(container, 1.2f);
    }

    private void SpawnThanosDustBurst(Image stone, int index)
    {
        if (stone == null) return;

        Canvas canvas = stone.canvas;
        if (canvas == null) canvas = stone.GetComponentInParent<Canvas>();
        Transform canvasRoot = (canvas != null && canvas.rootCanvas != null) ? canvas.rootCanvas.transform : stone.transform.root;

        GameObject container = new GameObject($"ThanosDust_Burst_{index}");
        container.layer = stone.gameObject.layer;
        RectTransform containerRect = container.AddComponent<RectTransform>();
        container.transform.SetParent(canvasRoot, false);
        containerRect.position = stone.rectTransform.position;
        containerRect.sizeDelta = stone.rectTransform.sizeDelta;
        container.transform.SetAsLastSibling();

        float width = stone.rectTransform.rect.width > 0 ? stone.rectTransform.rect.width : 60f;
        float height = stone.rectTransform.rect.height > 0 ? stone.rectTransform.rect.height : 85f;

        float maxLifetime = 0f;

        Color[] dustPalette = new Color[]
        {
            new Color(0.35f, 0.80f, 1.0f, 1f),
            new Color(0.18f, 0.58f, 1.0f, 1f),
            new Color(0.32f, 0.45f, 0.95f, 1f),
            new Color(0.75f, 0.94f, 1.0f, 1f),
            new Color(0.92f, 0.98f, 1.0f, 1f),
            emberColor,
            new Color(0.18f, 0.16f, 0.28f, 0.95f),
            new Color(0.28f, 0.26f, 0.40f, 0.95f)
        };

        Sprite emberSpr = GetEmberParticleSprite();
        Sprite ashSpr = GetAshParticleSprite();

        for (int p = 0; p < dustParticleCount; p++)
        {
            GameObject pObj = new GameObject($"Dust_{p}");
            pObj.layer = stone.gameObject.layer;
            RectTransform pRect = pObj.AddComponent<RectTransform>();
            pObj.transform.SetParent(container.transform, false);

            float spawnX = Random.Range(-width * 0.45f, width * 0.45f);
            float spawnY = Random.Range(-height * 0.45f, height * 0.45f);
            pRect.anchoredPosition = new Vector2(spawnX, spawnY);

            float pSize = Random.Range(12f, 26f);
            pRect.sizeDelta = new Vector2(pSize, pSize * Random.Range(0.7f, 1.3f));

            Image pImage = pObj.AddComponent<Image>();
            bool isEmber = (p % 4 != 0);
            pImage.sprite = isEmber ? emberSpr : ashSpr;
            pImage.raycastTarget = false;
            pImage.color = dustPalette[Random.Range(0, dustPalette.Length)];

            float normalizedY = (spawnY + height * 0.5f) / height;
            float delay = normalizedY * 0.22f + Random.Range(0f, 0.08f);

            float driftX = Random.Range(25f, 150f);
            float driftY = Random.Range(70f, 230f);
            Vector2 targetPos = pRect.anchoredPosition + new Vector2(driftX, driftY);

            float lifetime = Random.Range(0.9f, 1.4f);
            float totalDuration = delay + lifetime;
            if (totalDuration > maxLifetime) maxLifetime = totalDuration;

            pRect.localScale = Vector3.zero;

            float popDuration = 0.12f;
            float driftDuration = Mathf.Max(0.1f, lifetime - popDuration);
            float targetScale = Random.Range(1.1f, 1.6f);

            Sequence scaleSeq = DOTween.Sequence();
            scaleSeq.SetUpdate(true);
            scaleSeq.AppendInterval(delay);
            scaleSeq.Append(pRect.DOScale(Vector3.one * targetScale, popDuration).SetEase(Ease.OutBack));
            scaleSeq.AppendInterval(driftDuration * 0.3f);
            scaleSeq.Append(pRect.DOScale(Vector3.zero, driftDuration * 0.7f).SetEase(Ease.InQuad));

            pRect.DOAnchorPos(targetPos, lifetime)
                .SetDelay(delay)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            pRect.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), lifetime, RotateMode.FastBeyond360)
                .SetDelay(delay)
                .SetUpdate(true);

            pImage.DOFade(0f, driftDuration * 0.65f)
                .SetDelay(delay + driftDuration * 0.35f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true);
        }

        Destroy(container, maxLifetime + 0.3f);
    }

    private void RestoreStone(int index)
    {
        if (activeCoroutines[index] != null)
        {
            StopCoroutine(activeCoroutines[index]);
            activeCoroutines[index] = null;
        }

        Image stone = stones[index];
        if (stone == null) return;

        stone.transform.DOKill();
        stone.DOKill();
        stone.material = null;
        stone.preserveAspect = true;
        stone.transform.localScale = Vector3.one;
        stone.color = activeColor;
        if (activeSprite != null) stone.sprite = activeSprite;
    }

    #region Procedural Particle Sprites
    private static Sprite GetEmberParticleSprite()
    {
        if (_emberParticleSprite != null) return _emberParticleSprite;

        int res = 32;
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
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha;
                colors[y * res + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        _emberParticleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        return _emberParticleSprite;
    }

    private static Sprite GetAshParticleSprite()
    {
        if (_ashParticleSprite != null) return _ashParticleSprite;

        int res = 32;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        float center = (res - 1) * 0.5f;
        Color[] colors = new Color[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float manhattan = (Mathf.Abs(x - center) + Mathf.Abs(y - center)) / center;
                float alpha = manhattan <= 1.0f ? 1.0f : 0.0f;
                colors[y * res + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        _ashParticleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        return _ashParticleSprite;
    }
    #endregion

    #region Context Menu Helpers
    [ContextMenu("Test Take 1 Damage (Dissolve)")]
    public void TestTakeDamage()
    {
        if (stones == null || stones.Length == 0) return;

        if (currentCount == -1)
        {
            currentCount = stones.Length;
        }

        int target = Mathf.Max(0, currentCount - 1);
        SetCount(target);
    }

    [ContextMenu("Test Reset (Restore All)")]
    public void TestResetStones()
    {
        if (stones == null || stones.Length == 0) return;
        currentCount = 0;
        SetCount(stones.Length);
    }

    [ContextMenu("Test Thanos Dissolve First Stone")]
    public void TestThanosDissolveFirst()
    {
        if (stones == null || stones.Length == 0) return;
        AnimateThanosDissolve(0);
    }
    #endregion
}