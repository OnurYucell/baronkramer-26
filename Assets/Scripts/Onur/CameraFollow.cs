using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Edilecek Hedef")]
    public Transform target; // Karakterimizin Transform bileşeni

    [Header("Kamera Ayarları")]
    [Range(1f, 10f)]
    public float smoothSpeed = 5f; // Takip yumuşaklığı (Düşük = daha gecikmeli, Yüksek = daha hızlı)

    // Kameranın karakterden ne kadar uzakta duracağı (2D'de Z ekseni kesinlikle eksi bir değerde kalmalı)
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    void LateUpdate()
    {
        // Hedef yoksa hata vermemesi için kontrol et
        if (target == null) return;

        // Kameranın gitmesi gereken ideal hedef pozisyon (Karakterin konumu + bizim belirlediğimiz offset)
        Vector3 desiredPosition = target.position + offset;

        // Kameranın mevcut konumu ile ideal konum arasında yumuşak bir geçiş (Lerp) hesapla
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Hesaplanan yeni konumu kameraya uygula
        transform.position = smoothedPosition;
    }
}