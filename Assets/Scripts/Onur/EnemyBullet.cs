using UnityEngine;
using UnityEngine.SceneManagement; // Sahneyi yeniden yüklemek için bu kütüphane ŞART!

public class EnemyBullet : MonoBehaviour
{
    void Start()
    {
        // Mermi hiçbir şeye çarpmazsa 4 saniye sonra kendi kendini yok etsin
        Destroy(gameObject, 4f);
    }

    // Trigger sistemine geri dönüyoruz (Merminin Is Trigger'ı TİKLİ olmalı)
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. Mermi OYUNCUYA çarparsa: Sahneyi baştan başlat
        if (hitInfo.CompareTag("Player"))
        {
            // GetActiveScene().buildIndex -> Şu an oynadığımız sahnenin numarasını bulur ve onu tekrar yükler
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        // 2. Mermi ZEMİNE çarparsa: Sadece mermiyi yok et
        else if (hitInfo.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        // 3. Düşmana veya önemsiz şeylere çarpmıyorsa yine de yok et
        else if (!hitInfo.CompareTag("Enemy") && !hitInfo.CompareTag("Untagged"))
        {
            Destroy(gameObject);
        }
    }
}