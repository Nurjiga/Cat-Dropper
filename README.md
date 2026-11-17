# Cat Dropper

Игра про слияние котов в стиле Suika, сделана в Unity для iPhone. Учебный проект AITU, группа MT-2401, Assignment 4.

Кидай котов в стакан: два одинаковых сливаются в более крупного. Дойди до Legendary-кота и получи двойные очки. Не дай котам подняться выше пунктирной линии.

## Как скачать и запустить на другом компьютере

### 1. Что нужно установить
- [Unity Hub](https://unity.com/download).
- Редактор **Unity 6000.6.7f1** (именно эта версия). В Unity Hub: *Installs → Install Editor → Archive → Unity 6* и найди `6000.6.7f1`. Для сборки на iPhone поставь вместе с модулем **iOS Build Support**.
- Для запуска на iPhone дополнительно нужны Mac и Xcode. Для игры внутри Unity (кнопка ▶) они не нужны.

### 2. Скачать проект
Вариант А — через терминал (нужен Git):
```bash
git clone https://github.com/Nurjiga/Cat-Dropper.git
```
Вариант Б — без Git: на странице репозитория нажми **Code → Download ZIP** и распакуй архив.

> Скачивай репозиторий целиком. Одной папки `Assets` мало: рядом нужны `Packages` (пакеты Unity) и `ProjectSettings` (настройки проекта).

### 3. Открыть в Unity
1. Unity Hub → **Add → Add project from disk**.
2. Выбери папку **`Cat Dropper`** внутри скачанного репозитория (в ней лежат `Assets`, `Packages`, `ProjectSettings`).
3. Открой проект версией **6000.6.7f1**.
4. Подожди: при первом открытии Unity скачивает пакеты и собирает кэш (`Library`), это занимает несколько минут. Нужен интернет.

### 4. Запустить игру
1. В окне Project открой сцену `Assets/Scenes/Main Menu.unity`.
2. Нажми кнопку **▶ Play** вверху. Первый клик по окну *Game* только передаёт ему фокус.
3. Для размера экрана выбери в окне Game, например, **iPhone 12 Pro Max**.

## Что где лежит
| Папка | Что внутри |
|---|---|
| `Cat Dropper/Assets/Scenes` | Две сцены: `Main Menu` (первая) и `SampleScene` (игра) |
| `Cat Dropper/Assets/Scripts` | Скрипты игры (`GameManager`, `Cat`, `DeadZone`, `SaveData`, `AudioManager`, `TutorialModal`, `SettingsPanel` и др.) |
| `Cat Dropper/Assets/Editor` | Инструменты в меню **Tools → Cat Dropper** (сборка Settings и окон инструкции, стиль кнопок, сброс прогресса) |
| `Cat Dropper/Assets/Resources/Audio` | Звуки и музыка |
| `Cat Dropper/Assets/Sprites`, `Prefabs` | Картинки, коты (`cat_0 … cat_7`) |
| `Cat Dropper/Tools` | `generate_sounds.py` — синтез звуков (нужны Python и numpy) |

## Сборка на iPhone
1. В Unity: **File → Build Profiles → iOS → Build**, выбери пустую папку.
2. Открой `Unity-iPhone.xcodeproj` в Xcode.
3. *Signing & Capabilities* → включи *Automatically manage signing* и выбери свою Team (Apple ID).
4. Подключи iPhone, выбери его вверху и нажми ▶. При первом запуске на телефоне: Настройки → Основные → VPN и управление устройством → доверять разработчику.

## Если что-то пошло не так
- **Много красных ошибок в консоли после открытия:** проверь версию Unity (нужна `6000.6.7f1`) и что был интернет при первом открытии. Закрой Unity, удали папку `Library` рядом с `Assets` и открой проект снова.
- **Проект не появляется в Unity Hub:** добавляй именно папку `Cat Dropper` (внутри репозитория), а не корень репозитория.
- **Запускается пустая сцена:** открой `Assets/Scenes/Main Menu.unity` двойным кликом.
- **Сбросить прогресс** (рекорд, открытых котов, инструкцию): **Tools → Cat Dropper → Reset Progress (PlayerPrefs)**.

## Сохранения (PlayerPrefs)
`TutorialSeen`, `BestScore`, `CatUnlocked_0…7`, `SoundOn`, `MusicOn`.
