using UnityEngine;

// SISTEMA DE INVENTÁRIO

// Item colecionável colocado no mundo do jogo.
// Quando o jogador encosta nele, avisa o EventManager
// que o item foi coletado e some da cena.
// Precisa de um Collider2D com "Is Trigger" marcado.
public class ItemPickup : MonoBehaviour
{
    // Nome do item que este pickup representa.
    // Deve ser igual ao itemName de um ItemData na ItemDatabase.
    // Exemplo: "Potion"
    public string itemName;

    // Impede que o item seja coletado duas vezes
    // antes de o objeto ser destruído.
    private bool collected;


    void Start()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();

        // Avisa se o objeto não estiver configurado para detectar o jogador.
        if (pickupCollider == null || !pickupCollider.isTrigger)
        {
            Debug.LogWarning(
                $"ItemPickup: \"{name}\" precisa de um Collider2D com Is Trigger marcado.",
                this
            );
        }
    }


    // Chamado pelo Unity quando outro Collider2D entra no trigger.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;

        // Avisa o inventário que o item foi coletado.
        EventManager.TriggerItemCollected(itemName);

        // Remove o item da cena.
        Destroy(gameObject);
    }
}
