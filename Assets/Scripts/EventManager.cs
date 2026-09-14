using System;

// Classe responsável por centralizar os eventos do jogo.
public static class EventManager
{
    // SISTEMA DE VIDA

    // Evento chamado quando a vida do jogador muda.
    // Recebe:
    // int current = vida atual
    // int max = vida máxima
    public static event Action<int, int> OnHealthChanged;


    // SISTEMA DE INVENTÁRIO
    
    // Evento chamado quando um item é coletado.
    // Recebe o nome do item coletado.
    public static event Action<string> OnItemCollected;


    // SISTEMA DE COMBATE

    // Evento chamado quando um inimigo é derrotado.
    // Recebe o nome do inimigo.
    public static event Action<string> OnEnemyDefeated;


    // SISTEMA DE SALVAMENTO

    // Evento chamado quando algum sistema solicita
    // que o jogo seja salvo.
    public static event Action OnSaveRequested;

    // Evento chamado quando algum sistema solicita
    // que o jogo seja carregado.
    public static event Action OnLoadRequested;


    // Dispara o evento de alteração da vida.
    // Exemplo: TriggerHealthChanged(50, 100);
    public static void TriggerHealthChanged(int current, int max)
        => OnHealthChanged?.Invoke(current, max);


    // Dispara o evento de item coletado.
    // Exemplo: TriggerItemCollected("Poção");
    public static void TriggerItemCollected(string item)
        => OnItemCollected?.Invoke(item);


    // Dispara o evento de inimigo derrotado.
    // Exemplo: TriggerEnemyDefeated("Slime");
    public static void TriggerEnemyDefeated(string inimigo)
        => OnEnemyDefeated?.Invoke(inimigo);


    // Dispara o evento de solicitação de salvamento.
    public static void TriggerSaveRequested()
        => OnSaveRequested?.Invoke();


    // Dispara o evento de solicitação de carregamento.
    public static void TriggerLoadRequested()
        => OnLoadRequested?.Invoke();
}