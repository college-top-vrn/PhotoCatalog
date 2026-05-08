```mermaid
classDiagram
    class IAlbumRepository {
        <<interface>>
        +GetById(int id) Result~Album~
        +Add(Album album) ResultVoid
        +Update(Album album) ResultVoid
        +Delete(int id) ResultVoid
    }
```