```mermaid
classDiagram
    class Tag {
        -int Id
        -string Name
        -Tag()
        -Tag(name: string)
        +static Create(name: string) Result~Tag~
    }
```