````mermaid
classDiagram
    class DimensionsTypeHandler {
        <<SqlMapper.TypeHandler~Dimensions~>>
        +SetValue(IDbDataParameter parameter, Dimensions? value) void
        +Parse(object value) Dimensions?
    }

    class Dimensions {
        <<record>>
        +int Width
        +int Height
        +static Create(int width, int height) Result~Dimensions~
    }

    class IDbDataParameter {
        <<interface>>
        +object? Value
    }

    class SqlMapper_TypeHandler~T~ {
        <<generic abstract>>
        +SetValue(IDbDataParameter, T) void*
        +Parse(object) T*
    }

    DimensionsTypeHandler --|> SqlMapper_TypeHandler~Dimensions~ : наследует
    DimensionsTypeHandler --> Dimensions : преобразует
    DimensionsTypeHandler --> IDbDataParameter : использует
````