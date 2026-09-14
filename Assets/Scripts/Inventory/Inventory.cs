using System;
using System.Collections.Generic;
using UnityEngine;

// SISTEMA DE INVENTÁRIO

// Um espaço do inventário.
// Guarda qual item está no slot e quantas unidades ele tem.
[Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}


// Guarda os itens coletados pelo jogador.
// Escuta o EventManager.OnItemCollected e adiciona o item
// correspondente, respeitando isStackable e maxStack.
public class Inventory : MonoBehaviour
{
    // Database usada para encontrar o ItemData pelo nome.
    public ItemDatabase itemDatabase;

    // Quantidade máxima de slots do inventário.
    public int maxSlots = 20;

    // Slots ocupados. Nunca passa de maxSlots.
    public List<InventorySlot> slots = new List<InventorySlot>();

    // Evento que avisa a UI quando o inventário muda.
    // Envia: a lista de slots atualizada.
    public event Action<List<InventorySlot>> OnInventoryChanged;


    // Começa a escutar o evento de item coletado.
    void OnEnable()
    {
        EventManager.OnItemCollected += HandleItemCollected;
    }


    // Para de escutar o evento. Como o EventManager é estático,
    // sem isso ele continuaria chamando um objeto destruído.
    void OnDisable()
    {
        EventManager.OnItemCollected -= HandleItemCollected;
    }


    void Start()
    {
        // Avisa a UI do estado inicial do inventário.
        OnInventoryChanged?.Invoke(slots);
    }


    // Chamado quando o EventManager avisa que um item foi coletado.
    void HandleItemCollected(string itemName)
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("Inventory: nenhuma ItemDatabase foi definida.", this);
            return;
        }

        ItemData item = itemDatabase.GetItemByName(itemName);

        // A própria database já mostra o aviso de item não encontrado.
        if (item == null)
        {
            return;
        }

        AddItem(item, 1);
    }


    // Adiciona uma quantidade de um item ao inventário.
    // Primeiro completa os slots que já têm o item,
    // depois ocupa slots novos.
    // Retorna false se o inventário encheu e sobrou item.
    public bool AddItem(ItemData item, int amount)
    {
        int stackLimit = GetStackLimit(item);
        int remaining = amount;

        // Completa os slots existentes do mesmo item.
        if (item.isStackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.item == item && slot.quantity < stackLimit)
                {
                    int added = Mathf.Min(stackLimit - slot.quantity, remaining);

                    slot.quantity += added;
                    remaining -= added;
                }
            }
        }

        // Ocupa slots vazios com o que sobrou.
        while (remaining > 0 && slots.Count < maxSlots)
        {
            int added = Mathf.Min(stackLimit, remaining);

            slots.Add(new InventorySlot(item, added));
            remaining -= added;
        }

        // Só avisa a UI se algo foi realmente adicionado.
        if (remaining < amount)
        {
            OnInventoryChanged?.Invoke(slots);
        }

        if (remaining > 0)
        {
            Debug.LogWarning(
                $"Inventory: inventário cheio, {remaining}x \"{item.itemName}\" não coube.",
                this
            );

            return false;
        }

        return true;
    }


    // Retorna quantas unidades do item cabem em um slot.
    // Itens não empilháveis ocupam um slot por unidade.
    int GetStackLimit(ItemData item)
    {
        if (!item.isStackable)
        {
            return 1;
        }

        return Mathf.Max(1, item.maxStack);
    }
}
