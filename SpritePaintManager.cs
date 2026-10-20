using UnityEngine;

// このスクリプトがアタッチされたGameObjectには、必ずSpriteRendererコンポーネントが必要であることを保証する
[RequireComponent(typeof(SpriteRenderer))]
public class SpritePaintManager : MonoBehaviour
{
    // インスペクターから設定可能なブラシの基本サイズ
    [SerializeField] private int brushSize = 10;
    // インスペクターから設定可能なブラシの基本色（デフォルトは白）[cite: 2]
    [SerializeField] private Color32 brushColor = new Color32(255, 255, 255, 255);

    // 自身のSpriteRendererコンポーネントの参照[cite: 2]
    private SpriteRenderer spriteRenderer;
    // 実際に書き換えを行う描画用のテクスチャ[cite: 2]
    private Texture2D drawTexture;
    // テクスチャのピクセルカラー情報を保持する1次元配列（バッファ）[cite: 2]
    private Color32[] buffer;

    void Start()
    {
        // 自身のSpriteRendererを取得[cite: 2]
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 元々設定されているスプライトを取得[cite: 2]
        Sprite originalSprite = spriteRenderer.sprite;

        // 元のスプライトが持っているテクスチャを取得[cite: 2]
        Texture2D mainTexture = originalSprite.texture;
        // テクスチャの幅と高さを取得[cite: 2]
        int width = mainTexture.width;
        int height = mainTexture.height;

        // 元のテクスチャのピクセルカラー情報を配列として全て取得[cite: 2]
        Color[] pixels = mainTexture.GetPixels();
        // 処理を高速化するため、Color32の配列としてバッファを初期化[cite: 2]
        buffer = new Color32[pixels.Length];
        
        // 取得したピクセル情報をバッファにコピー（初期状態の画像を構築）[cite: 2]
        for (int i = 0; i < pixels.Length; i++)
        {
            buffer[i] = pixels[i];
        }

        // 描画用の新しいテクスチャを元のテクスチャと同じサイズで生成（RGBA32フォーマット、ミップマップなし）[cite: 2]
        drawTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        // ドット絵のようにくっきり描画するため、フィルターモードをPointに設定[cite: 2]
        drawTexture.filterMode = FilterMode.Point;

        // 生成した描画用テクスチャをもとに、新しいスプライトを生成[cite: 2]
        Sprite newSprite = Sprite.Create(
            drawTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            originalSprite.pixelsPerUnit
        );
        // 生成した新しいスプライトをSpriteRendererにセット[cite: 2]
        spriteRenderer.sprite = newSprite;

        // 初期バッファの内容をテクスチャに反映[cite: 2]
        UpdateTexture();
    }

    /// <summary>
    /// 指定したワールド座標に任意のサイズ・色で円を塗る（PaintBulletなど外部から呼び出し）
    /// </summary>
    public void PaintCircleAtWorldPoint(Vector2 worldPos, int size, Color32? customColor = null)
    {
        // 現在のブラシ色を一時保存
        Color32 originalColor = brushColor;
        // カスタムカラー（弾の色など）が指定されていれば、ブラシ色をそれに変更
        if (customColor.HasValue)
        {
            brushColor = customColor.Value;
        }

        // ワールド座標をテクスチャ上のピクセル座標に変換
        Vector2 drawPoint = WorldToTexturePoint(worldPos);
        // 指定されたピクセル座標に、指定されたサイズで円を描画（バッファの更新）
        Draw(drawPoint, size);
        // バッファの内容をテクスチャに反映
        UpdateTexture();

        // ブラシ色を元の色に戻す
        brushColor = originalColor;
    }

    /// <summary>
    /// 指定した2点（ワールド座標）の間を直線状に塗る（外部から呼び出し）[cite: 2]
    /// </summary>
    public void PaintLineAtWorldPoint(Vector2 startWorldPos, Vector2 endWorldPos, Color32? customColor = null)
    {
        // 現在のブラシ色を一時保存[cite: 2]
        Color32 originalColor = brushColor;
        // カスタムカラーが指定されていれば変更[cite: 2]
        if (customColor.HasValue)
        {
            brushColor = customColor.Value;
        }

        // 開始位置と終了位置のワールド座標を、それぞれテクスチャのピクセル座標に変換[cite: 2]
        Vector2 startPoint = WorldToTexturePoint(startWorldPos);
        Vector2 endPoint = WorldToTexturePoint(endWorldPos);

        // 2点間に線を引くようにバッファを更新[cite: 2]
        DrawLine(startPoint, endPoint);
        // バッファの内容をテクスチャに反映[cite: 2]
        UpdateTexture();

        // ブラシ色を元に戻す[cite: 2]
        brushColor = originalColor;
    }

    /// <summary>
    /// ワールド座標をテクスチャ上のピクセル座標(X, Y)に変換する[cite: 2]
    /// </summary>
    private Vector2 WorldToTexturePoint(Vector2 worldPos)
    {
        // ワールド座標を、このオブジェクトを基準としたローカル座標に変換[cite: 2]
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // スプライトの実寸サイズ（ワールド空間での大きさ）を計算[cite: 2]
        Vector2 spriteSize = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
        
        // ローカル座標を元に、UV座標（0.0 ~ 1.0の割合）を計算[cite: 2]
        float u = (localPos.x / spriteSize.x) + 0.5f;
        float v = (localPos.y / spriteSize.y) + 0.5f;

        // UV座標とテクスチャの解像度を掛けて、実際のピクセル座標を算出[cite: 2]
        int px = Mathf.Clamp((int)(u * drawTexture.width), 0, drawTexture.width - 1);
        int py = Mathf.Clamp((int)(v * drawTexture.height), 0, drawTexture.height - 1);

        return new Vector2(px, py);
    }

    /// <summary>
    /// ピクセル座標の2点間(pとq)を補間して隙間なく線を引く[cite: 2]
    /// </summary>
    public void DrawLine(Vector2 p, Vector2 q)
    {
        // 2点間のピクセル距離を計算[cite: 2]
        float distance = Vector2.Distance(p, q);
        // 距離を切り上げて、何回のステップで描画するかを決定[cite: 2]
        int lerpNum = Mathf.CeilToInt(distance);

        for (int i = 0; i <= lerpNum; i++)
        {
            float t = lerpNum == 0 ? 0 : (float)i / lerpNum;
            Draw(Vector2.Lerp(p, q, t));
        }
    }

    /// <summary>
    /// デフォルトのブラシサイズで点を塗る[cite: 2]
    /// </summary>
    public void Draw(Vector2 p)
    {
        Draw(p, brushSize);
    }

    /// <summary>
    /// 指定されたピクセル座標 p を中心に、指定サイズ(size)の円を塗る[cite: 2]
    /// </summary>
    public void Draw(Vector2 p, int size)
    {
        int px = (int)p.x;
        int py = (int)p.y;
        int sqrBrushSize = size * size;
        int width = drawTexture.width;
        int height = drawTexture.height;

        int minX = Mathf.Max(0, px - size);
        int maxX = Mathf.Min(width, px + size + 1);
        int minY = Mathf.Max(0, py - size);
        int maxY = Mathf.Min(height, py + size + 1);

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

    /// <summary>
    /// 書き換えたバッファ配列の内容を、実際のテクスチャ画像に適用する[cite: 2]
    /// </summary>
    private void UpdateTexture()
    {
        drawTexture.SetPixels32(buffer);
        drawTexture.Apply(false);
    }
}
