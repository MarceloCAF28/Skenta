using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// SISTEMA DE INVENTÁRIO

// Mostra o inventário em uma grade de slots no Canvas.
// Escuta o Inventory.OnInventoryChanged e atualiza
// o ícone e a quantidade de cada slot.
public class InventoryUI : MonoBehaviour
{
    // Inventário que será exibido.
    public Inventory inventory;

    // Objeto dentro do Canvas onde os slots serão criados.
    // Se ficar vazio, usa o próprio objeto deste script.
    public Transform slotContainer;

    // Prefab opcional de slot.
    // Precisa ter um filho "Icon" (Image) e um filho "Quantity" (Text).
    // Se ficar vazio, os slots são criados por código.
    public GameObject slotPrefab;

    // Tamanho de cada slot e espaço entre eles na grade.
    public Vector2 slotSize = new Vector2(64f, 64f);
    public float slotSpacing = 4f;

    // Fonte do texto de quantidade.
    // Se ficar vazia, usa a fonte padrão do Unity.
    public Font font;

    // Componentes de cada slot criado, na mesma ordem da grade.
    private List<Image> slotIcons = new List<Image>();
    private List<Text> slotQuantities = new List<Text>();


    // Começa a escutar as mudanças do inventário.
    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateSlots;
        }
    }


    // Para de escutar as mudanças do inventário.
    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateSlots;
        }
    }


    void Start()
    {
        if (inventory == null)
        {
            Debug.LogWarning("InventoryUI: nenhum Inventory foi definido.", this);
            return;
        }

        // Desenha o estado atual, caso o Inventory
        // tenha avisado antes deste Start rodar.
        UpdateSlots(inventory.slots);
    }


    // Atualiza todos os slots da grade com os itens do inventário.
    void UpdateSlots(List<InventorySlot> slots)
    {
        // Cria a grade na primeira atualização.
        CreateSlots();

        for (int i = 0; i < slotIcons.Count; i++)
        {
            Image icon = slotIcons[i];
            Text quantityText = slotQuantities[i];

            bool hasItem = i < slots.Count && slots[i].item != null;

            if (icon != null)
            {
                icon.sprite = hasItem ? slots[i].item.icon : null;
                icon.enabled = icon.sprite != null;
            }

            if (quantityText != null)
            {
                // Só mostra o número quando há mais de uma unidade.
                quantityText.text = hasItem && slots[i].quantity > 1
                    ? slots[i].quantity.ToString()
                    : "";
            }
        }
    }


    // Cria um slot visual para cada espaço do inventário.
    // Não faz nada se a grade já foi criada.
    void CreateSlots()
    {
        if (slotIcons.Count > 0)
        {
            return;
        }

        if (slotContainer == null)
        {
            slotContainer = transform;
        }

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // Organiza os slots em grade automaticamente.
        GridLayoutGroup grid = slotContainer.GetComponent<GridLayoutGroup>();

        if (grid == null)
        {
            grid = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = slotSize;
            grid.spacing = new Vector2(slotSpacing, slotSpacing);
        }

        for (int i = 0; i < inventory.maxSlots; i++)
        {
            GameObject slot;

            if (slotPrefab != null)
            {
                slot = Instantiate(slotPrefab, slotContainer);
            }
            else
            {
                slot = CreateDefaultSlot();
            }

            slot.name = "Slot " + i;

            Transform icon = slot.transform.Find("Icon");
            Transform quantity = slot.transform.Find("Quantity");

            if (icon == null || quantity == null)
            {
                Debug.LogWarning(
                    "InventoryUI: o prefab de slot precisa ter os filhos \"Icon\" e \"Quantity\".",
                    slotPrefab
                );
            }

            slotIcons.Add(icon != null ? icon.GetComponent<Image>() : null);
            slotQuantities.Add(quantity != null ? quantity.GetComponent<Text>() : null);
        }
    }


    // Cria um slot simples por código:
    // fundo escuro, ícone no centro e quantidade no canto.
    GameObject CreateDefaultSlot()
    {
        // Fundo do slot.
        GameObject slot = new GameObject("Slot", typeof(RectTransform), typeof(Image));
        slot.transform.SetParent(slotContainer, false);
        slot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

        // Ícone do item.
        GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(slot.transform, false);
        StretchToParent(icon.GetComponent<RectTransform>(), 6f);

        Image iconImage = icon.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.enabled = false;

        // Texto da quantidade.
        GameObject quantity = new GameObject("Quantity", typeof(RectTransform), typeof(Text));
        quantity.transform.SetParent(slot.transform, false);
        StretchToParent(quantity.GetComponent<RectTransform>(), 4f);

        Text quantityText = quantity.GetComponent<Text>();
        quantityText.font = font;
        quantityText.fontSize = 16;
        quantityText.color = Color.white;
        quantityText.alignment = TextAnchor.LowerRight;
        quantityText.raycastTarget = false;
        quantityText.text = "";

        return slot;
    }


    // Faz o elemento ocupar todo o pai, com uma margem.
    void StretchToParent(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }
}
