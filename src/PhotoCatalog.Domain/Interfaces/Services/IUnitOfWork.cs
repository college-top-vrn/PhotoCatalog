using System;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Domain.Interfaces.Services;

/// <summary>
/// Контракт для управления транзакциями в доменном слое.
/// </summary>
/// <remarks>
/// Очищен от инфраструктурных типов для независимости слоя Domain.
/// </remarks>
interface IUnitOfWork : IDisposable
{
    /// <summary>Начинает новую транзакцию.</summary>
    /// <returns>Результат операции. Успех или ошибка с детализацией.</returns>
    ResultVoid BeginTransaction();

    /// <summary>Фиксирует все изменения текущей транзакции.</summary>
    /// <returns>Результат операции. Успех или ошибка с детализацией.</returns>
    ResultVoid Commit();

    /// <summary>Отменяет все изменения текущей транзакции.</summary>
    /// <returns>Результат операции. Успех или ошибка с детализацией.</returns>
    ResultVoid Rollback();
}