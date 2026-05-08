```mermaid
classDiagram
    class Error {
        <<record>>
        +string Code
        +string Message
        +static None Error
        +Error(string Code, string Message)
    }
```