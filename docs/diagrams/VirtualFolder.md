```mermaid
classDiagram
    namespace Entities {
        class Folder {
            +Id int [get; -set]
            +ParentFolderId int? [get; -set]
            +Name string [get; -set]
            -Folder() void
            +Create(int id, string name) Folder$
            +Rename(string newName) ResultVoid
            +MoveTo(Folder parentFolder) ResultVoid
            +MoveToRoot() ResultVoid
        }        
    }

```