Нажмите [здесь](/Docs/Russian/README.md), чтобы вернуться к README

## Начало работы
### Настройка зависимостей
1. Создайте свой проект C#
2. Добавьте файл dll, скачанный из релиза, в зависимости вашего проекта
### Отображение первой подсказки
Следующие блоки кода показывают, как использовать часто используемые функции HSM.

---
Создание подсказки "Hello World" на экране игрока.
```CSharp
Player player = Player.Get(xxx);

// Hint — это простая подсказка, которую можно отображать на экране игрока.
Hint hint1 = new Hint
{
    Text = "Hello World" // Вы можете задавать свойства подсказки внутри пары фигурных скобок ({})
};

// Вы можете задать свойства подсказки следующим образом:
hint1.FontSize = 40;
hint1.YCoordinate = 700;
hint1.Alignment = HintAlignment.Left;
// После задания свойств не нужно вызывать никаких методов для запроса обновления. Все обновления будут выполняться автоматически через HSM (HintServiceMeow).

// Вы можете отобразить подсказку игроку, добавив её в PlayerDisplay игрока.
// Также её можно удалить, убрав из PlayerDisplay.
PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(hint1);
// playerDisplay.RemoveHint(hint);

```
---
Используйте `AutoText` для создания подсказки, которая автоматически обновляет содержимое.

Используйте расширения для простого добавления или удаления подсказок.
```CSharp
Hint hint2 = new Hint
{
    AutoText = ev => DateTime.Now.ToString("HH:mm:ss"), // Вы также можете использовать функцию для задания текста подсказки, и подсказка будет обновляться автоматически.
    Alignment = HintAlignment.Right, // Вы можете задавать свойства подсказки в любом порядке и можете не задавать некоторые свойства, так как все они имеют значения по умолчанию.
    YCoordinate = 200
};

// Вы также можете использовать метод расширения для упрощения
player.AddHint(hint2); // Это эквивалентно playerDisplay.AddHint(hint);
// player.RemoveHint(hint); // Это эквивалентно playerDisplay.RemoveHint(hint);

```
---
Используйте `NextUpdateDelay` для настройки частоты обновления AutoText.

Используйте `PlayerDisplay::ShowHint(Hint, float)` для отображения подсказки на определённое время.
```CSharp
Hint hint3 = new Hint()
{
    YCoordinate = 300,
    Alignment = HintAlignment.Right,
    AutoText = ev =>
    {
        ev.NextUpdateDelay = TimeSpan.FromSeconds(2f); // Вы можете задать задержку следующего обновления в аргументе события, и подсказка обновится после задержки. Это полезно, когда нужно обновлять подсказку через определённые интервалы.

        return "TPS: " + Server.Tps.ToString("F2");
    },
};

// Если вы хотите отобразить подсказку временно, используйте ShowHint
playerDisplay.ShowHint(hint3, 12f); // Отображает подсказку 12 секунд, затем скрывает её.

```
---
Используйте DynamicHint для предотвращения конфликтов.
```CSharp
// DynamicHint — это подсказка, которая автоматически позиционирует себя, чтобы избежать перекрытия с другими подсказками.
DynamicHint dynamicHint = new DynamicHint
{
    Text = "Привет, динамическая подсказка",
    TargetX = 100f,
};

playerDisplay.AddHint(dynamicHint);

```
---
Используйте CommonHint для быстрой разработки пользовательского интерфейса.
```CSharp
// PlayerUI::CommonHint — это набор предустановленных подсказок, которые помогают легко отображать подсказки
PlayerUI ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP173", ["Убить всех людей", "Использовать навыки"]);
ui.CommonHint.ShowMapHint("Зона Тяжёлого Содержания", "Место, где появляется большинство SCP");
ui.CommonHint.ShowItemHint("Карта доступа", "Используется для открытия дверей");
ui.CommonHint.ShowOtherHint("Сервер запускается!");
```
---
Используйте `ResolutionOption` для адаптации подсказок к различным разрешениям экрана.
```CSharp
// ResolutionOption.Offset автоматически сдвигает подсказки с выравниванием по левому/правому краю к краю экрана на основе разрешения экрана игрока.
Hint resolutionHint = new Hint
{
    Text = "Adapted to screen edge",
    Alignment = HintAlignment.Left,
    YCoordinate = 400,
    ResolutionOption = ResolutionOption.Offset // Это значение по умолчанию. Установите ResolutionOption.None для отключения.
};

playerDisplay.AddHint(resolutionHint);

```
---
Используйте вспомогательные `RichTag` для удобного применения тегов форматирования Unity.
```CSharp
// Используйте оператор / или расширение UseTag() для оборачивания текста тегами rich text
string colored = "Hello" / ColorTag.Red; // Результат: <color=#FF0000>Hello</color>
string bold = "World".UseTag(BoldTag.Bold); // Результат: <b>World</b>
string styled = "Fancy".UseTag(ColorTag.Get("#FF8800"), BoldTag.Bold, SizeTag.Get(30)); // Комбинирование нескольких тегов

Hint richHint = new Hint
{
    Text = colored + " " + bold + "\n" + styled,
    YCoordinate = 500,
};

playerDisplay.AddHint(richHint);

```
---
Используйте `Transition` для анимации изменений свойств подсказок.
```CSharp
// Transition плавно анимирует изменения свойств в течение указанного времени
Hint animatedHint = new Hint
{
    Text = "I move smoothly!",
    YCoordinate = 600,
    FontSize = 20,
    YCoordinateTransition = Transition.Get(EasingType.EaseInOut, duration: 1f), // Анимация изменения позиции Y в течение 1 секунды
    FontSizeTransition = Transition.Get(EasingType.EaseOut, duration: 0.5f), // Анимация изменения размера шрифта в течение 0,5 секунд
};

playerDisplay.AddHint(animatedHint);

// При изменении свойства переход анимирует его плавно
animatedHint.YCoordinate = 200; // Плавно перемещается с 600 до 200 за 1 секунду
animatedHint.FontSize = 40; // Плавно увеличивается с 20 до 40 за 0,5 секунды

```
---
Используйте `HintTemplate` для быстрого создания подсказок с предустановленными свойствами.
```CSharp
// HintTemplate — это шаблон для создания подсказок с предопределёнными свойствами
HintTemplate template = new HintTemplate
{
    FontSize = 25,
    YCoordinate = 700,
    Alignment = HintAlignment.Right,
    FontSizeTransition = Transition.Get(EasingType.EaseInOut, 0.5f),
};

// Используйте GetHint() для создания новой подсказки из шаблона
Hint hintFromTemplate = template.GetHint();
hintFromTemplate.Text = "Created from template";
playerDisplay.AddHint(hintFromTemplate);

// Или используйте Apply() для применения шаблона к существующей подсказке
Hint existingHint = new Hint { Text = "Existing hint" };
template.Apply(existingHint); // Применяет FontSize, YCoordinate, Alignment и FontSizeTransition
playerDisplay.AddHint(existingHint);

```
---
Приведённые выше блоки кода создадут такой интерфейс:
![Вид подсказки](Images/GettingStartedExample.jpg)
С метками:
![Вид подсказки с метками](Images/GettingStartedExampleLabeled.jpg)

Прочитайте [Основные функции](CoreFeatures.md), чтобы узнать больше

Нажмите [здесь](/Docs/Russian/README.md), чтобы вернуться к README
