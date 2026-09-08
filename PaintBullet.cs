using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PaintBullet : MonoBehaviour
{
    [SerializeField] private Color32 bulletColor = new Color32(255, 0, 0, 255);

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突したオブジェクトに SpritePaintManager がついているか判定
        if (collision.gameObject.TryGetComponent<SpritePaintManager>(out var paintManager))
        {
            // 最初の接触点（ワールド座標）を取得して塗る
            Vector2 hitPoint = collision.GetContact(0).point;
            paintManager.PaintAtWorldPoint(hitPoint, bulletColor);
        }

        // 地面や壁に着弾したら弾を破棄
        Destroy(gameObject);
    }
}