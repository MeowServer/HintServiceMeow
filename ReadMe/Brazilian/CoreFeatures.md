# Documentação
Essa documentação introduz o uso e recursos do HintServiceMeow.
## Hint
Hint (dica) é um recurso primário que permite que você adicione texto para uma posição específica na tela de um jogador. O exemplo a seguir adiciona texto na parte esquerda inferior da tela do jogador.
```Csharp
 Hint hint = new Hint
 {
     Text = "Hello World",
     FontSize = 40,
     YCoordinate = 700,
     Alignment = HintAlignment.Left
 };

 PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
 playerDisplay.AddHint(hint);
```
Já que o HSM tem uma mecânica de atualização automática, quaisquer mudanças a uma propriedade (como Text, FontSize, Alignment, etc.) serão automaticamente refletidas na tela do jogador sem nenhumas outras chamadas de método.
```Csharp
hint.Text = "Algum Texto Novo";
// Você não precisa chamar qualquer método após atualizar propriedades
``` 
#### Propriedades
| Propriedades | Descrição |
| - | - |
| Guid (Somente leitura) | Um Guid gerado para a dica |
| Id | Um ID de string personalizado |
| SyncSpeed | A prioridade de atualização da dica. Quanto mais rápido for, mais rápido ela será atualizada depois do conteúdo ser atualizado |
| FontSize | O tamanho do texto |
| LineHeight | O espaço extra entre cada linha de texto |
| Content | O conteúdo da dica. O texto exibido será obtido dessa propriedade. |
| Text | Definir essa propriedade irá sobrescrever o Content com texto estático. |
| AutoText | Definir essa propriedade irá sobrescrever o Content com um delegado de string. |
| Hide | Se deve ocultar essa dica ou não |
| XCoordinate | A posição horizontal do texto. Quanto maior a coordenada X for, mais à direita o texto será exibido |
| YCoordinate | A posição vertical do texto. Quanto maior a coordenada Y for, mais abaixo o texto será exibido ![A posição da coordenada Y](Images/YCoordinateExample.jpg) |
| Alignment | O alinhamento do texto. |
| YCoordinateAlign | O alinhamento do texto para a coordenada y. Por exemplo, Top significa que a coordenada y representa o topo do texto |
## DynamicHint
Dicas dinâmicas permitem que você exiba texto sem especificar uma posição fixa. Dicas dinâmicas são automaticamente posicionadas na tela em áreas onde elas não sobreporão a outros elementos de texto, assegurando legibilidade máxima. O exemplo a seguir adiciona texto à tela do jogador sem definir uma posição fixa.
```CSharp
var dynamicHint = new DynamicHint
{
    Text = "Olá, Dica Dinâmica"
};

PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(dynamicHint);
```
#### Propriedades
| Propriedades | Descrição |
| - | - |
| Guid (Somente leitura) | Um Guid gerado para a dica |
| Id | Um ID de string personalizado |
| SyncSpeed | A prioridade de atualização da dica. Quanto mais rápido for, mais rápido ela será atualizada depois do conteúdo ser atualizado |
| FontSize | O tamanho do texto |
| LineHeight | O espaço extra entre cada linha de texto |
| Content | O conteúdo da dica. O texto exibido será obtido dessa propriedade. |
| Text | Definir essa propriedade irá sobrescrever o Content com texto estático. |
| AutoText | Definir essa propriedade irá sobrescrever o Content com um delegado de string. |
| Hide | Se deve ocultar essa dica ou não |
| TopBoundary, BottomBoundary, LeftBoundary, RightBoundary | O limite para dica dinâmica organizar seu texto |
| TargetX, TargetY | A dica dinâmica tentará mover em direção a essas coordenadas, mas o local final depende do espaço disponível. |
| TopMargin, BottomMargin, LeftMargin, RightMargin | O espaço extra que a dica tentará adicionar aos seus arredores ao organizar |
| Priority | A propriedade da dica dinâmica. Quanto maior a propriedade, mais cedo ela será organizada |
| Strategy | A dica dinâmica de estratégia será usada ao organizar |
## CommonHint
CommonHint é um componente que permite que você exiba texto em uma posição predefinida. O exemplo a seguir usa CommonHint para exibir múltiplas mensagens ao jogador.
```CSharp
var ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP173", new[] { "Mate todos humanos", "Use suas habilidades" });
ui.CommonHint.ShowMapHint("Zona de Contenção Pesada", "O lugar que a maioria dos SCPs nascem");
ui.CommonHint.ShowItemHint("Cartão", "Usado para abrir portas");
ui.CommonHint.ShowOtherHint("O servidor está iniciando!");
```
