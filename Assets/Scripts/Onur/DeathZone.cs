using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yenilemek için şart!

public class DeathZone : MonoBehaviour
{
    // Bir obje bu görünmez alana (Trigger) girdiğinde çalışır
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Eğer giren obje "Player" etiketine sahipse
        if (hitInfo.CompareTag("Player"))
        {
            // Mevcut sahneyi anında baştan başlat
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}