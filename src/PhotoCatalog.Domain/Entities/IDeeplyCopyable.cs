namespace PhotoCatalog.Domain.Entities;

/// <summary>
/// Представляет механизм для создания глубокого копирования. 
/// </summary>
/// <typeparam name="T">Тип доменной сущности, наследуемый от <see cref="Entity"/>.</typeparam>
public interface IDeeplyCopyable<out T> where T: Entity
{
    /// <summary>
    /// Создаёт новый экземпляр текущего объекта, являющийся его глубокой копией.
    /// </summary>
    /// <returns>глубокую копию текущего объекта типа.</returns>
    T DeepCopy();
}