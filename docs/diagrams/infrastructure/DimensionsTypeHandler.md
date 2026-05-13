````mermaid
classDiagram
    class DimensionsTypeHandler {
        <<SqlMapper.TypeHandler~Dimensions~>>
        +SetValue(IDbDataParameter parameter, Dimensions? value) void
        +Parse(object value) Dimensions?
    }
````
