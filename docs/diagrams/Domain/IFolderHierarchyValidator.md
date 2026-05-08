```mermaid
classDiagram
    interface IFolderHierarchyValidator {
        +CheckForCycles(sourceFolderId: int, targetFolderId: int) ResultVoid
    }
```