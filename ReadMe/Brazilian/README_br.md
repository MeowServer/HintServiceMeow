# Introdução
**HintServiceMeow (HSM)** é um framework que permite que plug-ins exibam texto em uma posição selecionada na tela de um jogador.

---

# Instalação

Para instalar esse plug-in, siga os passos abaixo:

1. Vá à [página de lançamento](#) e baixe o arquivo mais recente `HintServiceMeow.dll`. Em seguida, cole-o na pasta de plug-ins.
2. Se você estiver usando a **PluginAPI** (API padrão), coloque o arquivo `Harmony.dll` na pasta de **dependências**.
3. Reinicie o servidor para concluir a instalação.

---

# Documentos

Aqui estão alguns recursos úteis para você começar:

- [Introdução de Funções](Features.md)
- [Introdução](GettingStarted.md)
- [Funções Principais](CoreFeatures.md)

---

# FAQ

### 1. Por que o plug-in não funciona?
- Certifique-se de que o **HintServiceMeow** está instalado corretamente.
- Verifique se há algum plug-in em conflito com o **HintServiceMeow**.
- Veja se ocorre algum erro durante a ativação dos plug-ins.

### 2. Por que as hints se sobrepõem?
- Isso pode acontecer quando múltiplos plug-ins colocam suas hints na mesma posição. Você pode verificar o arquivo de configuração de cada plug-in para ajustar a posição da UI.
- Se você não puder ajustar a posição usando o arquivo de configuração, por favor, entre em contato com o autor dos plug-ins para assistência.
