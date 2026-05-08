```mermaid
classDiagram
    class IUnitOfWork {
<<interface>> 
        +BeginTransaction() ResultVoid
        +Commit() ResultVoid
        +Rollback() ResultVoid
        +Dispose() void
    }
```
