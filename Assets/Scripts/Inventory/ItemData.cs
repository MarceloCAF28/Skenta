using UnityEngine;

// SISTEMA DE INVENTÁRIO

// Dados de um item do jogo.
// Cada item é criado como um asset pelo menu:
// Create > Inventory > Item Data
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    // Nome do item.
    // Deve ser igual ao nome enviado no EventManager.
    // Exemplo: EventManager.TriggerItemCollected("Potion");
    public string itemName;

    // Ícone exibido no slot do inventário.
    public Sprite icon;

    // Texto que descreve o item.
    [TextArea]
    public string description;

    // Define se vários itens iguais podem ocupar o mesmo slot.
    public bool isStackable = true;

    // Quantidade máxima de itens em um único slot.
    // Só é usado quando isStackable for verdadeiro.
    public int maxStack = 99;


    // Chamado pelo Unity quando um valor é alterado no Inspector.
    // Impede que o maxStack fique menor que 1.
    void OnValidate()
    {
        if (maxStack < 1)
        {
            maxStack = 1;
        }
    }
}
