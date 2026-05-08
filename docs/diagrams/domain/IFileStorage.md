```mermaid
classDiagram
    interface IFileStorage {
        +StoreFile(sourcePath: string, newFileName: string) Result~string~
        +DeleteFile(filePath: string) ResultVoid
        +FileExists(filePath: string) Result~bool~
    }
```