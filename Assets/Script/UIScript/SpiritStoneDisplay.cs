using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menampilkan spirit stone seperti sistem heart.
/// Stone aktif = warna normal, stone terpakai = gelap.
/// Pasang di panel "Spirit Stones", lalu isi array stones dari kiri ke kanan.
/// </summary>
public class SpiritStoneDisplay : MonoBehaviour
{
    [Tooltip("Urutkan dari kiri ke kanan")]
    [SerializeField] private Image[] stones;

    [Header("Warna")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color usedColor = new Color(0.25f, 0.25f, 0.35f, 1f);

    [Header("Opsional: ganti sprite saat terpakai")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite usedSprite;

    /// <summary>remaining = jumlah stone yang masih aktif.</summary>
    public void SetCount(int remaining)
    {
        for (int i = 0; i < stones.Length; i++)
        {
            if (stones[i] == null) continue;

            // Stone paling kanan yang gelap duluan
            bool active = i < remaining;
            stones[i].color = active ? activeColor : usedColor;

            if (usedSprite != null && activeSprite != null)
                stones[i].sprite = active ? activeSprite : usedSprite;
        }
    }
}