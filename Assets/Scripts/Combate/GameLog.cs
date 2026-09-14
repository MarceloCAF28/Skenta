using System;
using Game.Health;

/// <summary>
/// Canal simples de mensagens para o painel de registro.
/// Existe para que qualquer sistema escreva uma linha no log sem precisar
/// achar o painel na cena nem guardar referencia para ele.
/// </summary>
public static class GameLog
{
    public static event Action<ActionLog.Tipo, string> AoEscrever;

    public static void Escrever(ActionLog.Tipo tipo, string mensagem)
        => AoEscrever?.Invoke(tipo, mensagem);
}
