# Диаграмма прецендентов

```mermaid
graph LR
    User((Пользователь))
    FS[Файловая система]

    subgraph "Управление физическим хранилищем"
        UC_Import(Импортировать фото в систему)
        UC_PhysDel(Удалить файл с диска)
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

