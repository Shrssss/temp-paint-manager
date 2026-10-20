using UnityEngine;

public class PaintBullet : MonoBehaviour
{
    [Header("Color & Layer")]
    // 弾が塗る色を指定
    [SerializeField] private Color32 bulletColor = new Color32(255, 0, 0, 255);
    // 塗る対象（地面などのキャンバス）を判定するための変数
    [SerializeField] private LayerMask groundLayer;

    [Header("Trail Settings (軌跡の塗り)")]
    // 移動中に確率で落とすインク（飛沫）のサイズ
    [SerializeField] private int smallBrushSize = 4;
    // インクを落とす確率 (0.0=絶対落とさない 〜 1.0=必ず落とす)
    [SerializeField, Range(0f, 1f)] private float trailDropChance = 0.35f; 
    // どれくらい移動したら「インクを落とすかどうかの確率判定」を行うかという距離（ワールド単位）
    [SerializeField] private float stepDistance = 0.25f; 

    [Header("Impact Settings (破棄時の塗り)")]
    // 弾が消滅（破棄）した際にその場に塗る大きなインクのサイズ
    [SerializeField] private int largeBrushSize = 25;

    // 前回の判定時の弾の座標を保持する変数
    private Vector2 lastPosition;
    
    // ゲーム終了時やシーン遷移時にOnDestroyが呼ばれた際、無駄な処理やエラーを防ぐための変数
    private static bool isApplicationQuitting = false;

    void Start()
    {
        // 生成された時の初期位置をlastPositionに保存
        lastPosition = transform.position;
    }

    void Update()
    {
        // 現在のフレームでの弾の座標を取得
        Vector2 currentPosition = transform.position;
        // 前回判定した位置から、現在の位置までの移動距離を計算
        float distance = Vector2.Distance(lastPosition, currentPosition);

        // 移動距離が、指定したステップ間隔(stepDistance)以上になっていれば判定処理を行う
        if (distance >= stepDistance)
        {
            // 今回の移動距離の中に、stepDistanceがいくつ含まれているか（何ステップ分移動したか）を計算（切り捨て）
            // 弾速が非常に速く、1フレームで一気に進んだ場合でも間がスカスカにならないようにするため
            int steps = Mathf.FloorToInt(distance / stepDistance);

            // ステップ数分だけループを回して判定
            for (int i = 1; i <= steps; i++)
            {
                // Random.value (0.0~1.0のランダム値) が 設定した確率以下の時だけインクを落とす
                if (Random.value <= trailDropChance)
                {
                    // 前回位置から現在位置までの間のどの位置にインクを落とすか、割合で座標を計算 (補間)
                    Vector2 samplePos = Vector2.Lerp(lastPosition, currentPosition, (float)i / steps);
                    // 計算した位置に、小さいサイズの塗りを実行する
                    PaintAtPosition(samplePos, smallBrushSize);
                }
            }

            // 次の判定の基準にするため、lastPositionを現在の座標に更新
            lastPosition = currentPosition;
        }
    }

    // このオブジェクト（弾）が破棄される瞬間に呼ばれるUnityの標準イベント関数
    private void OnDestroy()
    {
        // ゲーム終了時、またはシーン自体がアンロードされている最中の場合は処理しない
        // (これをしないと、ゲーム終了時に既に破棄されたオブジェクトにアクセスしようとしてエラーが出る場合がある)
        if (isApplicationQuitting || !gameObject.scene.isLoaded) return;

        // 弾が消滅する現在位置に対して、大きいサイズの塗りを実行する
        PaintAtPosition(transform.position, largeBrushSize);
    }

    // ゲーム終了時（エディタのプレイ停止時含む）に呼ばれるUnityの標準イベント関数
    private void OnApplicationQuit()
    {
        // ゲーム終了中であることをフラグとして記録する
        isApplicationQuitting = true;
    }

    /// <summary>
    /// 指定されたワールド座標に地面があるか判定し、存在すれば指定サイズで色を塗る
    /// </summary>
    private void PaintAtPosition(Vector2 worldPos, int size)
    {
        // 指定した座標に、指定したレイヤー(groundLayer)を持つコライダー(2D)が重なっているかチェック
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, groundLayer);

        // コライダーが存在し、かつそのオブジェクトが SpritePaintManager を持っているか確認
        if (hitCollider != null && hitCollider.TryGetComponent<SpritePaintManager>(out var paintManager))
        {
            // 持っていれば、SpritePaintManagerの円塗り関数を呼び出して塗りを実行する
            paintManager.PaintCircleAtWorldPoint(worldPos, size, bulletColor);
        }
    }
}
