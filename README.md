# DomainPcInfo

## Для чего

**DomainPcInfo** — небольшой информационный виджет для Windows.

Программа показывает:

- имя компьютера;
- домен или рабочую группу;
- текущий контроллер домена (DC);
- доступность DC по ICMP Ping;
- активные физические сетевые интерфейсы и их IPv4-адреса.

Информация обновляется при запуске, автоматически каждые 5 минут и вручную кнопкой `↻`.

Если компьютер не состоит в домене, отображается рабочая группа, а `Доступ DC` показывает `Недоступно`.

## Структура проекта

```text
DomainPcInfo/
├─ src/
│  └─ DomainPcInfo/
├─ installer/
│  └─ DomainPcInfo.Setup/
├─ scripts/
├─ docs/
├─ .github/
│  └─ workflows/
├─ DomainPcInfo.sln
├─ README.md
└─ .gitignore
```

Скрипты сборки используют пути относительно корня репозитория, поэтому проект можно клонировать в любой локальный каталог.

## Требования для сборки

- Windows 10/11;
- .NET 10 SDK;
- PowerShell;
- Git;
- доступ к NuGet при первой сборке WiX Toolset SDK.

Проверка SDK:

```powershell
dotnet --list-sdks
```

## Получение проекта

```powershell
git clone https://github.com/ilislava2/DomainPcInfo.git
cd DomainPcInfo
```

## Запуск из исходников

Из корня репозитория:

```powershell
dotnet run --project .\src\DomainPcInfo\DomainPcInfo.csproj
```

или:

```powershell
.\scripts\run.ps1
```

## Сборка MSI

Из корня репозитория:

```powershell
.\scripts\build-msi.ps1
```

Готовый MSI создаётся в:

```text
installer\DomainPcInfo.Setup\bin\Release\
```

## Установка

Пример:

```powershell
msiexec /i ".\installer\DomainPcInfo.Setup\bin\Release\DomainPcInfo-x64-1.0.0.msi"
```

Программа устанавливается в:

```text
C:\Program Files\DomainPcInfo\
```

После установки:

- приложение добавляется в автозапуск;
- в меню **Пуск → DomainPcInfo** появляется ярлык запуска;
- там же появляется ярлык **Удалить DomainPcInfo**.

## Установка через GPO

MSI следует разместить в сетевой папке, доступной компьютерам домена, например:

```text
\\SERVER\Software\DomainPcInfo\DomainPcInfo-x64-1.0.0.msi
```

В Group Policy Management:

```text
Computer Configuration
→ Policies
→ Software Settings
→ Software installation
→ New
→ Package
```

Указать UNC-путь к MSI и выбрать `Assigned`.

## Остановка

При запуске через `dotnet run`:

```text
Ctrl + C
```

Для установленной версии:

```powershell
Get-Process DomainPcInfo -ErrorAction SilentlyContinue | Stop-Process
```

## Удаление

Через меню Пуск:

```text
Пуск
→ DomainPcInfo
→ Удалить DomainPcInfo
```

Или через Windows:

```text
Параметры
→ Приложения
→ Установленные приложения
→ DomainPcInfo
→ Удалить
```

Или командой:

```powershell
msiexec /x ".\installer\DomainPcInfo.Setup\bin\Release\DomainPcInfo-x64-1.0.0.msi"
```

## Работа с Git

Короткая памятка для работы с проектом на нескольких компьютерах находится в:

```text
docs\GIT_WORKFLOW.md
```

Памятка по выпуску версий:

```text
docs\RELEASE.md
```
