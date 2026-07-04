using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    void Start()
    {
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Eğer fırlatan kişiye (Player) çarpıyorsa hiçbir şey yapma, devam et
        if (hitInfo.CompareTag("Player")) return;

        // İleride buraya düşmana hasar verme kodu gelecek

        // Duvara veya başka bir şeye çarptığında bıçağı yok et
        Destroy(gameObject);
    }
}