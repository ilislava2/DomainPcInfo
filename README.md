# DomainPcInfo

## Для чего

**DomainPcInfo** — небольшой информационный виджет для Windows, предназначенный для рабочих станций в доменной и недоменной среде.

Программа показывает:

- имя компьютера;
- имя домена или рабочей группы;
- текущий контроллер домена (DC);
- доступность DC по ICMP Ping;
- активные физические сетевые интерфейсы и их IPv4-адреса.

Проверка информации выполняется при запуске, далее автоматически каждые 5 минут. Также доступно ручное обновление кнопкой `↻`.

Если компьютер не состоит в домене, вместо домена отображается рабочая группа, а состояние DC выводится как `Недоступно`.

Приложение запускается в пользовательской сессии Windows и отображается в правом верхнем углу рабочего стола.

---

## Структура проекта

Рабочий каталог проекта:

```text
D:\DomainPcInfo
```

Резервный/финальный каталог:

```text
E:\04_Хобби\Программирование\DomainPcInfo
```

Основная структура репозитория:

```text
DomainPcInfo/
├─ src/
│  └─ DomainPcInfo/
│     ├─ DomainPcInfo.csproj
│     ├─ App.xaml
│     ├─ App.xaml.cs
│     ├─ MainWindow.xaml
│     ├─ MainWindow.xaml.cs
│     ├─ SystemInfo.cs
│     ├─ NetworkInfo.cs
│     ├─ DomainInfo.cs
│     └─ AppLog.cs
│
├─ installer/
│  └─ DomainPcInfo.Setup/
│     ├─ DomainPcInfo.Setup.wixproj
│     └─ Package.wxs
│
├─ scripts/
│  ├─ run.ps1
│  ├─ build.ps1
│  ├─ publish.ps1
│  └─ build-msi.ps1
│
├─ .github/
│  └─ workflows/
│
├─ artifacts/
│  └─ publish/
│
├─ DomainPcInfo.sln
├─ README.md
└─ .gitignore
```

### Основные файлы

`MainWindow.xaml`  
Оформление информационного виджета.

`MainWindow.xaml.cs`  
Обновление данных, таймер 5 минут, ручное обновление и позиционирование окна.

`SystemInfo.cs`  
Получение имени ПК, домена и рабочей группы.

`DomainInfo.cs`  
Определение контроллера домена и проверка его доступности.

`NetworkInfo.cs`  
Определение активных физических сетевых интерфейсов и IPv4-адресов. Виртуальные интерфейсы не отображаются.

`AppLog.cs`  
Заглушка под логирование. В текущей версии отдельное файловое логирование не используется.

`Package.wxs`  
Описание MSI-пакета, пути установки и автозапуска.

`build-msi.ps1`  
Публикация приложения и сборка MSI.

---

## Установка

### Требования для сборки

Для разработки и локальной сборки требуется:

- Windows 10/11;
- .NET 10 SDK;
- PowerShell;
- доступ к NuGet для первой загрузки WiX Toolset SDK.

Проверка установленного SDK:

```powershell
dotnet --list-sdks
```

### Сборка MSI

Перейти в каталог проекта:

```powershell
cd D:\DomainPcInfo
```

Запустить:

```powershell
.\scripts\build-msi.ps1
```

После успешной сборки MSI будет находиться примерно здесь:

```text
D:\DomainPcInfo\installer\DomainPcInfo.Setup\bin\Release\DomainPcInfo-x64-<version>.msi
```

Например:

```text
DomainPcInfo-x64-1.0.0.msi
```

### Локальная установка MSI

Запустить PowerShell или командную строку от имени администратора:

```powershell
msiexec /i "D:\DomainPcInfo\installer\DomainPcInfo.Setup\bin\Release\DomainPcInfo-x64-1.0.0.msi"
```

Программа устанавливается в:

```text
C:\Program Files\DomainPcInfo\
```

Автозапуск создаётся в:

```text
HKLM\Software\Microsoft\Windows\CurrentVersion\Run
```

Значение:

```text
DomainPcInfo
```

После следующего входа пользователя в Windows программа запустится автоматически.

### Установка через GPO

MSI необходимо разместить в сетевой папке, доступной компьютерам домена, например:

```text
\\SERVER\Software\DomainPcInfo\DomainPcInfo-x64-1.0.0.msi
```

В Group Policy Management:

```text
Computer Configuration
  → Policies
    → Software Settings
      → Software installation
```

Создать новый пакет:

```text
New → Package
```

Указать UNC-путь к MSI:

```text
\\SERVER\Software\DomainPcInfo\DomainPcInfo-x64-1.0.0.msi
```

Тип развертывания:

```text
Assigned
```

Для тестирования политики на клиенте:

```powershell
gpupdate /force
```

Для установки через `Computer Configuration` обычно требуется перезагрузка рабочей станции.

---

## Запуск

### Запуск из исходников

```powershell
cd D:\DomainPcInfo
dotnet run --project .\src\DomainPcInfo\DomainPcInfo.csproj
```

### Запуск установленной версии

```powershell
& "C:\Program Files\DomainPcInfo\DomainPcInfo.exe"
```

### Автоматический запуск

После установки MSI приложение запускается автоматически при входе пользователя в Windows.

Приложение:

- выполняет полное обновление при запуске;
- автоматически обновляет данные каждые 5 минут;
- позволяет выполнить ручное обновление кнопкой `↻`.

---

## Остановка

Если программа запущена через `dotnet run`, в том же окне PowerShell нажать:

```text
Ctrl + C
```

Для остановки установленной версии:

```powershell
Get-Process DomainPcInfo -ErrorAction SilentlyContinue | Stop-Process
```

Для принудительной остановки:

```powershell
Get-Process DomainPcInfo -ErrorAction SilentlyContinue | Stop-Process -Force
```

Также процесс можно завершить через Диспетчер задач Windows.

---

## Удаление

### Через MSI

Остановить программу:

```powershell
Get-Process DomainPcInfo -ErrorAction SilentlyContinue | Stop-Process -Force
```

Запустить удаление:

```powershell
msiexec /x "D:\DomainPcInfo\installer\DomainPcInfo.Setup\bin\Release\DomainPcInfo-x64-1.0.0.msi"
```

### Через Windows

Также приложение можно удалить через:

```text
Параметры Windows
→ Приложения
→ Установленные приложения
→ DomainPcInfo
→ Удалить
```

После удаления должны быть удалены:

```text
C:\Program Files\DomainPcInfo\
```

и значение автозапуска:

```text
HKLM\Software\Microsoft\Windows\CurrentVersion\Run\DomainPcInfo
```

---

## Обновление версии

Для новой версии изменяется `ProductVersion` в:

```text
installer\DomainPcInfo.Setup\DomainPcInfo.Setup.wixproj
```

Пример:

```xml
<ProductVersion>1.0.1</ProductVersion>
```

`UpgradeCode` в `Package.wxs` должен оставаться постоянным для всех версий DomainPcInfo.

После изменения версии выполнить:

```powershell
.\scripts\build-msi.ps1
```

Новая версия MSI должна обновлять ранее установленную версию программы.
