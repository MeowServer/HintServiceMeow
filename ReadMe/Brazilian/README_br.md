## Introdução
HintServiceMeow(HSM) é um framework que permite que plug-ins exibam texto em uma posição selecionada na tela de um jogador.

## Instalação
Para instalar esse plug-in, por favor
1. Vá à página de lançamento e baixe o HintServiceMeow.dll mais recente. Então, cole-o na pasta de plug-in.
2. Se você estiver usando a PluginAPI (A API padrão), ponha o Harmony.dll na pasta de dependências.
3. Reinicie seu servidor.

### Documentos
- [Features Introduction](Features.md)
- [Getting Started](GettingStarted.md)
- [Core Features](CoreFeatures.md)

### FAQ
1. Why doesn't the plugin work?
- Make sure that HintServiceMeow is correctly installed.
- Check if there's any plugin that conflicts with HintServiceMeow
- Check if any error occurs when activating plugins.
2. Why do hints overlaps with each other
- This might happen when multiple plugins put their hint in the same position. You can check the config file for each plugin to adjust the position of their UI. If you cannot adjust the position using the config file, please contact the author of the plugins.
