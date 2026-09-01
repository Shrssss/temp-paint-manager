using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePaintManager : MonoBehaviour
{
    [SerializeField] private int brushSize = 10;
    [SerializeField] private Color32 brushColor = new Color32(255, 255, 255, 255);

    private SpriteRenderer spriteRenderer;
    private Texture2D drawTexture;
    private Color32[] buffer;
    private bool touching = false;
    private Vector2 prevPoint;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Sprite originalSprite = spriteRenderer.sprite;

        Texture2D mainTexture = originalSprite.texture;
        int width = mainTexture.width;
        int height = mainTexture.height;

        Color[] pixels = mainTexture.GetPixels();
        buffer = new Color32[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            buffer[i] = pixels[i];
        }

        drawTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Point;

        // 動的テクスチャから新しいスプライトを生成して適用
        Sprite newSprite = Sprite.Create(
            drawTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            originalSprite.pixelsPerUnit
        );
        spriteRenderer.sprite = newSprite;

        UpdateTexture();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            // 2Dレイキャストでコライダーとの接触を判定
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos2D, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // ワールド座標をテクスチャのピクセル座標(X, Y)に変換
                Vector2 drawPoint = WorldToTexturePoint(hit.point);

                if (touching)
                {
                    DrawLine(prevPoint, drawPoint);
                }
                else
                {
                    Draw(drawPoint);
                }

                prevPoint = drawPoint;
                touching = true;
                UpdateTexture();
            }
            else
            {
                touching = false;
            }
        }
        else
        {
            touching = false;
        }
    }

    // ワールド座標からスプライトのピクセル座標へ変換する処理
    private Vector2 WorldToTexturePoint(Vector2 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // スプライトのローカルサイズを取得してUV(0.0 ~ 1.0)を算出
        Vector2 spriteSize = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
        float u = (localPos.x / spriteSize.x) + 0.5f;
        float v = (localPos.y / spriteSize.y) + 0.5f;

        int px = Mathf.Clamp((int)(u * drawTexture.width), 0, drawTexture.width - 1);
        int py = Mathf.Clamp((int)(v * drawTexture.height), 0, drawTexture.height - 1);

        return new Vector2(px, py);
    }

    public void DrawLine(Vector2 p, Vector2 q)
    {
        float distance = Vector2.Distance(p, q);
        int lerpNum = Mathf.CeilToInt(distance);

        for (int i = 0; i <= lerpNum; i++)
        {
            float t = lerpNum == 0 ? 0 : (float)i / lerpNum;
            Draw(Vector2.Lerp(p, q, t));
        }
    }

    public void Draw(Vector2 p)
    {
        int px = (int)p.x;
        int py = (int)p.y;
        int sqrBrushSize = brushSize * brushSize;
        int width = drawTexture.width;
        int height = drawTexture.height;

        int minX = Mathf.Max(0, px - brushSize);
        int maxX = Mathf.Min(width, px + brushSize + 1);
        int minY = Mathf.Max(0, py - brushSize);
        int maxY = Mathf.Min(height, py + brushSize + 1);

        for (int x = minX; x < maxX; x++)
        {
            int dx = x - px;
            int dx2 = dx * dx;

            for (int y = minY; y < maxY; y++)
            {
                int dy = y - py;
                if (dx2 + (dy * dy) < sqrBrushSize)
                {
                    buffer[x + width * y] = brushColor;
                }
            }
        }
    }

    private void UpdateTexture()
    {
        drawTexture.SetPixels32(buffer);
        drawTexture.Apply(false);
    }
}