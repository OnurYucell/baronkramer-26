using UnityEngine;

public class SlashAnimationBridge : MonoBehaviour
{
    private PlayerController player;

    void Start()
    {
        // Oyun başladığında, bir üst objedeki (Player) PlayerController scriptini otomatik bulur
        player = GetComponentInParent<PlayerController>();
    }

    // Animasyon Event'inden bu fonksiyonu çağıracağız
    public void TriggerDealDamage()
    {
        if (player != null)
        {
            // Sinyali aldığında ana koddaki hasar verme fonksiyonunu çalıştır
            player.DealDamage();
        }
    }
}