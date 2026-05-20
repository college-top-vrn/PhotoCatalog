
* [Конвенции и правила проекта](docs/rules/Сonvention.md)
* [Техническая спецификация проекта](docs/docfx/docs/TechnicalSpecification.md)


# Диаграмма прецендентов

```mermaid
graph LR
    User((Пользователь))
    FS[Файловая система]

    subgraph "Управление физическим хранилищем"
        UC_Import(Импортировать фото в систему)
        UC_PhysDel(Удалить файл из cистемы)
    end

    subgraph "Управление виртуальной структурой"
        UC_CreateFolder(Создать виртуальную папку)
        UC_CreateAlbum(Создать виртуальный альбом)
        UC_Nest(Вложить альбом в папку)
        UC_Link(Привязать фото к альбому)
    end

    subgraph "Организация и поиск"
        UC_Tag(Добавить теги к фото)
        UC_Sort(Сортировать коллекцию)
        UC_Filter(Фильтровать по тегам)
    end

    %% Связи пользователя с прецедентами
    User --> UC_Import
    User --> UC_CreateFolder
    User --> UC_CreateAlbum
    User --> UC_Link
    User --> UC_Tag
    User --> UC_Filter

    %% Внутренние зависимости (Include/Extend)
    UC_Link -.->|include| UC_CreateAlbum
    UC_Nest -.->|extend| UC_CreateFolder
    UC_Filter -.->|include| UC_Sort

    %% Взаимодействие с системой
    UC_Import --- FS
    UC_PhysDel --- FS
```

# Диаграмма компонентов

```mermaid 
graph TD
    subgraph "Infrastructure Layer (Внешний слой)"
        Errors
        Extensions
        Fakes
        Handlers
        Repositories
        Services
        SqlScripts
        UnitOfWork
    end

    subgraph "Application Layer"
        direction TB
        UseCases
        UC_Search[Search & Tagging Use Cases]
        ApplicationFakes[Fakes]
        ApplicationErrors[Errors]
        DTOs
        Caching
    end

    subgraph "Domain Layer"
        direction TB
        Primitives
        Entities
        Extensions
        ValueObjects
        subgraph "Interfaces"
            Repositories
            Services         
            
        end
    end

    
```
У наc не доделан Search & Tagging Use Cases

# Технологический стек проекта

Данный проект базируется на стеке технологий .NET.

## Основная платформа
*   **Язык программирования:** C# 13
*   **Runtime:** .NET 10

## База данных и ORM
*   **СУБД:** SQLite — легковесная встраиваемая реляционная база данных.
*   **ORM:** Dapper — используется для маппинга доменных моделей на таблицы БД.

## Тестирование
*   **Unit-тестирование:** xUnit — основной фреймворк для написания и запуска автоматических тестов.
*   **Архитектурные тесты:** ArchUnitNET — используется для контроля соблюдения правил архитектуры.

## Логирование и диагностика
*   **Логгирование:** Serilog — библиотека для структурированного логирования. Позволяет сохранять события не просто как текст, а в виде структурированных данных, что упрощает отладку и мониторинг состояния системы.
