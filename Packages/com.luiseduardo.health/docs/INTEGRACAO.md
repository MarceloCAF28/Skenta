# Guia de integracao

Pagina unica para os outros modulos. Ninguem precisa ler o codigo do modulo de vida.

## Como esta montado no projeto Skenta

```
Player.cs  ---dispara--->  EventManager  ---avisa--->  HealthHudBinder ---> HUD
   |                       (barramento do projeto)
   +--- delega a regra ---> HealthComponent  (pacote Game.Health)
```

O `Player.cs` continua sendo a porta de entrada de tudo que envolve vida. Ele nao calcula mais nada: repassa para o `HealthComponent` e publica o resultado no `EventManager`.

O HUD nao conhece o `Player`. Ele so escuta o barramento.

---

## Para o modulo de Salvamento e Carregamento

Continue usando o `Player`, nada mudou de assinatura:

```csharp
// salvar
int vida = jogador.GetCurrentHealth();

// carregar
jogador.LoadHealth(vidaSalva);
```

`LoadHealth` ja dispara o evento de mudanca, entao o HUD se atualiza sozinho depois do load. Voce nao precisa avisar a interface.

Se preferir salvar vida atual e maxima juntas, existe o contrato do modulo:

```csharp
var vida = jogador.GetComponent<IHealthSnapshot>();
HealthState estado = vida.Capture();          // dois ints: current e max
string json = JsonUtility.ToJson(estado);

vida.Restore(JsonUtility.FromJson<HealthState>(json));
```

O `EventManager` ja tem `OnSaveRequested` e `OnLoadRequested` prontos para voce assinar.

---

## Para o modulo de Inventario com UI

Pocao de cura, do jeito que ja estava:

```csharp
jogador.UseItem("Potion");   // por dentro chama Heal(20)
```

Ou direto, sem conhecer o `Player`:

```csharp
if (usuario.TryGetComponent(out IHealable curavel))
    curavel.Heal(quantidade);
```

Item que aumenta vida maxima:

```csharp
jogador.GetComponent<HealthComponent>().SetMaxHealth(150, keepRatio: true);
```

`keepRatio` true mantem a proporcao, ou seja, quem estava com metade continua com metade.

**Atencao, mudanca de comportamento:** curar jogador morto nao faz mais nada. Curar e ressuscitar viraram intencoes diferentes. Para trazer de volta use `jogador.Revive()`. Se o grupo decidir que pocao ressuscita, e uma linha no `UseItem`.

---

## Para quem quiser reagir a vida sem conhecer o jogador

```csharp
void OnEnable()  => EventManager.OnHealthChanged += AoMudarVida;
void OnDisable() => EventManager.OnHealthChanged -= AoMudarVida;

void AoMudarVida(int atual, int max) { /* ... */ }
```

Evento estatico exige assinar e cancelar em par, senao a assinatura sobrevive ao stop do editor e a sessao seguinte herda ouvinte morto.

---

## Causar dano de qualquer lugar

Armadilha, inimigo, queda. Nao precisa conhecer o `Player`:

```csharp
if (alvo.TryGetComponent(out IDamageable vida))
    vida.TakeDamage(10);
```

---

## O que o modulo garante

- Vida nunca fica negativa nem passa do maximo.
- Morte dispara uma unica vez por morte, nao a cada dano depois dela.
- Morto nao recebe cura. Para voltar, `Revive`.
- Todo caminho de alteracao avisa a interface, inclusive o carregamento do save.

## O que o modulo nao faz

Nao tem invencibilidade temporaria depois do dano, regeneracao ao longo do tempo nem tipos de dano. Se algum modulo precisar, e extensao, nao remendo.
