using System.Collections.Generic;
using UnityEngine;

// SISTEMA DE INVENTÁRIO

// Lista com todos os itens que existem no jogo.
// O Inventory usa essa lista para descobrir qual ItemData
// corresponde ao nome recebido pelo EventManager.
// Criado pelo menu: Create > Inventory > Item Database
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    // Todos os itens cadastrados.
    public List<ItemData> items = new List<ItemData>();


    // Procura um item pelo nome.
    // Exemplo: GetItemByName("Potion");
    // Retorna null se o item não estiver cadastrado.
    public ItemData GetItemByName(string name)
    {
        foreach (ItemData item in items)
        {
            // Ignora posições vazias da lista no Inspector.
            if (item != null && item.itemName == name)
            {
                return item;
            }
        }

        Debug.LogWarning(
            $"ItemDatabase: item \"{name}\" não encontrado.",
            this
        );

        return null;
    }
}
