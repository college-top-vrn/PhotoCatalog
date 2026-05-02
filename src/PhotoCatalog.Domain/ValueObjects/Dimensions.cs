using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.ValueObjects;



public record Dimensions
{
    private const int MaxWidth = 3840;
    private const int MaxHeight = 2160;
    private const int MinValues = 1;
    public int Width { get; init; }
    public int Height { get; init; }

    private Dimensions(int width, int height)
    {   
        Width = width;
        Height = height;
    }

    public static Result<Dimensions> Create(int width, int height)
    {
        if ((width >= MinValues && width <= MaxWidth) || (height >= MinValues  && height <= MaxHeight))
        {
            return Result<Dimensions>.Success(new Dimensions(width, height));
        }

        return Result<Dimensions>.Failure(new Error("Dimensions.Invalid", "Ширина и высота должны быть больше нуля")); // TODO Добавить ошибку при отрицательной длины ширены
    }
};