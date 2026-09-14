# Player Health

Modulo de gerenciamento de vida do jogador para Unity. Trabalho de Computacao Grafica.

Distribuido como pacote Unity (UPM), entao entra e sai de qualquer projeto sem arrastar codigo solto.

## Ideia em uma frase

A regra de vida vive num objeto C# puro. O Unity so aparece na borda, como adaptador. Quem quiser saber de vida escuta um evento, nunca chama meu codigo direto.

## Estrutura

```
Runtime/Core            assembly Game.Health.Core, proibida de usar UnityEngine
Runtime/Core/Contracts  interfaces que os outros modulos usam
Runtime/Unity           assembly Game.Health.Unity, adaptador MonoBehaviour e canal
Tests/EditMode          testes NUnit do nucleo
docs/                   decisoes de projeto e guia de integracao
```

## Instalar

1. Abra o projeto Unity do grupo.
2. Window, Package Manager, botao mais, Install package from disk.
3. Aponte para o `package.json` deste repositorio.

## Usar

Ponha `HealthComponent` no jogador. Configure vida maxima no Inspector.

Causar dano de qualquer lugar:

```csharp
if (alvo.TryGetComponent(out IDamageable vida))
    vida.TakeDamage(10);
```

Ligar barra de vida sem escrever codigo: arraste o `fillAmount` da Image no evento `onNormalizedChanged` do Inspector.

## Integracao com os outros modulos

Ver `docs/INTEGRACAO.md`. Resumo:

| Modulo | Ponto de contato |
| --- | --- |
| Eventos | `HealthEventChannel` ou os eventos C# em `Health` |
| Save | `IHealthSnapshot` com `Capture` e `Restore` |
| Inventario | `IHealable` para pocao, `SetMaxHealth` para item de vida maxima |

## Ver funcionando

Package Manager, selecione Player Health, aba Samples, Import em "Demo de vida".
Abra `DemoVida.unity` e de Play. Botoes de dano, cura, matar, reviver, salvar e carregar.

Para remontar a cena do zero: menu Game, Health, Construir cena demo.

## Testes

Window, General, Test Runner, aba EditMode, Run All.

Headless, o mesmo comando que serve para integracao continua:

```
Unity.exe -batchmode -nographics -projectPath <projeto> \
  -runTests -testPlatform EditMode -testResults results.xml -logFile unity.log
```

Ultima execucao: 12 testes, 12 verdes, no 6000.3.24f1.

## Decisoes

Cada escolha esta registrada com o porque em `docs/DECISOES.md`.
