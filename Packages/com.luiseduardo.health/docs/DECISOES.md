# Decisoes de projeto

Registro do que foi escolhido e por que. Serve para eu explicar o trabalho na apresentacao.

Formato: decisao, alternativa descartada, motivo.

---

## D01 Nucleo da vida em C# puro, sem UnityEngine

**Escolha:** a classe `Health` nao tem nenhum `using UnityEngine`.

**Alternativa descartada:** colocar a logica direto num `MonoBehaviour`.

**Motivo:** MonoBehaviour so existe preso a um GameObject numa cena rodando. Logica dentro dele so da para testar entrando em play e olhando com o olho. Com C# puro, os testes rodam em milissegundos no Test Runner sem abrir cena, e a mesma regra serve para jogador, inimigo ou objeto destrutivel.

---

## D02 MonoBehaviour como adaptador, nao como dono da regra

**Escolha:** `HealthComponent` nao decide nada. Ele cria um `Health`, escuta os eventos dele e repassa para o Inspector e para o canal.

**Alternativa descartada:** o componente calcular dano e clamp por conta propria.

**Motivo:** padrao adaptador. A borda do sistema muda com frequencia (UI nova, efeito de particula, som). O miolo nao muda. Separar os dois evita que mexer na apresentacao quebre a regra.

---

## D03 Vida em numero inteiro

**Escolha:** `int`, nao `float`.

**Alternativa descartada:** float com casas decimais.

**Motivo:** float acumula erro de arredondamento, entao comparar `vida == 0` vira armadilha. Com int, morrer e exatamente chegar a zero. Se o jogo precisar de dano fracionado depois, a conversao acontece de fora, arredondando antes de chamar `TakeDamage`.

---

## D04 Caminho unico de escrita

**Escolha:** todo dano, cura, revive e restore passam pelo metodo privado `Apply`.

**Alternativa descartada:** cada metodo publico mexer no campo `current` e disparar seus proprios eventos.

**Motivo:** com varios caminhos de escrita, cedo ou tarde alguem esquece de disparar um evento ou de aplicar o clamp, e a barra de vida dessincroniza do estado real. Funil unico torna esse bug impossivel por construcao.

---

## D05 Tres formas de escutar, para nao apostar numa so

**Escolha:** o modulo publica em evento C# (`Health.Changed`), em UnityEvent do Inspector e num `ScriptableObject` de canal.

**Alternativa descartada:** escolher apenas uma.

**Motivo:** estou integrando as cegas com tres colegas. Evento C# serve para quem tem referencia ao objeto. UnityEvent serve para ligar UI sem escrever codigo. Canal ScriptableObject serve para quem nao conhece nem o objeto nem a cena. Custa pouco manter os tres e cobre qualquer decisao que eles tomem.

---

## D06 Canal de eventos em ScriptableObject, nao em singleton

**Escolha:** `HealthEventChannel` e um asset do projeto. Emissor e ouvinte arrastam o mesmo arquivo.

**Alternativa descartada:** um `GameManager` estatico com `Instance`.

**Motivo:** singleton cria dependencia escondida, ordem de inicializacao fragil e teste dificil. Com asset, a ligacao fica visivel no Inspector, da para ter varios canais (jogador, inimigo, chefe) e nenhum script conhece o outro.

**Cuidado registrado:** ScriptableObject sobrevive ao stop do editor. Por isso o canal limpa os assinantes em `OnDisable`, senao a sessao seguinte herda ouvintes mortos.

---

## D07 Estado de save como struct simples e serializavel

**Escolha:** `HealthState` com campos publicos `current` e `max`.

**Alternativa descartada:** entregar o objeto `Health` inteiro para o sistema de save.

**Motivo:** `JsonUtility` do Unity so enxerga campos publicos, nao propriedades, e nao serializa eventos. Alem disso, quem salva nao pode depender da minha implementacao interna: se eu mudar `Health` depois, o arquivo de save antigo continua valendo porque o contrato e o `HealthState`, nao a classe.

---

## D08 Interfaces como fronteira com os outros modulos

**Escolha:** `IDamageable`, `IHealable`, `IHealthSnapshot`.

**Alternativa descartada:** os colegas chamarem `HealthComponent` direto.

**Motivo:** interface e o contrato minimo. O modulo de inventario compila conhecendo so `IHealable`. Se amanha trocarmos a implementacao de vida, nada do lado dele muda. E o que permite plugar e desplugar.

---

## D09 Duas assemblies, com o nucleo proibido de ver o Unity

**Escolha:** `Game.Health.Core` com `noEngineReferences: true` e lista de referencias vazia, e `Game.Health.Unity` que referencia o nucleo.

**Alternativa descartada:** uma assembly so, ou deixar tudo no `Assembly-CSharp` padrao.

**Motivo:** a flag `noEngineReferences` faz o compilador recusar qualquer `using UnityEngine` dentro do nucleo. A decisao D01 deixa de ser disciplina e vira erro de compilacao. A lista de referencias vazia tem o mesmo efeito na outra direcao: meu modulo nao consegue importar codigo de inventario ou de save nem por acidente. De quebra, mudar um script meu recompila so este pacote, nao o projeto inteiro.

---

## D10 Distribuir como pacote UPM, nao como pasta de projeto

**Escolha:** repositorio com `package.json`, instalado via Install package from disk.

**Alternativa descartada:** um projeto Unity completo com a pasta `Assets`.

**Motivo:** cada um de nos trabalha no seu repositorio e o projeto do grupo puxa os quatro pacotes. Evita conflito de merge em cena e em ProjectSettings, que e a dor classica de Unity em equipe. Tambem me destrava: comeco agora sem depender do que os outros tres fizerem.

---

## D11 Projeto no disco do Windows

**Escolha:** codigo em `C:\Users\Notebook\unity-health-system`.

**Alternativa descartada:** dentro do sistema de arquivos do WSL.

**Motivo:** Unity e aplicativo Windows. Ler pasta do WSL passa por um protocolo de rede e deixa importacao de asset lenta, alem de confundir o detector de mudanca de arquivo do editor. O terminal Linux acessa o mesmo caminho por `/mnt/c` sem custo.

---

## D12 Git LFS e merge de YAML configurados desde o primeiro commit

**Escolha:** `.gitattributes` marcando cena e prefab como YAML e imagem e audio como LFS.

**Alternativa descartada:** configurar depois, quando doer.

**Motivo:** LFS so vale para arquivo commitado a partir do momento em que a regra existe. Se um asset pesado entrar antes, ele fica no historico do git para sempre. Configurar antes do primeiro commit e barato, corrigir depois exige reescrever historico.

---

## D13 Unity 6.3 LTS, versao 6000.3.24f1

**Escolha:** todo o grupo usa `6000.3.24f1`.

**Alternativa descartada:** `6000.6.0f1`, que era o botao padrao do Unity Hub e ja estava instalado.

**Motivo:** 6.3 e Long Term Support, com suporte ate dezembro de 2027 e ecossistema de pacotes verificado. A 6.6 e Tech Stream, e o sufixo `f1` indica a primeira build estavel dela, ou seja, a menos testada. Num trabalho com quatro maquinas diferentes, bug de versao custa mais caro que recurso novo. Detalhe que pesou: projeto Unity sobe de versao sem dor, mas desce quebrado, entao escolher errado agora nao teria volta barata.

---

## D14 Projeto sandbox fora do repositorio

**Escolha:** o projeto Unity de teste vive em `unity-health-sandbox`, pasta IRMA do repositorio, nunca dentro dele.

**Alternativa descartada:** commitar o projeto Unity junto com o modulo.

**Motivo:** o repositorio entrega um pacote, nao um jogo. Commitar um projeto inteiro traria `ProjectSettings` e cenas que iam colidir com o projeto do grupo no merge. O sandbox e descartavel: serve so para eu abrir o editor, rodar os testes e ver a barra de vida mexendo. Quem clonar o repo recria em minutos, ou simplesmente instala o pacote no projeto dele.

**Como o sandbox enxerga o modulo:** `"com.luiseduardo.health": "file:../../unity-health-system"` no `manifest.json`. E um link, nao uma copia: editar o codigo do modulo reflete no editor na hora.

**Erro cometido e corrigido:** na primeira tentativa o sandbox ficou DENTRO da pasta do pacote. O Unity resolveu o pacote e encontrou o proprio projeto la dentro, importando em loop. Regra que fica: pasta de pacote nunca pode conter um projeto Unity.

---

## D15 Template 2D podado: fora inputsystem e collab-proxy

**Escolha:** o sandbox usa o template 2D do editor menos dois pacotes.

**Alternativa descartada:** usar o template como veio.

**Motivo:** o template 2D embutido no editor 6.3 e da linha 6.1 e fixa `com.unity.inputsystem 1.12.0` e `com.unity.collab-proxy 2.6.0`, velhos demais para a API do 6.3. Os dois quebraram a compilacao antes de qualquer teste rodar, com erro dentro do proprio pacote da Unity, nao no meu codigo. Nenhum dos dois faz falta aqui: a demo usa botao de UI, e controle de versao a gente faz por git.

**Aprendizado que vale para o grupo:** template embutido no editor nem sempre acompanha a versao do editor. Se o projeto do grupo der erro de compilacao logo no primeiro open, olhar primeiro para `Library/PackageCache` antes de suspeitar do codigo proprio.

---

## Resultado da primeira execucao

12 testes, 12 verdes, 0,054 segundos, no editor 6000.3.24f1 em modo headless.
Comando usado, que serve tambem para integracao continua depois:

```
Unity.exe -batchmode -nographics -projectPath <sandbox> \
  -runTests -testPlatform EditMode -testResults results.xml -logFile unity.log
```

---

## D16 Cena demo montada por codigo, entregue como Sample do pacote

**Escolha:** um script de editor (`ConstruirCenaDemo`) monta a cena, e o resultado vai para `Samples~/DemoVida`.

**Alternativa descartada:** montar a cena arrastando objeto no editor e commitar o `.unity` direto.

**Motivo, em duas partes.**

Montar por codigo: cena de Unity e um YAML enorme e ilegivel, entao duas pessoas mexendo geram conflito de merge impossivel de resolver na mao. Um script que constroi a cena e codigo normal, com diff revisavel, e nasce identico em qualquer maquina. Se a cena corromper, basta rodar o menu Game, Health, Construir cena demo.

`Samples~` em vez de `Assets`: a pasta com til no fim e ignorada pelo Unity ate alguem importar pelo Package Manager. Ou seja, quem instala o modulo nao carrega a demo junto no projeto de producao, mas pode trazer com um clique quando quiser ver funcionando.

**Detalhe que importa:** os arquivos `.meta` foram gerados com a demo dentro de `Assets` e so depois movidos para `Samples~`, com os `.meta` junto. Sem isso, cada import geraria GUID novo e a cena perderia as referencias dos botoes.

---

## D17 Salvar e carregar saem da demo, mas ficam no modulo

**Escolha:** a cena perdeu os botoes Salvar e Carregar. `Capture`, `Restore` e `IHealthSnapshot` continuam no modulo, intactos.

**Alternativa descartada:** manter a demo mostrando um save funcionando com `PlayerPrefs`.

**Motivo:** salvar e o territorio de outro integrante do grupo. A demo estava implementando um pedaco do trabalho dele, ainda que de mentira, e isso confunde quem olha: parece que o modulo de vida tem responsabilidade de persistencia. A capacidade continua exposta pelo contrato, entao quando o colega chegar ele pluga sem eu mexer em nada.

**Principio por tras:** demonstrar a interface nao exige implementar o consumidor dela.

---

## D18 Coracao gerado por equacao, nao por arquivo de imagem

**Escolha:** o sprite do coracao nasce em tempo de execucao, de uma equacao implicita.

```
(x2 + y2 - 1)3 - x2 * y3 <= 0
```

Cada pixel da textura e testado nessa desigualdade. Dentro da curva, opaco. Fora, transparente.

**Alternativa descartada:** baixar ou desenhar um PNG de coracao e commitar no repositorio.

**Motivo, tres razoes.**

Primeira, repositorio sem asset binario. Imagem no git nao tem diff util, incha o historico e obrigaria configurar Git LFS so por causa de um icone.

Segunda, a forma fica parametrizada. Mudar resolucao, proporcao ou grossura e mudar numero no codigo, nao reabrir editor de imagem.

Terceira, e assunto de computacao grafica: a borda serrilharia se cada pixel fosse so um teste dentro ou fora. O sprite usa **supersampling 4 por 4**, ou seja, 16 amostras por pixel, e o alfa final e a fracao de amostras que caiu dentro da curva. Isso e antialiasing por area de cobertura, o mesmo principio do MSAA.

**Custo:** a textura e gerada uma vez, fica em cache estatico, e o sprite branco e tintado pelo `Image.color`. Um unico sprite serve para coracao cheio, vazio, cinza de morte e flash de dano.

---

## D19 HUD no canto e registro de acoes com hierarquia visual

**Escolha:** vida no canto superior direito, registro no canto inferior esquerdo, botoes embaixo ao centro. Titulo removido.

**Alternativa descartada:** tudo centralizado com um titulo explicando o que e.

**Motivo:** HUD de jogo fica na periferia da tela porque o centro pertence a acao. Titulo escrito na tela e muleta de demo, jogo nenhum tem. Tirar forcou a interface a se explicar sozinha.

**No registro, tres decisoes de leitura:**

Etiqueta colorida por tipo, porque cor e lida antes da palavra. Hora em cada linha, para dar nocao de ritmo entre eventos. E as linhas antigas esmaecendo por opacidade, para o olho cair sempre na mais recente sem precisar procurar.

**Detalhe tecnico:** o painel inteiro e um unico componente de texto com rich text, nao um objeto por linha. Criar e destruir objeto a cada evento geraria lixo de memoria num caminho que roda a cada clique.

**Bug que quase passou:** tag de cor aninhada no Unity nao multiplica alfa, a de dentro sobrescreve a de fora. O esmaecimento so funciona porque o alfa e calculado e escrito em cada pedaco da linha na hora de redesenhar.

---

## D20 Numero exato so no registro, nunca no HUD

**Escolha:** o HUD perdeu o texto `100 / 100`, e os botoes perderam o valor do rotulo. Viraram `Dano`, `Cura`, `Matar`, `Reviver`. Quanto entrou de dano aparece so na linha do registro.

**Alternativa descartada:** mostrar vida atual sobre maxima na tela o tempo todo.

**Motivo:** cada informacao deve aparecer uma vez, no lugar onde ela e util. Durante a acao o jogador precisa saber **quanto sobrou**, e cinco coracoes respondem isso mais rapido que ler dois numeros e dividir. Quanto exatamente entrou de dano e pergunta de depois, e depois e o registro.

Havia redundancia tripla no mesmo instante: o rotulo do botao dizia 10, a barra encolhia, o numero caia para 90 e o log escrevia 10. Quatro maneiras de contar a mesma coisa competindo pela atencao.

**Sobre o rotulo do botao:** valor escrito no botao tambem envelhece mal. No dia em que o dano virar 12, ou variar por arma, o rotulo passa a mentir e ninguem lembra de trocar. Nome de acao nao envelhece.

---

## D21 A versao passa a ser 6000.6.0f1, revogando a D13

**Escolha:** o grupo fica em `6000.6.0f1`.

**O que isso revoga:** a D13, que tinha escolhido a LTS `6000.3.24f1`.

**Motivo:** quando a D13 foi tomada, nao existia projeto do grupo. Existe agora, no repositorio Skenta, e nasceu em 6.6 porque e o botao padrao do Unity Hub. Projeto Unity sobe de versao sem dor e desce quebrado, entao quem tem que ceder somos nos. Insistir na LTS custaria reinstalacao para tres pessoas e um downgrade que o Unity nao faz direito, para ganhar estabilidade que ainda nao nos fez falta.

**O que sobrevive da D13:** o argumento de que a versao tem que ser identica para os quatro. O que mudou foi qual, nao o principio.

**Registro honesto:** a previsao de que o padrao do Hub venceria estava escrita na propria pergunta que originou a D13. Perdemos a aposta para o caminho de menor atrito, que e como quase sempre termina.

---

## D22 O Player.cs vira fachada, com a API intacta

**Escolha:** o `Player.cs` continua expondo `maxHealth`, `TakeDamage`, `Heal`, `GetCurrentHealth`, `LoadHealth` e `OnHealthChanged` com as mesmas assinaturas. Por dentro, todos passaram a chamar o `HealthComponent`.

**Alternativa descartada:** deixar a vida inline dele e somar a nossa ao lado.

**Motivo:** dois sistemas de vida no mesmo jogo e pior que qualquer um dos dois sozinho, porque ninguem sabe qual manda e os dois divergem no primeiro bug. Como a API publica nao mudou, nenhum colega precisa alterar uma linha: quem chamava `Player.TakeDamage` continua chamando, quem lia `GetCurrentHealth` continua lendo.

**O que mudou de comportamento, e precisa ser dito:**

Antes, `Heal` funcionava em jogador morto, porque era so um `Mathf.Clamp` entre zero e o maximo. Agora nao funciona: pelo modulo, curar e diferente de ressuscitar, e para trazer de volta existe `Revive`, que foi adicionado ao `Player`. Isso afeta `UseItem("Potion")` usado com o jogador morto.

E uma mudanca deliberada, coberta por teste (`MortoNaoRecebeCura`), mas e mudanca. Se o grupo quiser que pocao ressuscite, e uma linha no `UseItem`, nao uma mudanca no modulo.

---

## D23 O HUD escuta o EventManager, nunca o jogador

**Escolha:** o `HealthHudBinder` assina `EventManager.OnHealthChanged`. O `Player` publica ali atraves do `HealthComponent`.

**Alternativa descartada:** o HUD procurar o `Player` na cena e assinar o evento dele direto.

**Motivo:** o `EventManager` e a parte do Vinicius no trabalho, e ele ja tinha `OnHealthChanged` e `TriggerHealthChanged` prontos, so que ninguem publicava neles. Passar por ali conecta o sistema dele em vez de furar por fora, e entrega o desacoplamento de verdade: o HUD nao tem referencia ao jogador, nao procura ele na cena e nao sabe que ele existe. Se a vida passar a ser de um chefe, de um veiculo ou de uma porta destrutivel, o HUD nao muda.

**Camadas que isso cria, e que o compilador garante:**

```
Game.Health.Core    regra pura, proibida de ver o Unity
Game.Health.Unity   adaptador MonoBehaviour
Game.Health.View    HUD de coracoes, so desenha, nao conhece regra
Assembly-CSharp     Player, EventManager, HealthHudBinder, codigo do jogo
```

O modulo nao pode importar `EventManager`, porque o asmdef dele tem lista de referencias vazia. Quem faz a ponte e o codigo do jogo, que e o unico lado que pode conhecer os dois. A dependencia aponta para dentro, nunca para fora.

---

## D24 HUD reusavel sai da demo e entra no modulo

**Escolha:** `HealthHud` e `HeartSprite` sairam de `Samples~` e viraram `Runtime/View`, numa terceira assembly, `Game.Health.View`.

**Alternativa descartada:** copiar os dois arquivos para dentro do projeto do grupo.

**Motivo:** copia vira divergencia. Corrigido num lado, quebrado no outro, e ninguem lembra qual e o original. Como o HUD nao conhece nenhuma regra de vida, so recebe numeros e desenha, ele e reusavel de verdade e pertence ao modulo.

**Por que uma assembly separada em vez de juntar na `Game.Health.Unity`:** o HUD precisa de `UnityEngine.UI`. Quem quiser so a regra de vida, sem interface, nao deveria ser obrigado a arrastar uGUI junto. Separar deixa a escolha com quem consome.

---

## D25 Entrega por pull request, nunca commit direto

**Escolha:** a integracao foi para a branch `feat/gerenciamento-de-vida` e vira pull request no repositorio do Vinicius.

**Motivo:** o repositorio e do grupo e o `Player.cs` e arquivo de outra pessoa. Commit direto na main tira dele a chance de revisar uma mudanca que altera comportamento, mesmo que para melhor. O pull request deixa a discussao escrita, e a mudanca do `Heal` em jogador morto precisa ser vista por ele antes de entrar.
