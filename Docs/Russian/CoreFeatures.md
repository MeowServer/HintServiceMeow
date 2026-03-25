Нажмите [здесь](/Docs/Russian/README.md), чтобы вернуться к README

# Основные функции

Данная документация охватывает публичный API HintServiceMeow, организованный по функциональным модулям.

---

## Содержание

- [Основные функции](#основные-функции)
  - [Содержание](#содержание)
  - [Модели подсказок](#модели-подсказок)
    - [AbstractHint](#abstracthint)
    - [Hint](#hint)
    - [DynamicHint](#dynamichint)
  - [PlayerDisplay](#playerdisplay)
  - [Слой UI](#слой-ui)
    - [PlayerUI](#playerui)
    - [CommonHint](#commonhint)
  - [Методы расширения](#методы-расширения)
    - [Расширения AbstractHint](#расширения-abstracthint)
    - [Расширения PlayerDisplay](#расширения-playerdisplay)
    - [Расширения NW Player](#расширения-nw-player)
    - [Расширения EXILED Player](#расширения-exiled-player)
  - [Содержимое подсказки](#содержимое-подсказки)
    - [AbstractHintContent](#abstracthintcontent)
    - [StringContent](#stringcontent)
    - [AutoContent](#autocontent)
  - [Переходы](#переходы)
    - [Класс Transition](#класс-transition)
    - [Перечисление EasingType](#перечисление-easingtype)
    - [Свойства перехода подсказок](#свойства-перехода-подсказок)
  - [Помощник Rich-тегов](#помощник-rich-тегов)
    - [Базовый класс RichTag](#базовый-класс-richtag)
    - [Конкретные теги](#конкретные-теги)
    - [Расширения строк](#расширения-строк)
  - [Адаптация разрешения](#адаптация-разрешения)
  - [Шаблоны](#шаблоны)
    - [AbstractHintTemplate](#abstracthinttemplate-1)
    - [HintConfig](#hintconfig)
    - [HintTemplate](#hinttemplate)
    - [DynamicHintConfig](#dynamichintconfig)
    - [DynamicHintTemplate](#dynamichinttemplate)
    - [Конфигурации только позиции](#конфигурации-только-позиции)

---

## Модели подсказок

### AbstractHint

> Пространство имён: `HintServiceMeow.Core.Models.Hints`

Базовый класс для всех типов подсказок. **Все подсказки (`Hint`, `DynamicHint` и др.) наследуются от `AbstractHint`**, который предоставляет общие свойства, такие как текстовое содержимое, размер шрифта, скорость синхронизации и видимость. Реализует `INotifyPropertyChanged`, чтобы изменения свойств автоматически вызывали обновление отображения.

При создании любого типа подсказки свойства, перечисленные ниже, всегда доступны независимо от конкретного типа подсказки.

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| Guid | `Guid` (только чтение) | Автоматически генерируемый уникальный идентификатор |
| Id | `string` | Пользовательский строковый идентификатор для поиска. По умолчанию: `""` |
| SyncSpeed | `HintSyncSpeed` | Приоритет обновления. По умолчанию: `Normal`. Значения: `Fastest` (192) — обновляется как можно скорее, может задерживать другие подсказки; `Fast` (160) — планирует обновление немедленно при изменении; `Normal` (128) — стандартная скорость; `Slow` (96) — ждёт других подсказок; `Slowest` (64) — ждёт дольше; `UnSync` (32) — без автосинхронизации, но всё равно обновляется, когда другие подсказки инициируют синхронизацию |
| FontSize | `int` | Размер шрифта текста. По умолчанию: `20` |
| LineHeight | `float` | Дополнительный вертикальный интервал между строками |
| Content | `AbstractHintContent` | Провайдер содержимого для данной подсказки. По умолчанию: `StringContent("")` |
| Text | `string?` | Ярлык для получения/задания статического текста. Задание этого свойства заменяет `Content` новым `StringContent` |
| AutoText | `AutoContent.TextUpdateHandler?` | Ярлык для получения/задания делегата динамического текста. Задание этого свойства заменяет `Content` новым `AutoContent` |
| Hide | `bool` | Скрыта ли подсказка. По умолчанию: `false` |

**Пример использования:**

```csharp
// Свойства автоматически синхронизируются с экраном игрока
hint.Text = "Обновлённый текст";
hint.FontSize = 30;
// Дополнительных вызовов методов не требуется
```

---

### Hint

> Пространство имён: `HintServiceMeow.Core.Models.Hints`

Подсказка с фиксированной позицией, отображаемая в конкретных координатах экрана. Наследуется от [AbstractHint](#abstracthint).

**Свойства (в дополнение к AbstractHint):**

| Свойство | Тип | Описание |
|----------|-----|----------|
| YCoordinate | `float` | Вертикальная позиция. Большие значения перемещают текст ниже на экране. По умолчанию: `700` |
| XCoordinate | `float` | Горизонтальное смещение. Большие значения перемещают текст вправо. По умолчанию: `0` |
| Alignment | `HintAlignment` | Выравнивание текста. Значения: `Left`, `Right`, `Center`. По умолчанию: `Center` |
| YCoordinateAlign | `HintVerticalAlign` | Как координата Y выравнивается относительно текста. Значения: `Top` — Y является верхним краем; `Middle` — Y является вертикальным центром; `Bottom` — Y является нижним краем. По умолчанию: `Middle` |

![Пример координаты Y](Images/YCoordinateExample.jpg)

**Пример использования:**

```csharp
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

Поскольку в HSM есть функция автообновления, любые изменения свойств автоматически отражаются на экране игрока без дополнительных вызовов методов.

```csharp
hint.Text = "Какой-то новый текст";
// Дополнительных вызовов методов не требуется
```

---

### DynamicHint

> Пространство имён: `HintServiceMeow.Core.Models.Hints`

Подсказка, которая автоматически позиционируется, чтобы избежать перекрытия с другими подсказками. Наследуется от [AbstractHint](#abstracthint).

**Свойства (в дополнение к AbstractHint):**

| Свойство | Тип | Описание |
|----------|-----|----------|
| TopBoundary | `float` | Верхняя граница для размещения. По умолчанию: `0` |
| BottomBoundary | `float` | Нижняя граница для размещения. По умолчанию: `1000` |
| LeftBoundary | `float` | Левая граница для размещения. По умолчанию: `-1200` |
| RightBoundary | `float` | Правая граница для размещения. По умолчанию: `1200` |
| TargetX | `float` | Предпочтительная горизонтальная позиция. По умолчанию: `0` |
| TargetY | `float` | Предпочтительная вертикальная позиция. По умолчанию: `700` |
| TopMargin | `float` | Дополнительное пространство выше подсказки при расстановке. По умолчанию: `5` |
| BottomMargin | `float` | Дополнительное пространство ниже подсказки при расстановке. По умолчанию: `5` |
| LeftMargin | `float` | Дополнительное пространство слева при расстановке. По умолчанию: `100` |
| RightMargin | `float` | Дополнительное пространство справа при расстановке. По умолчанию: `100` |
| Priority | `HintPriority` | Приоритет расстановки. Подсказки с более высоким приоритетом расставляются первыми. Значения: `Highest` (192), `High` (160), `Medium` (128), `Low` (96), `Lowest` (64). По умолчанию: `Medium` |
| Strategy | `DynamicHintStrategy` | Поведение при отсутствии доступного места. Значения: `Hide` — скрыть подсказку; `StayInPosition` — оставить на целевой позиции. По умолчанию: `Hide` |

**Пример использования:**

```csharp
var dynamicHint = new DynamicHint
{
    Text = "Привет, динамическая подсказка"
};

PlayerDisplay playerDisplay = PlayerDisplay.Get(player);
playerDisplay.AddHint(dynamicHint);
```

---

## PlayerDisplay

> Пространство имён: `HintServiceMeow.Core.Utilities`

Центральный класс для управления отображением подсказок игрока. У каждого игрока есть один экземпляр `PlayerDisplay`.

**События:**

| Событие | Тип | Описание |
|---------|-----|----------|
| UpdateAvailable | `UpdateAvailableEventHandler` | Вызывается каждый тик, когда дисплей готов к обновлению |

**Делегат:** `delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev)`

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| ReferenceHub | `ReferenceHub?` (только чтение) | Игрок, которому принадлежит данный дисплей |
| HintParser | `IHintParser` | Парсер, который преобразует подсказки в rich text. Заменяемый |
| CompatibilityAdaptor | `ICompatibilityAdaptor` | Адаптер для совместимости с другими плагинами. Заменяемый |

**Статические методы:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| Get | `ReferenceHub referenceHub` | `PlayerDisplay` | Получает или создаёт PlayerDisplay для игрока |
| Get | `LabApi.Features.Wrappers.Player player` | `PlayerDisplay` | Получает или создаёт PlayerDisplay (NW/LabApi) |
| Get | `Exiled.API.Features.Player player` | `PlayerDisplay` | Получает или создаёт PlayerDisplay (только EXILED) |

**Методы экземпляра:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| AddHint | `AbstractHint? hint` | `void` | Добавляет подсказку в дисплей |
| AddHint | `IEnumerable<AbstractHint>? hints` | `void` | Добавляет несколько подсказок |
| AddHint | `params AbstractHint[]? hints` | `void` | Добавляет несколько подсказок (params) |
| AddHint | `AbstractHint? hint, string groupName` | `void` | Добавляет подсказку в определённую группу |
| ShowHint | `AbstractHint hint, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove` | `void` | Добавляет подсказку и автоматически удаляет/скрывает её через `duration` секунд. Значения `AfterShowAction`: `Remove` — удалить подсказку; `Hide` — установить `Hide = true` |
| ShowHint | `IEnumerable<AbstractHint> hints, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove` | `void` | Отображает несколько подсказок с автоудалением |
| RemoveHint | `AbstractHint? hint` | `void` | Удаляет подсказку |
| RemoveHint | `IEnumerable<AbstractHint>? hints` | `void` | Удаляет несколько подсказок |
| RemoveHint | `params AbstractHint[]? hints` | `void` | Удаляет несколько подсказок (params) |
| RemoveHint | `AbstractHint? hint, string groupName` | `void` | Удаляет подсказку из определённой группы |
| RemoveHint | `string id` | `void` | Удаляет все подсказки с указанным Id |
| RemoveHint | `Guid id` | `void` | Удаляет подсказку с указанным Guid |
| ClearHint | — | `void` | Удаляет все подсказки, принадлежащие вызывающей сборке |
| GetHint | `string? id` | `AbstractHint?` | Возвращает первую подсказку с соответствующим Id |
| GetHint | `Guid guid` | `AbstractHint?` | Возвращает первую подсказку с соответствующим Guid |
| GetHints | `string id` | `IEnumerable<AbstractHint>` | Возвращает все подсказки с соответствующим Id |
| GetHints | — | `IEnumerable<AbstractHint>` | Возвращает все подсказки, принадлежащие вызывающей сборке |
| HasHint | `string id` | `bool` | Проверяет, существует ли подсказка с указанным Id |
| HasHint | `Guid guid` | `bool` | Проверяет, существует ли подсказка с указанным Guid |
| TryGetHint | `string id, out AbstractHint hint` | `bool` | Пытается получить первую подсказку с соответствующим Id |
| TryGetHint | `Guid guid, out AbstractHint hint` | `bool` | Пытается получить первую подсказку с соответствующим Guid |
| TryGetHints | `string? id, out IEnumerable<AbstractHint> hints` | `bool` | Пытается получить все подсказки с соответствующим Id |
| ForceUpdate | `bool useFastUpdate = false` | `void` | Принудительно обновляет дисплей. Используйте при работе с `HintSyncSpeed.UnSync` |
| SetMinUpdateInterval | `TimeSpan interval` | `void` | Устанавливает минимальный интервал между обновлениями |
| AddDisplayOutput | `IDisplayOutput output` | `void` | Добавляет пользовательский вывод дисплея |
| RemoveDisplayOutput | `IDisplayOutput output` | `void` | Удаляет вывод дисплея |
| RemoveDisplayOutput\<T\> | — | `void` | Удаляет все выводы дисплея типа `T` (где `T : IDisplayOutput`) |

**Пример использования:**

```csharp
PlayerDisplay pd = PlayerDisplay.Get(player);

// Добавить подсказку
var hint = new Hint { Text = "Привет", YCoordinate = 500 };
pd.AddHint(hint);

// Показать временную подсказку на 5 секунд
pd.ShowHint(new Hint { Text = "Временная!" }, duration: 5f);

// Найти и изменить подсказки
if (pd.TryGetHint("my-hint-id", out var found))
{
    found.Text = "Обновлено";
}

// Принудительное обновление для UnSync подсказок
pd.ForceUpdate();
```

---

## Слой UI

### PlayerUI

> Пространство имён: `HintServiceMeow.UI.Utilities`

Фасад пользовательского интерфейса для каждого игрока, предоставляющий доступ к [CommonHint](#commonhint).

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| ReferenceHub | `ReferenceHub` (только чтение) | Ссылка на базового игрока |
| PlayerDisplay | `PlayerDisplay` (только чтение) | Экземпляр PlayerDisplay игрока |
| CommonHint | `CommonHint` (только чтение) | Компонент общих подсказок |

**Статические методы:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| Get | `ReferenceHub referenceHub` | `PlayerUI` | Получает или создаёт PlayerUI для игрока |
| Get | `LabApi.Features.Wrappers.Player player` | `PlayerUI` | Получает или создаёт PlayerUI (NW/LabApi) |
| Get | `Exiled.API.Features.Player player` | `PlayerUI` | Получает или создаёт PlayerUI (только EXILED) |

---

### CommonHint

> Пространство имён: `HintServiceMeow.UI.Utilities`

Предоставляет предварительно настроенные макеты подсказок для распространённых случаев: описания предметов, информация о карте, информация о роли и общие сообщения.

Все длительности отображения настраиваются через конфигурацию плагина. Параметр `time` в каждой перегрузке указывается в секундах.

**Методы — подсказки предмета:**

| Метод | Параметры | Описание |
|-------|-----------|----------|
| ShowItemHint | `string itemName` | Показывает только название предмета (короткая длительность) |
| ShowItemHint | `string itemName, float time` | Показывает только название предмета с пользовательской длительностью |
| ShowItemHint | `string itemName, string description` | Показывает название предмета и одну строку описания |
| ShowItemHint | `string itemName, string description, float time` | Показывает название предмета и одну строку описания с пользовательской длительностью |
| ShowItemHint | `string itemName, string[] description` | Показывает название предмета и несколько строк описания |
| ShowItemHint | `string itemName, string[] description, float time` | Показывает название предмета и несколько строк описания с пользовательской длительностью |

**Методы — подсказки карты:**

| Метод | Параметры | Описание |
|-------|-----------|----------|
| ShowMapHint | `string roomName` | Показывает только название комнаты (короткая длительность) |
| ShowMapHint | `string roomName, float time` | Показывает только название комнаты с пользовательской длительностью |
| ShowMapHint | `string roomName, string description` | Показывает название комнаты и одну строку описания |
| ShowMapHint | `string roomName, string description, float time` | Показывает название комнаты и одну строку описания с пользовательской длительностью |
| ShowMapHint | `string roomName, string[] description` | Показывает название комнаты и несколько строк описания |
| ShowMapHint | `string roomName, string[] description, float time` | Показывает название комнаты и несколько строк описания с пользовательской длительностью |

**Методы — подсказки роли:**

| Метод | Параметры | Описание |
|-------|-----------|----------|
| ShowRoleHint | `string roleName` | Показывает только название роли (короткая длительность) |
| ShowRoleHint | `string roleName, float time` | Показывает только название роли с пользовательской длительностью |
| ShowRoleHint | `string roleName, string description` | Показывает название роли и одну строку описания |
| ShowRoleHint | `string roleName, string description, float time` | Показывает название роли и одну строку описания с пользовательской длительностью |
| ShowRoleHint | `string roleName, string[] description` | Показывает название роли и несколько строк описания |
| ShowRoleHint | `string roleName, string[] description, float time` | Показывает название роли и несколько строк описания с пользовательской длительностью |

**Методы — прочие подсказки:**

| Метод | Параметры | Описание |
|-------|-----------|----------|
| ShowOtherHint | `string messages` | Показывает одно сообщение как DynamicHint |
| ShowOtherHint | `string messages, float time` | Показывает одно сообщение с пользовательской длительностью |
| ShowOtherHint | `string[] messages` | Показывает несколько сообщений (длительность масштабируется с количеством) |
| ShowOtherHint | `string[] messages, float time` | Показывает несколько сообщений с пользовательской общей длительностью |

**Пример использования:**

```csharp
var ui = PlayerUI.Get(player);
ui.CommonHint.ShowRoleHint("SCP-173", new[] { "Убить всех людей", "Использовать навыки" });
ui.CommonHint.ShowMapHint("Зона Тяжёлого Содержания", "Место, где появляется большинство SCP");
ui.CommonHint.ShowItemHint("Карта доступа", "Используется для открытия дверей");
ui.CommonHint.ShowOtherHint("Сервер запускается!");
```

---

## Методы расширения

Все методы расширения собраны здесь для удобного использования.

### Расширения AbstractHint

> Пространство имён: `HintServiceMeow.Core.Extension`

| Метод | Расширяет | Параметры | Описание |
|-------|-----------|-----------|----------|
| HideAfter | `AbstractHint` | `float delay` | Устанавливает `Hide = true` через `delay` секунд. Сбрасывает любой существующий таймер скрытия |

```csharp
hint.HideAfter(5f); // Скрывает подсказку через 5 секунд
```

### Расширения PlayerDisplay

> Пространство имён: `HintServiceMeow.Core.Extension`

| Метод | Расширяет | Параметры | Описание |
|-------|-----------|-----------|----------|
| RemoveAfter | `PlayerDisplay` | `AbstractHint hint, float delay` | Удаляет подсказку из дисплея через `delay` секунд. Сбрасывает любой существующий таймер удаления |

```csharp
playerDisplay.RemoveAfter(hint, 10f); // Удаляет подсказку через 10 секунд
```

### Расширения NW Player

> Пространство имён: `HintServiceMeow.Core.Extension` / `HintServiceMeow.UI.Extension`

Методы расширения для `LabApi.Features.Wrappers.Player`.

| Метод | Пространство имён | Возвращает | Описание |
|-------|-------------------|------------|----------|
| GetPlayerDisplay | Core | `PlayerDisplay` | Получает PlayerDisplay игрока |
| AddHint | Core | `void` | Добавляет подсказку в дисплей игрока |
| RemoveHint | Core | `void` | Удаляет подсказку из дисплея игрока |
| GetPlayerUi | UI | `PlayerUI` | Получает экземпляр PlayerUI игрока |

```csharp
// Использование расширений NW player (LabApi)
LabApi.Features.Wrappers.Player player = ...;

// Получить PlayerDisplay и добавить подсказку прямо на объект игрока
var hint = new Hint { Text = "Привет из расширения NW!", YCoordinate = 500 };
player.AddHint(hint);

// Позже удалить
player.RemoveHint(hint);

// Доступ к PlayerDisplay для более сложных операций
PlayerDisplay pd = player.GetPlayerDisplay();
pd.ShowHint(new Hint { Text = "Временная!" }, duration: 3f);

// Доступ к PlayerUI и CommonHint
PlayerUI ui = player.GetPlayerUi();
ui.CommonHint.ShowRoleHint("SCP-096", new[] { "Сидеть и плакать", "Преследовать цели" });
```

### Расширения EXILED Player

> Пространство имён: `HintServiceMeow.Core.Extension` / `HintServiceMeow.UI.Extension`

Методы расширения для `Exiled.API.Features.Player`. Доступны только в сборках EXILED.

| Метод | Пространство имён | Возвращает | Описание |
|-------|-------------------|------------|----------|
| GetPlayerDisplay | Core | `PlayerDisplay` | Получает PlayerDisplay игрока |
| AddHint | Core | `void` | Добавляет подсказку в дисплей игрока |
| RemoveHint | Core | `void` | Удаляет подсказку из дисплея игрока |
| GetPlayerUi | UI | `PlayerUI` | Получает экземпляр PlayerUI игрока |

```csharp
// Использование расширений EXILED player
Exiled.API.Features.Player player = ...;

// Получить PlayerDisplay и добавить подсказку прямо на объект игрока
var hint = new Hint { Text = "Привет из расширения EXILED!", YCoordinate = 500 };
player.AddHint(hint);

// Позже удалить
player.RemoveHint(hint);

// Доступ к PlayerDisplay для более сложных операций
PlayerDisplay pd = player.GetPlayerDisplay();
pd.ShowHint(new Hint { Text = "Временная!" }, duration: 3f);

// Доступ к PlayerUI и CommonHint
PlayerUI ui = player.GetPlayerUi();
ui.CommonHint.ShowItemHint("Карта O5", "Открывает доступ ко всем зонам");
```

---

## Содержимое подсказки

Эти классы используются внутри подсказок для управления их текстовым содержимым. В большинстве случаев вам не нужно взаимодействовать с ними напрямую — используйте свойства `Text` или `AutoText` в [AbstractHint](#abstracthint).

### AbstractHintContent

> Пространство имён: `HintServiceMeow.Core.Models.HintContent`

Базовый класс для провайдеров содержимого подсказок.

**События:**

| Событие | Тип | Описание |
|---------|-----|----------|
| ContentUpdated | `UpdateHandler` | Вызывается при изменении содержимого |

**Методы:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| GetText | — | `string?` | Возвращает текущее текстовое содержимое |

---

### StringContent

> Пространство имён: `HintServiceMeow.Core.Models.HintContent`

Провайдер содержимого, хранящий статический текст. Наследуется от [AbstractHintContent](#abstracthintcontent).

**Конструктор:** `StringContent(string? content)`

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| Text | `string?` | Статический текст. При изменении вызывает `ContentUpdated` |

---

### AutoContent

> Пространство имён: `HintServiceMeow.Core.Models.HintContent`

Провайдер содержимого, периодически вызывающий делегат для генерации динамического текста. Наследуется от [AbstractHintContent](#abstracthintcontent).

**Делегат:** `delegate string TextUpdateHandler(AutoContentUpdateArg ev)`

**Конструктор:** `AutoContent(TextUpdateHandler? autoText, float defaultUpdateInterval = -1)`

Если `defaultUpdateInterval` отрицательный, по умолчанию используется `0.1` секунды.

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| AutoText | `TextUpdateHandler?` | Делегат, вызываемый для генерации текста. Сброс этого свойства также сбрасывает время следующего обновления |

**Пример использования:**

```csharp
hint.AutoText = (ev) =>
{
    ev.NextUpdateDelay = TimeSpan.FromSeconds(1); // Обновлять каждую 1 секунду
    return $"Время: {DateTime.Now:HH:mm:ss}";
};
```

---

## Переходы

Переходы обеспечивают плавную анимированную интерполяцию при изменении свойств подсказок. Вместо мгновенного перехода к новому значению, свойство постепенно изменяется в течение настраиваемой продолжительности с использованием функции сглаживания.

### Класс Transition

> Пространство имён: `HintServiceMeow.Core.Models.Transition`

Определяет, как должно анимироваться изменение свойства.

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| Duration | `float` | Продолжительность анимации в секундах. Минимум: `0.001`. По умолчанию: `0.5` |
| Easing | `EasingType` | Функция сглаживания для интерполяции. По умолчанию: `EaseInOut` |
| NormalizedCurve | `IAnimationCurve` | Пользовательская кривая анимации (нормализованная 0–1). Используется когда `Easing` установлен как `Custom` |

**Статические фабричные методы:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| Get | `EasingType type = EaseInOut, float duration = 0.5f` | `Transition` | Создаёт переход со встроенным сглаживанием |
| Get | `IAnimationCurve normalizedCurve, float duration = 0.5f` | `Transition` | Создаёт переход с пользовательской кривой |

**Пример использования:**

```csharp
// Создание переходов со встроенным сглаживанием
var smooth = Transition.Get(EasingType.EaseInOut, duration: 1f);
var quickFade = Transition.Get(EasingType.EaseOut, duration: 0.3f);
var linear = Transition.Get(EasingType.Linear, duration: 2f);

// Назначение свойствам подсказки
hint.YCoordinateTransition = smooth;
hint.FontSizeTransition = quickFade;
```

---

### Перечисление EasingType

> Пространство имён: `HintServiceMeow.Core.Enum`

Определяет форму кривой интерполяции.

| Значение | Описание |
|----------|----------|
| `Linear` | Постоянная скорость от начала до конца |
| `EaseIn` | Начинается медленно, ускоряется к концу |
| `EaseOut` | Начинается быстро, замедляется к концу |
| `EaseInOut` | Начинается и заканчивается медленно, максимальная скорость в середине |
| `Custom` | Использует `Transition.NormalizedCurve` для пользовательской кривой |

---

### Свойства перехода подсказок

Эти свойства доступны для типов подсказок для включения анимированных переходов:

**В `Hint`** (в дополнение к свойствам [AbstractHint](#abstracthint)):

| Свойство | Тип | Описание |
|----------|-----|----------|
| FontSizeTransition | `Transition?` | Переход при изменении `FontSize`. По умолчанию: `null` (без анимации) |
| XCoordinateTransition | `Transition?` | Переход при изменении `XCoordinate`. По умолчанию: `null` |
| YCoordinateTransition | `Transition?` | Переход при изменении `YCoordinate`. По умолчанию: `null` |

**В `DynamicHint`:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| FontSizeTransition | `Transition?` | Переход при изменении `FontSize`. По умолчанию: `null` (без анимации) |

> Примечание: `DynamicHint` не поддерживает `XCoordinateTransition` или `YCoordinateTransition`, поскольку его позиция управляется автоматически.

**Пример использования:**

```csharp
var hint = new Hint
{
    Text = "Анимированная подсказка",
    YCoordinate = 100,
    FontSize = 20,
    YCoordinateTransition = Transition.Get(EasingType.EaseInOut, 1f),
    FontSizeTransition = Transition.Get(EasingType.EaseOut, 0.5f),
};

playerDisplay.AddHint(hint);

// Позже измените свойства — переходы анимируют их плавно
hint.YCoordinate = 800; // Плавно перемещается в течение 1 секунды
hint.FontSize = 40;     // Плавно увеличивается в течение 0,5 секунд
```

---

## Помощник Rich-тегов

Помощник Rich-тегов предоставляет типобезопасный fluent API для создания разметки Unity TextMeshPro rich text. Вместо написания необработанных тегов вроде `<color=#FF0000>text</color>`, вы можете использовать объекты тегов и операторы.

### Базовый класс RichTag

> Пространство имён: `HintServiceMeow.UI.Models`

Абстрактный базовый класс для всех обёрток rich text тегов.

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| OpenTag | `string` (только чтение) | Синтаксис открывающего тега (например, `<color=#FF0000>`) |
| CloseTag | `string` (только чтение) | Синтаксис закрывающего тега (например, `</color>`) |

**Методы:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| Apply | `string str` | `string` | Оборачивает текст открывающим и закрывающим тегами |

**Оператор:**

| Оператор | Использование | Описание |
|----------|---------------|----------|
| `/` | `"text" / tag` | Сокращение для `tag.Apply("text")` |

**Пример использования:**

```csharp
// Использование оператора /
string red = "Hello" / ColorTag.Red; // <color=#FF0000>Hello</color>

// Использование Apply()
string bold = BoldTag.Bold.Apply("World"); // <b>World</b>
```

---

### Конкретные теги

> Пространство имён: `HintServiceMeow.UI.Models.RichTags`

Все конкретные классы тегов наследуются от [RichTag](#базовый-класс-richtag). Теги с фиксированным поведением используют паттерн одиночки; параметризированные теги используют фабричные методы.

**Теги стиля:**

| Класс тега | Аксессор | Пример вывода |
|------------|----------|---------------|
| `BoldTag` | `BoldTag.Bold` | `<b>text</b>` |
| `ItalicsTag` | `ItalicsTag.Italics` | `<i>text</i>` |
| `UnderlineTag` | `UnderlineTag.Underline` | `<u>text</u>` |
| `StrikethroughTag` | `StrikethroughTag.Strikethrough` | `<s>text</s>` |

**Теги регистра:**

| Класс тега | Аксессор | Пример вывода |
|------------|----------|---------------|
| `AllcapsTag` | `AllcapsTag.Allcaps` | `<allcaps>text</allcaps>` |
| `LowercaseTag` | `LowercaseTag.Lowercase` | `<lowercase>text</lowercase>` |
| `SmallcapTag` | `SmallcapTag.Smallcap` | `<smallcaps>text</smallcaps>` |

**Теги цвета и размера:**

| Класс тега | Фабрика / Аксессор | Описание |
|------------|-------------------|----------|
| `ColorTag` | `ColorTag.Red`, `.Green`, `.Blue`, `.White`, `.Black`, `.Yellow`, `.Orange`, `.Purple`, `.Cyan`, `.Magenta`, `.Grey` | Предустановленные цвета-одиночки |
| `ColorTag` | `ColorTag.Get(string value)` | Hex (`"#FF0000"`) или именованный цвет (`"red"`) |
| `ColorTag` | `ColorTag.Get(byte r, byte g, byte b)` | Значения RGB |
| `SizeTag` | `SizeTag.Get(int pixel)` | Абсолютный размер в пикселях |
| `SizeTag` | `SizeTag.Get(string value)` | Размер с единицей (`"150%"`, `"1.5em"`) |

**Теги интервалов:**

| Класс тега | Фабрика | Описание |
|------------|---------|----------|
| `SpaceTag` | `SpaceTag.Get(string)` | Горизонтальный интервал |
| `CSpaceTag` | `CSpaceTag.Get(string)` | Межсимвольный интервал |
| `MSpaceTag` | `MSpaceTag.Get(string)` | Моноширинная ширина |
| `IndentTag` | `IndentTag.Get(string)` | Отступ первой строки |
| `LineIndentTag` | `LineIndentTag.Get(string)` | Отступ всех строк |
| `LineHeightTag` | `LineHeightTag.Get(string)` | Высота строки |

**Теги позиции:**

| Класс тега | Фабрика | Описание |
|------------|---------|----------|
| `PosTag` | `PosTag.Get(string)` | Горизонтальная позиция |
| `MarginTag` | `MarginTag.Get(string)` | Отступ текста |
| `VOffsetTag` | `VOffsetTag.Get(string)` | Вертикальное смещение |
| `RotateTag` | `RotateTag.Get(string)` | Поворот текста |
| `WidthTag` | `WidthTag.Get(string)` | Ширина текстовой области |
| `AlignTag` | `AlignTag.Get(string)` | Выравнивание текста |

**Теги шрифта и внешнего вида:**

| Класс тега | Фабрика | Описание |
|------------|---------|----------|
| `FontTag` | `FontTag.Get(string)` | Семейство шрифтов |
| `FontWeightTag` | `FontWeightTag.Get(string)` | Насыщенность шрифта |
| `AlphaTag` | `AlphaTag.Get(string)` | Непрозрачность текста |
| `MarkTag` | `MarkTag.Get(string)` | Цвет выделения/фона текста |
| `GradientTag` | `GradientTag.Get(string)` | Цветовой градиент |

**Специальные теги:**

| Класс тега | Аксессор / Фабрика | Описание |
|------------|-------------------|----------|
| `NoParseTag` | `NoParseTag.NoParse` | Предотвращает разбор внутреннего текста как rich text |
| `NoBRTag` | `NoBRTag.NoBR` | Предотвращает перенос строки внутри помеченного текста |
| `BreakTag` | `BreakTag.Break` | Вставляет перенос строки |
| `LinkTag` | `LinkTag.Get(string)` | Создаёт ID ссылки |
| `HyperlinkTag` | `HyperlinkTag.Get(string)` | Создаёт гиперссылку |
| `SpriteTag` | `SpriteTag.Get(string)` | Вставляет спрайт |

---

### Расширения строк

> Пространство имён: `HintServiceMeow.UI.Extension`

Методы расширения для `string` для удобного применения тегов.

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| UseTag | `this string str, RichTag tag` | `string` | Применяет один тег к строке |
| UseTag | `this string str, params RichTag[] tags` | `string` | Применяет несколько тегов (самый внешний первым) |

**Пример использования:**

```csharp
// Один тег
string red = "Hello".UseTag(ColorTag.Red);

// Несколько тегов — применяются от внешнего к внутреннему
string styled = "Fancy".UseTag(ColorTag.Get("#FF8800"), BoldTag.Bold, SizeTag.Get(30));
// Результат: <color=#FF8800><b><size=30>Fancy</size></b></color>

// Использование оператора /
string quick = "Quick" / ColorTag.Blue / BoldTag.Bold;
// Результат: <b><color=#0000FF>Quick</color></b>
```

---

## Адаптация разрешения

Адаптация разрешения автоматически корректирует позиционирование подсказок на основе соотношения сторон экрана игрока, обеспечивая корректное отображение подсказок на экранах разных размеров.

> Пространство имён: `HintServiceMeow.Core.Enum`

### Перечисление ResolutionOption

| Значение | Описание |
|----------|----------|
| `None` | Без адаптации разрешения. Подсказки используют необработанные значения координат |
| `Offset` | Сдвигает подсказки с выравниванием по левому/правому краю к краю экрана на основе соотношения XY экрана игрока |

### Свойство AbstractHint

| Свойство | Тип | Описание |
|----------|-----|----------|
| ResolutionOption | `ResolutionOption` | Управляет адаптацией подсказки к различным разрешениям экрана. По умолчанию: `Offset` |

При значении `Offset` система отслеживает разрешение экрана каждого игрока и корректирует `XCoordinate` подсказок с выравниванием по левому/правому краю, чтобы они отображались последовательно у края экрана независимо от соотношения сторон.

**Пример использования:**

```csharp
// Адаптация разрешения включена по умолчанию (Offset)
var hint = new Hint
{
    Text = "Всегда у края",
    Alignment = HintAlignment.Left,
    YCoordinate = 400,
    // ResolutionOption = ResolutionOption.Offset  // Это уже значение по умолчанию
};

// Отключите адаптацию разрешения для фиксированного позиционирования
var fixedHint = new Hint
{
    Text = "Фиксированная позиция",
    Alignment = HintAlignment.Left,
    YCoordinate = 400,
    ResolutionOption = ResolutionOption.None
};
```

---

## Шаблоны

Шаблоны предоставляют паттерн blueprint для создания и настройки подсказок. Они поддерживают nullable-свойства — применяются только ненулевые значения, что упрощает определение частичных конфигураций. Классы Config поддерживают YAML-сериализацию для внешней конфигурации, а классы Template добавляют свойства только для кода, помеченные `[YamlIgnore]`.

> Пространство имён: `HintServiceMeow.UI.Models.Template`

### Иерархия классов

```
AbstractHintTemplate
├── HintConfig
│   └── HintTemplate
└── DynamicHintConfig
    └── DynamicHintTemplate

HintPositionConfig (самостоятельный, только позиция)
DynamicHintPositionConfig (самостоятельный, только позиция)
```

---

### AbstractHintTemplate

Базовый класс для всех шаблонов подсказок. Все свойства nullable — при вызове `Apply()` применяются только ненулевые значения.

**Свойства:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| SyncSpeed | `HintSyncSpeed?` | Приоритет обновления |
| FontSize | `int?` | Размер шрифта текста |
| LineHeight | `float?` | Дополнительный вертикальный интервал между строками |
| Text | `string?` | Статическое текстовое содержимое |

---

### HintConfig

> Расширяет: [AbstractHintTemplate](#abstracthinttemplate-1)

YAML-сериализуемая конфигурация для подсказок с фиксированной позицией.

**Свойства (в дополнение к AbstractHintTemplate):**

| Свойство | Тип | Описание |
|----------|-----|----------|
| XCoordinate | `float?` | Горизонтальное смещение |
| YCoordinate | `float?` | Вертикальная позиция |
| Alignment | `HintAlignment?` | Выравнивание текста |
| YCoordinateAlign | `HintVerticalAlign?` | Как координата Y выравнивается относительно текста |

**Методы:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| Apply | `Hint hint` | `void` | Применяет все ненулевые свойства к подсказке |
| GetHint | — | `Hint` | Создаёт новый `Hint` с применёнными ненулевыми свойствами |

---

### HintTemplate

> Расширяет: [HintConfig](#hintconfig)

Полный шаблон с дополнительными свойствами только для кода. Свойства, помеченные `[YamlIgnore]`, не сериализуются.

**Свойства (в дополнение к HintConfig):**

| Свойство | Тип | Сериализуемо | Описание |
|----------|-----|--------------|----------|
| Id | `string?` | Да | Логический идентификатор |
| Hide | `bool?` | Да | Видимость |
| AutoText | `AutoContent.TextUpdateHandler?` | Нет | Обработчик динамического текста |
| Content | `AbstractHintContent?` | Нет | Провайдер содержимого |
| FontSizeTransition | `Transition?` | Нет | Анимация размера шрифта |
| XCoordinateTransition | `Transition?` | Нет | Анимация координаты X |
| YCoordinateTransition | `Transition?` | Нет | Анимация координаты Y |

**Пример использования:**

```csharp
// Создание шаблона как blueprint
var template = new HintTemplate
{
    FontSize = 25,
    YCoordinate = 700,
    Alignment = HintAlignment.Right,
    FontSizeTransition = Transition.Get(EasingType.EaseInOut, 0.5f),
};

// Создание новой подсказки из шаблона
Hint hint = template.GetHint();
hint.Text = "Создана из шаблона";
playerDisplay.AddHint(hint);

// Или применение шаблона к существующей подсказке
var existing = new Hint { Text = "Существующая" };
template.Apply(existing); // Применяет только ненулевые свойства
```

---

### DynamicHintConfig

> Расширяет: [AbstractHintTemplate](#abstracthinttemplate-1)

YAML-сериализуемая конфигурация для автоматически позиционируемых подсказок.

**Свойства (в дополнение к AbstractHintTemplate):**

| Свойство | Тип | Описание |
|----------|-----|----------|
| TopBoundary | `float?` | Верхняя граница для размещения |
| BottomBoundary | `float?` | Нижняя граница для размещения |
| LeftBoundary | `float?` | Левая граница для размещения |
| RightBoundary | `float?` | Правая граница для размещения |
| TargetX | `float?` | Предпочтительная горизонтальная позиция |
| TargetY | `float?` | Предпочтительная вертикальная позиция |
| TopMargin | `float?` | Дополнительное пространство сверху |
| BottomMargin | `float?` | Дополнительное пространство снизу |
| LeftMargin | `float?` | Дополнительное пространство слева |
| RightMargin | `float?` | Дополнительное пространство справа |
| Priority | `HintPriority?` | Приоритет расстановки |
| Strategy | `DynamicHintStrategy?` | Поведение при отсутствии доступного места |

**Методы:**

| Метод | Параметры | Возвращает | Описание |
|-------|-----------|------------|----------|
| Apply | `DynamicHint hint` | `void` | Применяет все ненулевые свойства к подсказке |
| GetDynamicHint | — | `DynamicHint` | Создаёт новый `DynamicHint` с применёнными ненулевыми свойствами |

---

### DynamicHintTemplate

> Расширяет: [DynamicHintConfig](#dynamichintconfig)

Полный шаблон для динамических подсказок с дополнительными свойствами только для кода.

**Свойства (в дополнение к DynamicHintConfig):**

| Свойство | Тип | Сериализуемо | Описание |
|----------|-----|--------------|----------|
| Id | `string?` | Да | Логический идентификатор |
| Hide | `bool?` | Да | Видимость |
| AutoText | `AutoContent.TextUpdateHandler?` | Нет | Обработчик динамического текста |
| Content | `AbstractHintContent?` | Нет | Провайдер содержимого |
| FontSizeTransition | `Transition?` | Нет | Анимация размера шрифта |

---

### Конфигурации только позиции

Эти облегчённые классы конфигурации содержат только свойства, связанные с позицией. Они **не** наследуются от `AbstractHintTemplate`.

**HintPositionConfig:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| XCoordinate | `float?` | Горизонтальное смещение |
| YCoordinate | `float?` | Вертикальная позиция |
| Alignment | `HintAlignment?` | Выравнивание текста |
| YCoordinateAlign | `HintVerticalAlign?` | Как координата Y выравнивается относительно текста |

Методы: `Apply(Hint)`, `GetHint()`

**DynamicHintPositionConfig:**

| Свойство | Тип | Описание |
|----------|-----|----------|
| TopBoundary | `float?` | Верхняя граница |
| BottomBoundary | `float?` | Нижняя граница |
| LeftBoundary | `float?` | Левая граница |
| RightBoundary | `float?` | Правая граница |
| TargetX | `float?` | Предпочтительная горизонтальная позиция |
| TargetY | `float?` | Предпочтительная вертикальная позиция |
| TopMargin | `float?` | Дополнительное пространство сверху |
| BottomMargin | `float?` | Дополнительное пространство снизу |
| LeftMargin | `float?` | Дополнительное пространство слева |
| RightMargin | `float?` | Дополнительное пространство справа |

Методы: `Apply(DynamicHint)`, `GetHint()`

Нажмите [здесь](/Docs/Russian/README.md), чтобы вернуться к README
