```mermaid
classDiagram
    interface IUnitOfWork {
        +BeginTransaction() ResultVoid
        +Commit() ResultVoid
        +Rollback() ResultVoid
        +Dispose() void
    }
```