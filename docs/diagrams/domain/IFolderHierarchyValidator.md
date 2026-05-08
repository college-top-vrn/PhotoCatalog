```mermaid
classDiagram
    class IFolderHierarchyValidator {
<<interface>> 
        +CheckForCycles(sourceFolderId: int, targetFolderId: int) ResultVoid
    }
```
