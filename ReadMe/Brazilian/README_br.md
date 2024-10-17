## Introdução
HintServiceMeow(HSM) é um framework que permite que plug-ins exibam texto em uma posição selecionada na tela de um jogador.

## Instalação
Para instalar esse plug-in, por favor
1. Vá à página de lançamento e baixe o HintServiceMeow.dll mais recente. Então, cole-o na pasta de plug-in.
2. Se você estiver usando a PluginAPI (A API padrão), ponha o Harmony.dll na pasta de dependências.
3. Reinicie seu servidor.

### Documentos
- [Introdução de Funções](Features.md)
- [Introdução](GettingStarted.md)
- [Funções Principais](CoreFeatures.md)

### FAQ
1. Por que o plug-in não funciona?
- Certifique-se de que o HintServiceMeow está corretamente instalado.
- Verifique se há algum plug-in entrando em conflito com o HintServiceMeow
- Verifique se algum erro ocorre ao ativar plug-ins.
2. Por que hints se sobrepõem?
- Isso pode acontecer quando múltiplos plug-ins põem suas hints na mesma posição. Você pode verificar o arquivo de configuração de cada plug-in para ajustar a posição de sua UI. Se você não puder ajustar a posição usando o arquivo de configuração, por favor, contate o autor dos plug-ins.
