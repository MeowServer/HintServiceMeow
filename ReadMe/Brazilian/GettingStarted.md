## Introdução
### Defina dependências
1. Crie seu projeto C#
2. Inclua o arquivo .dll instalado do lançamento nas dependências do seu projeto
### Mostre sua primeira hint
O código a seguir exibe "Hello World" no canto inferior esquerdo da tela do jogador.
```CSharp
//Associe isso ao evento de jogador verificado(jogador entrou)
public static void OnVerified(VerifiedEventArgs ev)
{
    //Cria uma instância de hint
    Hint hint = new Hint
    {
        Text = "Hello World"
    };

    //Define propriedades da hint(opcional)
    hint.YCoordinate = 200;
    hint.Alignment = HintAlignment.Left;

    //Obtém a exibição do jogador e adiciona hint à exibição do jogador
    PlayerDisplay playerDisplay = PlayerDisplay.Get(ev.Player);
    playerDisplay.AddHint(hint);
}
```
![A visualização da hint](Images/GettingStartedExample.jpg)
