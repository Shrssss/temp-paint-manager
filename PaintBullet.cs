using UnityEngine;

public class PaintBullet : MonoBehaviour
{
    [SerializeField] private Color32 bulletColor = new Color32(255, 0, 0, 255);
    [SerializeField] private LayerMask groundLayer; // 塗りたい地面のレイヤーを指定

    private Vector2 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;

        // 移動前後の2点間で地面（コライダー）との接触を判定
        RaycastHit2D hit = Physics2D.Linecast(lastPosition, currentPosition, groundLayer);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<SpritePaintManager>(out var paintManager))
            {
                paintManager.PaintLineAtWorldPoint(lastPosition, hit.point, bulletColor);
            }
        }
        else
        {
            // Top-Down視点などで弾が地面の上を通過している場合
            RaycastHit2D overlapHit = Physics2D.Raycast(currentPosition, Vector2.zero, 0f, groundLayer);
            if (overlapHit.collider != null && overlapHit.collider.TryGetComponent<SpritePaintManager>(out var paintManager))
            {
                paintManager.PaintLineAtWorldPoint(lastPosition, currentPosition, bulletColor);
            }
        }

        lastPosition = currentPosition;
    }
}
