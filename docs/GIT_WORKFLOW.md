# Работа с DomainPcInfo на двух компьютерах

Репозиторий является единой точкой обмена между домашним и рабочим ПК. Копировать проект вручную между компьютерами не нужно.

## Первый запуск на втором ПК

Установить Git и .NET 10 SDK, затем выбрать удобную папку для проектов:

```powershell
git clone https://github.com/ilislava2/DomainPcInfo.git
cd DomainPcInfo
```

Проверить:

```powershell
git status
dotnet --list-sdks
```

## Перед началом работы на любом ПК

Сначала получить изменения, сделанные на другом компьютере:

```powershell
git pull --ff-only
```

После этого редактировать проект.

## После работы

Проверить изменения:

```powershell
git status
git diff
```

Сохранить и отправить:

```powershell
git add .
git commit -m "Краткое описание изменений"
git push
```

## Обычный цикл

```text
Домашний ПК:
git pull → работа → commit → push

Рабочий ПК:
git pull → работа → commit → push

Домашний ПК:
git pull → ...
```

Главное правило: **перед началом работы на другом компьютере делать `git pull --ff-only`, а после законченной работы — `git push`.**

## Настройка Git на новом ПК

Проверить имя и email:

```powershell
git config --global user.name
git config --global user.email
```

Если они не заданы:

```powershell
git config --global user.name "Ваше имя"
git config --global user.email "ваш-email-для-git"
```

Для отправки изменений GitHub при первом `git push` может запросить вход через Git Credential Manager.
