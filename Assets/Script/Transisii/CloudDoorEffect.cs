using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PixeLadder.EasyTransition;

/// <summary>
/// Transisi "pintu geser awan":
/// AnimateOut -> awan kiri & kanan masuk menutup layar.
/// AnimateIn  -> awan keluar lagi ke kiri & kanan, lalu dihapus.
///
/// Prefab yang dibutuhkan: sebuah Canvas (Screen Space - Overlay, Sort Order 1000)
/// dengan dua child Image bernama persis "CloudLeft" dan "CloudRight".
///
/// PENTING: field Transition Material di asset ini tetap harus diisi
/// (material apa saja), karena SceneTransitioner selalu membuat salinannya.
/// </summary>
[CreateAssetMenu(fileName = "CloudDoorEffect", menuName = "Easy Transition/Cloud Door Effect")]
public class CloudDoorEffect : TransitionEffect
{
    [Header("Prefab")]
    [Tooltip("Canvas prefab berisi child 'CloudLeft' dan 'CloudRight'")]
    [SerializeField] private GameObject cloudDoorPrefab;

    [Header("Tutup (awan masuk)")]
    [SerializeField] private float closeDuration = 0.7f;
    [SerializeField] private AnimationCurve closeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Buka (awan keluar)")]
    [SerializeField] private float openDuration = 0.7f;
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float holdBeforeOpen = 0.15f;

    // State runtime, tidak disimpan ke asset
    [System.NonSerialized] private GameObject instance;
    [System.NonSerialized] private RectTransform left;
    [System.NonSerialized] private RectTransform right;

    public override IEnumerator AnimateOut(Image image)
    {
        // Image penutup bawaan SceneTransitioner tidak dipakai, awan yang menutup layar
        image.enabled = false;
        yield return Close();
    }

    public override IEnumerator AnimateIn(Image image)
    {
        yield return Open();
        // Nyalakan lagi supaya effect lain (Fade, Wipe, dll) tetap normal
        image.enabled = true;
    }

    private IEnumerator Close()
    {
        if (instance == null) Spawn();
        if (instance == null) yield break;

        // 0 = awan di luar layar, 1 = awan menutup layar
        yield return Slide(0f, 1f, closeDuration, closeCurve);
    }

    private IEnumerator Open()
    {
        if (instance == null) yield break;

        yield return null; // lewati frame berat setelah scene aktif
        if (holdBeforeOpen > 0f)
            yield return new WaitForSecondsRealtime(holdBeforeOpen);

        yield return Slide(1f, 0f, openDuration, openCurve);

        Destroy(instance);
        instance = null;
    }

    private void Spawn()
    {
        if (cloudDoorPrefab == null)
        {
            Debug.LogError("[CloudDoorEffect] Cloud Door Prefab belum di-assign.", this);
            return;
        }

        instance = Instantiate(cloudDoorPrefab);
        DontDestroyOnLoad(instance); // tetap hidup saat scene diganti

        left = instance.transform.Find("CloudLeft") as RectTransform;
        right = instance.transform.Find("CloudRight") as RectTransform;

        if (left == null || right == null)
        {
            Debug.LogError("[CloudDoorEffect] Child 'CloudLeft' / 'CloudRight' tidak ditemukan di prefab.", this);
            Destroy(instance);
            instance = null;
            return;
        }

        Canvas.ForceUpdateCanvases(); // supaya rect.width sudah benar
        SetProgress(0f);
    }

    private IEnumerator Slide(float from, float to, float duration, AnimationCurve curve)
    {
        duration = Mathf.Max(duration, 0.0001f);
        float t = 0f;

        while (t < duration)
        {
            t += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f); // cegah lompatan saat frame lag
            float k = curve.Evaluate(Mathf.Clamp01(t / duration));
            SetProgress(Mathf.LerpUnclamped(from, to, k));
            yield return null;
        }

        SetProgress(to);
    }

    private void SetProgress(float p)
    {
        float lx = Mathf.LerpUnclamped(-left.rect.width, 0f, p);
        float rx = Mathf.LerpUnclamped(right.rect.width, 0f, p);
        left.anchoredPosition = new Vector2(lx, left.anchoredPosition.y);
        right.anchoredPosition = new Vector2(rx, right.anchoredPosition.y);
    }
}