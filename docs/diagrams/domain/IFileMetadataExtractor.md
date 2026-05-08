```mermaid
classDiagram
    interface IFileMetadataExtractor {
        +CalculateHash(filePath: string) Result~string~
        +GetDimensions(filePath: string) Result~Dimensions~
    }
```