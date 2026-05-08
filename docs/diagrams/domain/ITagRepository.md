```mermaid
classDiagram
    interface ITagRepository {
        +GetById(id: int) Result~Tag~
        +GetByName(name: string) Result~Tag~
        +Add(tag: Tag) ResultVoid
        +Delete(id: int) Result~Tag~
    }
```