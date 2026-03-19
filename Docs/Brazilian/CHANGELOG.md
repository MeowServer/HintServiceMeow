Clique [aqui](/Docs/Brazilian/README.md) para voltar ao README

# Registro de Alterações

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Não lançado]

---

## [5.5.1]

### Corrigido
- Erro de carregamento de configuração do Exiled causado pela propriedade Instance.

---

## [5.5.0]

### Corrigido
- Um bug que poderia causar o envio de hints pelo `PlayerDisplay` após ser destruído.
- Um bug que causava o `HintCollection` não remover a chave da coleção quando sua `List<Hint>` correspondente estava vazia.
- Um bug que causava o `AbstractHint` não desvincular `AbstractHint::OnContentUpdate` de `AbstractHint::content::ContentUpdated`.
- Um bug que causava atraso desnecessário na atualização de hints.
- Um bug que causava o `AutoContent` continuar chamando a função quando `AutoContent::autoText` continuava lançando exceções.
- Um bug que poderia potencialmente causar `NullReferenceException` em `PlayerDisplay::Destruct()`.
- Bugs que poderiam causar problemas de thread no `TaskScheduler`.
- Um bug em `AbstractHint`, `Hint` e `DynamicHint` que causava deadlock quando múltiplas instâncias no mesmo `PlayerDisplay` eram atualizadas ao mesmo tempo.
- Um bug que causava alguns métodos no CommonHint a usar o tempo de exibição padrão incorreto.

### Alterado
- Definido `CompatibilityAdaptor` como desabilitado por padrão para garantir segurança.
- Movidas as dependências para o repositório do GitHub.
- Melhorado o estilo de código com base nas restrições de estilo de código.
- Melhorado o desempenho de `PlayerDisplay::ScheduleUpdate(float, AbstractHint?)`.
- Substituído `Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1)` por `Hints.AlphaEffect(1)` para melhorar o desempenho.
- Reescrito `DefaultDisplayOutput` e `Patches` para garantir compatibilidade com a nova versão.
- Feitos ajustes menores no `PlayerDisplay` para evitar problemas críticos de estabilidade.
- Melhorado o nome dos testes e o estilo de código em `HintServiceMeow.Test`.
- Substituído `MEC` por `ICoroutine` no `PlayerDisplay` e no `CompatibilityAdaptor` para remover a dependência do Unity.
- Feito `RichTextParserPool` limpar o estado antes de retornar um `RichTextParser`.
- Feito `HintParser` independente do Mirror.
- Melhoria no desempenho do `HintParser`. Alcançou uma execução até 32% mais rápida e reduziu a alocação de memória em até 76%.
- Melhorado `HintServiceExample` para uma demonstração mais detalhada e abrangente.
- Melhorado o desempenho de `HintExtension` e `PlayerDisplayExtension`.

### Adicionado
- `PlayerDisplay::AddHint(params AbstractHint[])`, `RemoveHint(params AbstractHint[])`, `SetMinUpdateInterval(TimeSpan)`, `AddHint(AbstractHint?, string)`, `RemoveHint(AbstractHint?, string)`, `ShowHint(AbstractHint, float, AfterShowAction)` e `ShowHint(IEnumerable<AbstractHint>, float, AfterShowAction)`.
- Restrições obrigatórias de estilo de código ao usar o modo Release. (Obrigado ao @Someone)
- Modelo de relatório de bug, modelo de solicitação de recurso e arquivo de Código de Conduta.
- Cache em `HintCollection::AllGroups` e `HintCollection::AllHints`.
- Log de depuração no `PlayerDisplay`.
- Testes unitários para `PlayerDisplay`, `Cache`, `PeriodicRunner`, `UpdateAnalyzer`, `DynamicHint`, `HintCollection`, `Hint`, `CompatibilityAdaptor`, `HintParser`, `RichTextParser`, `ConcurrentTaskDispatcher`, `CoordinateTools`, `AutoContent`, `StringContent`, `Patcher` (ainda não ativado), `Patches` (ainda não ativado), `RichTextParserTool` e `StringBuilderPool`. Testes relacionados a patches do Harmony não estão ativados pois o Harmony não pode ser executado em ambiente de teste.
- `HintServiceMeow.Benchmark` para medir o desempenho do `HintServiceMeow`.

---

## [5.4.4]

### Corrigido
- Bug causando `PlayerDisplay::CoroutineMethod` não funcionar corretamente

---

## [5.4.3]

### Corrigido
- Bug causando `CoordinateTool::GetTextWidth` lançar exceção ao lidar com strings vazias

---

## [5.4.2]

### Corrigido
- Bug causando o Adaptador de Compatibilidade (CA) não limpar hints corretamente

---

## [5.4.1]

### Alterado
- Centralizadas ações de múltiplos threads para melhorar o desempenho

### Removido
- Dependência YamlDotNet para melhor compatibilidade

---

## [5.4.0]

### Alterado
- **Quebra de compatibilidade:** Parâmetro `AutoText` atualizado

---

## [5.4.0-beta.2]

### Corrigido
- Parte do código usando erroneamente PluginAPI em vez de LabAPI

---

## [5.4.0-beta.1]

### Adicionado
- Suporte para LabAPI

### Removido
- Suporte para NWAPI
- Limitação de frequência de atualização de hints

### Corrigido
- Vários bugs

---

## [5.3.14]

### Corrigido
- `AutoText` continuando a chamar após o `PlayerDisplay` ser destruído

---

## [5.3.13]

### Corrigido
- Vários bugs

---

## [5.3.12]

### Corrigido
- Bug causando `RemoveAfter` no `PlayerDisplay` não funcionar corretamente
- Ajustes menores para evitar bugs

### Removido
- Detecção de overflow (não funcionando corretamente — quebre as linhas manualmente conforme necessário)

---

## [5.3.11]

### Corrigido
- Bug no `PlayerDisplay` causando remoção por ID não funcionar corretamente
- Erro de namespace em `HintContent` — **nota: isso pode quebrar plug-ins usando `HintContents`**

---

## [5.3.10]

### Alterado
- Padronizado o estilo de código
- Re-implementada a API para frameworks de plug-ins
- Ajustes menores de desempenho

### Corrigido
- Bug causando hints ficarem presas na tela

---

## [5.3.9]

### Adicionado
- Suporte para `\n` (texto simples) como quebra de linha

### Corrigido
- Problema no `RichTextParser` causando a tag de quebra de linha `<br>` não funcionar corretamente

---

## [5.3.8]

### Alterado
- Substituído `MultiThreadTool` por `MainThreadDispatcher`

### Corrigido
- Problema no `StringBuilderPool` que poderia causar vazamentos de memória

---

## [5.3.7]

### Adicionado
- Tratamento de erros em `PlayerDisplay.StartParserTask`
- Suporte para tag `<br>` no texto

---

## [5.3.6]

### Adicionado
- Propriedade de tempo de atraso em `TextUpdateArg`

### Alterado
- Melhorada a qualidade do código

### Corrigido
- Problema de thread safety no `PlayerDisplay`
- Vários problemas

---

## [5.3.5]

### Adicionado
- Mais propriedades configuráveis no `PlayerDisplay`

### Alterado
- Melhorada a estabilidade do Adaptador de Compatibilidade
- Melhorado o desempenho
- Melhorias menores na qualidade do código

### Corrigido
- Bug que poderia causar taxa de atualização maior que o esperado
- Vários bugs em extensões
- Problemas de thread safety na Coleção de Hints
- Bug causando crash em sistemas Linux

---

## [5.3.4]

### Corrigido
- Bug no `CompatibilityAdapter` ao passar `Duration` negativo
- Problema de thread safety no `TaskScheduler`
- Problema no `FontTool`

---

## [5.3.3]

### Alterado
- Melhorada a qualidade do código

### Corrigido
- Problema em `Timing.CallDelayed`

---

## [5.3.2]

### Corrigido
- Bug causando `CompatibilityAdapter` não funcionar corretamente
- Bug causando gerenciamento de atualização não funcionar corretamente

---

## [5.3.1]

### Adicionado
- Propriedades `RemoveAfter` e `HideAfter` para `PlayerDisplay` e `AbstractHint`

### Alterado
- Reescrito o código de gerenciamento de atualização no `PlayerDisplay`

---

## [5.3.0]

### Adicionado
- Pool de string builder para melhorar o desempenho

### Alterado
- Uso do .NET 4.8 (em vez de 4.8.1) como versão padrão
- Melhorada a compatibilidade com NW API
- Atualizações menores de nomenclatura

### Corrigido
- Problema causando crash no patch `ReceiveHint`

---

## [5.3.0-pre.2.3]

### Corrigido
- Altura da linha não sendo incluída ao calcular a altura do texto
- `FontTools` não calculando o comprimento dos caracteres corretamente
- `RichTextParser` não tratando quebras de linha corretamente

---

## [5.3.0-pre.2.2]

### Corrigido
- Bug causando altura da linha não ser utilizável
- Atualizações menores e correções de bugs

---

## [5.3.0-pre.2.1]

### Adicionado
- Suporte para tags de estilo `case` e `script`
- Propriedades `margin` no DynamicHint

### Corrigido
- Bug causando o parser de rich text tratar alinhamento incorretamente
- Bug causando o parser de rich text quebrar linhas incorretamente

---

## [5.3.0-pre.2.0]

### Adicionado
- Suporte para tag `size` em hints

### Alterado
- Melhorado o comportamento do Adaptador de Compatibilidade

### Corrigido
- Bug causando crash no servidor ao obter o display do jogador

---

## [5.3.0-pre.1.4]

### Corrigido
- Bug causando DynamicHint ser exibida incorretamente
- Problema de referência nula no `PlayerDisplay`
- Bug causando linhas vazias serem tratadas incorretamente

---

## [5.3.0-pre.1.3]

### Corrigido
- Bug causando hints do Adaptador de Compatibilidade piscarem
- Bug causando hints de múltiplas linhas não serem exibidas corretamente

---

## [5.3.0-pre.1.2]

### Alterado
- Melhorado o comportamento do `HintParser`
- Melhorada a thread safety

---

## [5.3.0-pre.1.1]

### Adicionado
- Suporte para unidade `em` em `line-height` para o Adaptador de Compatibilidade

### Corrigido
- Bug causando a cor do componente `Style` não funcionar
- Bug causando a tag `pos` no Adaptador de Compatibilidade não funcionar

---

## [5.3.0-pre.1.0]

### Adicionado
- Suporte a múltiplos threads para funções principais
- Suporte à tag `pos` no Adaptador de Compatibilidade
- Componente `Style` no PlayerUI

---

## [5.2.5]

### Corrigido
- Cache do Adaptador de Compatibilidade potencialmente causando alto uso de memória

---

## [5.2.4]

### Adicionado
- Suporte para tags `color`, `b`, `i` no Adaptador de Compatibilidade
- Mais métodos para `PlayerDisplay`

---

## [5.2.3]

### Corrigido
- Precisão do Adaptador de Compatibilidade melhorada; problema de tamanho de fonte resolvido

---

## [5.2.2]

### Alterado
- Melhorias de desempenho
- Melhorada a qualidade do código

### Corrigido
- Vários bugs

---

## [5.2.1]

### Corrigido
- Bug causando configuração não ser aplicada para o Adaptador de Compatibilidade

---

## [5.2.0]

### Adicionado
- Adaptador de Compatibilidade

### Alterado
- Melhorias de desempenho

---

## [5.1.2]

### Adicionado
- Propriedade `LineHeight` para todas as hints

### Alterado
- Ajustada a velocidade de sincronização para melhorar o desempenho de exibição

---

## [5.1.1]

### Corrigido
- Bug causando o comprimento do texto ser calculado incorretamente

---

## [5.1.0]

### Adicionado
- Suporte para `\n` no texto

### Alterado
- Melhorado o desempenho do `DynamicHint`

---

## [5.0.2]

### Corrigido
- Vários bugs

---

## [5.0.1]

### Alterado
- Melhorada a experiência na instalação de fontes

### Corrigido
- Bug no arranjo do DynamicHint

---

## [5.0.0]

### Adicionado
- Velocidade de sincronização, texto automático e várias novas propriedades de hint
- Suporte para NW API

### Alterado
- Reescrito o código principal
- Padronizado o estilo de código
- Separados PlayerUI e CommonHint

### Removido
- Modelo de configuração de hints

### Corrigido
- Bug causando o arquivo de fonte ser colocado na pasta TEMP
- Bug impedindo a NW API de carregar o plug-in corretamente

---

## [4.0.0]

### Adicionado
- Classe de configuração para hints
- Evento de atualização no `PlayerDisplay`
- Prioridade de hints
- Hints comuns customizáveis

### Alterado
- Melhorada a qualidade do código

---

## [3.3.0]

### Alterado
- Separado `PlayerUITemplate` de `PlayerUIConfig` em um novo plug-in: `CustomizableUIMeow`

---

## [3.2.0]

### Alterado
- Organizada a configuração
- Tornado `PlayerUIConfig` mais customizável

---

## [3.1.2]

### Alterado
- Usado patch para bloquear todas as hints de outros plug-ins

---

## [3.1.1]

### Corrigido
- Vários bugs

---

## [3.1.0]

### Adicionado
- Configuração `PlayerUIConfig`

---

## [3.0.2]

### Corrigido
- Bug causando `PlayerDisplay` travar quando nenhuma hint está sendo exibida na tela

---

## [3.0.1]

### Corrigido
- Vários bugs

---

## [3.0.0]

### Alterado
- `ReferenceHub UI` separado do `PlayerDisplay` e estendido com mais métodos

---

## [2.2.0]

### Alterado
- Usado eventos para atualizar o display do `ReferenceHub`, aumentando a estabilidade e diminuindo os custos

---

## [2.1.1]

### Corrigido
- Vários bugs

---

## [2.1.0]

### Adicionado
- Hints Comuns

---

## [2.0.0]

### Adicionado
- Suporte a DynamicHint
- Limite máximo de taxa de atualização (0,5/segundo)

### Corrigido
- Vários bugs

---

## [1.0.1]

### Alterado
- Atualizada a exibição com base nas atualizações de conteúdo das hints

---

## [1.0.0]

### Adicionado
- Lançamento inicial
