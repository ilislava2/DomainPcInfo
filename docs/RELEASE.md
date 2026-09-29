# DomainPcInfo — выпуск новой версии

## 1. Проверить проект локально

```powershell
cd D:\DomainPcInfo
.\scripts\build-msi.ps1
```

Убедиться, что MSI собрался без ошибок.

## 2. Сохранить изменения в Git

```powershell
git add .
git commit -m "Описание изменений"
git push
```

## 3. Создать тег новой версии

Пример:

```powershell
git tag v1.0.1
```

Формат версии:

```text
v1.0.0
v1.0.1
v1.1.0
```

## 4. Отправить тег на GitHub

```powershell
git push origin v1.0.1
```

## 5. Что произойдёт автоматически

GitHub Actions:

- соберёт приложение;
- соберёт MSI;
- возьмёт версию из тега;
- создаст GitHub Release;
- прикрепит MSI к Release.

Пример итогового файла:

```text
DomainPcInfo-x64-1.0.1.msi
```

## 6. Где проверить сборку

На GitHub:

```text
Actions
→ Release DomainPcInfo MSI
```

## 7. Где взять готовый MSI

На GitHub:

```text
Releases
→ v1.0.1
```

## Важно

- Тег создавать только после `git push`.
- Формат тега должен быть `vX.Y.Z`.
- Старые теги не переиспользовать.
- Не переносить уже опубликованный тег на другой commit.
- Для каждой новой версии увеличивать номер.

## Быстрая памятка

Обычные изменения:

```powershell
git add .
git commit -m "Описание"
git push
```

Новый релиз:

```powershell
git tag v1.0.1
git push origin v1.0.1
```
