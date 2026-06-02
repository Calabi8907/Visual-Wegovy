using UnityEngine;
using TMPro;
using UnityEngine.UI;

[ExecuteAlways]
public class SliderTextUpdater : MonoBehaviour
{
    public TextMeshProUGUI textElement;
    public Slider slider;

    private static readonly string[] labels = { "전혀 배고프지 않음", "조금 배고픔", "보통", "꽤 배고픔", "매우 배고픔" };
    private static readonly Color[] valueColors = {
        new Color(0.30f, 0.69f, 0.31f),
        new Color(0.80f, 0.86f, 0.22f),
        new Color(1.00f, 0.92f, 0.23f),
        new Color(1.00f, 0.60f, 0.00f),
        new Color(0.96f, 0.26f, 0.21f)
    };

    void Awake()
    {
        CleanupLegacy();
        if (textElement != null && textElement.transform.parent.Find("_ValueBoxBG") == null)
            SetupValueBox();
    }

#if UNITY_EDITOR
    void OnValidate() => CleanupLegacy();
#endif

    private void CleanupLegacy()
    {
        if (textElement == null) return;
        var parent = textElement.transform.parent;

        var border = parent.Find("_ValueBoxBorder");
        if (border != null)
        {
            if (Application.isPlaying) Destroy(border.gameObject);
            else DestroyImmediate(border.gameObject);
        }

        var bgT = parent.Find("_ValueBoxBG");
        if (bgT != null)
        {
            var outline = bgT.GetComponent<Outline>();
            if (outline != null)
            {
                if (Application.isPlaying) Destroy(outline);
                else DestroyImmediate(outline);
            }
            var img = bgT.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = CreateFillSprite();
                img.color = Color.white;
                img.type = Image.Type.Sliced;
            }
        }
    }

    private float _lastValue = -1f;

    void OnEnable()
    {
        if (slider != null)
            SetupSliderGradient();
        UpdateText();
    }

    private RectTransform _handleRect;

    void Update()
    {
        if (slider == null) return;
        FixHandleShape();
        if (!Mathf.Approximately(slider.value, _lastValue))
        {
            _lastValue = slider.value;
            UpdateText();
        }
    }

    private void FixHandleShape()
    {
        if (_handleRect == null)
        {
            Transform handleT = slider.transform.Find("Handle Slide Area/Handle");
            if (handleT == null)
                foreach (Transform t in slider.GetComponentsInChildren<Transform>())
                    if (t.name == "Handle") { handleT = t; break; }
            if (handleT != null)
                _handleRect = handleT.GetComponent<RectTransform>();
        }
        if (_handleRect == null) return;

        _handleRect.anchorMin = new Vector2(_handleRect.anchorMin.x, 0.5f);
        _handleRect.anchorMax = new Vector2(_handleRect.anchorMax.x, 0.5f);
        _handleRect.sizeDelta = new Vector2(60f, 60f);
    }

    private void SetupValueBox()
    {
        var parent = textElement.transform.parent;
        RectTransform textRect = textElement.GetComponent<RectTransform>();
        Vector2 boxSize = textRect.sizeDelta + new Vector2(50f, 80f);
        Vector2 boxPos  = textRect.anchoredPosition + new Vector2(0f, -15f);

        textElement.margin = new Vector4(0, 20f, 0, 0);

        GameObject bgObj = new GameObject("_ValueBoxBG");
        bgObj.transform.SetParent(parent, false);
        bgObj.transform.SetSiblingIndex(textElement.transform.GetSiblingIndex());
        Image bg = bgObj.AddComponent<Image>();
        bg.sprite = CreateFillSprite();
        bg.type = Image.Type.Sliced;
        bg.color = Color.white;
        bg.raycastTarget = false;
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = textRect.anchorMin;
        bgRect.anchorMax = textRect.anchorMax;
        bgRect.anchoredPosition = boxPos;
        bgRect.sizeDelta = boxSize;
    }

    private Sprite CreateFillSprite()
    {
        int w = 256, h = 128, r = 24;
        Color fill = new Color(0.17f, 0.10f, 0.00f, 1f);
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.hideFlags = HideFlags.DontSave;
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[w * h];

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int cx = Mathf.Clamp(x, r, w - 1 - r);
            int cy = Mathf.Clamp(y, r, h - 1 - r);
            float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            pixels[y * w + x] = dist <= r ? fill : Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f,
                                   0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    private void SetupSliderGradient()
    {
        Sprite rainbow = CreateRainbowSprite();

        Transform bgT = slider.transform.Find("Background");
        if (bgT != null)
        {
            Image img = bgT.GetComponent<Image>();
            if (img != null) { img.sprite = rainbow; img.type = Image.Type.Simple; img.color = Color.white; }
        }

        Transform fillT = slider.transform.Find("Fill Area/Fill");
        if (fillT != null)
        {
            Image img = fillT.GetComponent<Image>();
            if (img != null) img.color = Color.clear;
        }

        Transform handleT = slider.transform.Find("Handle Slide Area/Handle");
        if (handleT == null)
            foreach (Transform t in slider.GetComponentsInChildren<Transform>())
                if (t.name == "Handle") { handleT = t; break; }

        if (handleT != null)
        {
            Image img = handleT.GetComponent<Image>();
            if (img != null) img.color = Color.white;
            RectTransform rt = handleT.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(60f, 60f);
            }
        }
    }

    private Sprite CreateRainbowSprite()
    {
        int width = 512, height = 16;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.hideFlags = HideFlags.DontSave;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] stops = {
            new Color(0.30f, 0.69f, 0.31f),
            new Color(0.80f, 0.86f, 0.22f),
            new Color(1.00f, 0.92f, 0.23f),
            new Color(1.00f, 0.60f, 0.00f),
            new Color(0.96f, 0.26f, 0.21f)
        };

        Color[] pixels = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            float scaled = t * (stops.Length - 1);
            int i = Mathf.FloorToInt(scaled);
            int j = Mathf.Min(i + 1, stops.Length - 1);
            Color c = Color.Lerp(stops[i], stops[j], scaled - i);
            for (int y = 0; y < height; y++)
                pixels[y * width + x] = c;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    public void UpdateText()
    {
        if (slider == null || textElement == null) return;
        int value = Mathf.RoundToInt(slider.value);
        int idx = Mathf.Clamp(value / 25, 0, 4);
        string hex = ColorUtility.ToHtmlStringRGB(valueColors[idx]);
        textElement.alignment = TextAlignmentOptions.Center;
        textElement.text = $"<color=#{hex}><size=56><b>{value}</b></size></color>\n<size=18>{labels[idx]}</size>";
    }
}
