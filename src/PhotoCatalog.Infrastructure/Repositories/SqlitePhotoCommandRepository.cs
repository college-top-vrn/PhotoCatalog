using System;
using System.Collections.Generic;
using System.Linq;

using Dapper;

using Microsoft.Data.Sqlite;

using PhotoCatalog.Domain.Entities;
using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Infrastructure.Errors;
using PhotoCatalog.Infrastructure.UnitOfWork;

using Serilog;

namespace PhotoCatalog.Infrastructure.Repositories;

/// <summary>
///     Реализация репозитория для работы с записью фотографий в SQLite с использованием Dapper.
/// </summary>
public class SqlitePhotoCommandRepository : IPhotoCommandRepository
{
    private readonly SqliteUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    /// <summary>
    ///     Инициализирует новый экземпляр репозитория фотографий.
    /// </summary>
    /// <param name="unitOfWork">Экземпляр Unit of Work для доступа к соединению и транзакции.</param>
    /// <param name="logger">Логгер для записи ошибок.</param>
    public SqlitePhotoCommandRepository(SqliteUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }


    /// <inheritdoc />
    public ResultVoid Add(Photo photo)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var dimensionsValue = $"{photo.Dimensions.Width}x{photo.Dimensions.Height}";

            connection.Execute(
                "INSERT INTO Photos (Id, RealPath, FileHash, Dimensions, AddedAt) VALUES (@Id, @RealPath, @FileHash, @Dimensions, @AddedAt)",
                new
                {
                    photo.Id,
                    photo.RealPath,
                    photo.FileHash,
                    Dimensions = dimensionsValue,
                    photo.AddedAt
                },
                _unitOfWork.Transaction);

            var tagValues = photo.TagIds.Select(tagId => new { PhotoId = photo.Id, TagId = tagId });

            connection.Execute(
                "INSERT INTO PhotoTags (PhotoId, TagId) VALUES (@PhotoId, @TagId)",
                tagValues,
                _unitOfWork.Transaction);


            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger.Error(ex, "Нарушение уникальности RealPath для фотографии {RealPath}", photo.RealPath);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Add для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Add для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }


    /// <inheritdoc />
    public ResultVoid Update(Photo photo)
    {
        if (photo == null)
        {
            return ResultVoid.Failure(DomainErrors.Photo.NullPhoto);
        }

        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            var dimensionsValue = $"{photo.Dimensions.Width}x{photo.Dimensions.Height}";

            var rowsAffected = connection.Execute(
                "UPDATE Photos SET RealPath = @RealPath, FileHash = @FileHash, Dimensions = @Dimensions, AddedAt = @AddedAt WHERE Id = @Id",
                new
                {
                    photo.Id,
                    photo.RealPath,
                    photo.FileHash,
                    Dimensions = dimensionsValue,
                    photo.AddedAt
                },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                return ResultVoid.Failure(DomainErrors.Photo.NotFound);
            }

            connection.Execute(
                "DELETE FROM PhotoTags WHERE PhotoId = @PhotoId",
                new { PhotoId = photo.Id },
                _unitOfWork.Transaction);

            var tagValues = photo.TagIds.Select(tagId => new { PhotoId = photo.Id, TagId = tagId });

            connection.Execute(
                "INSERT INTO PhotoTags (PhotoId, TagId) VALUES (@PhotoId, @TagId)",
                tagValues,
                _unitOfWork.Transaction);

            return ResultVoid.Success();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger.Error(ex, "Нарушение уникальности RealPath для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConstraintViolation);
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Update для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Update для фотографии {Id}", photo.Id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }


    /// <inheritdoc />
    public ResultVoid Delete(int id)
    {
        try
        {
            var connection = _unitOfWork.Connection;
            if (connection == null)
            {
                return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
            }

            connection.Execute(
                "DELETE FROM PhotoTags WHERE PhotoId = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            var rowsAffected = connection.Execute(
                "DELETE FROM Photos WHERE Id = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (rowsAffected == 0)
            {
                return ResultVoid.Failure(DomainErrors.Photo.NotFound);
            }

            return ResultVoid.Success();
        }
        catch (SqliteException ex)
        {
            _logger.Error(ex, "Ошибка SQLite в методе Delete для фотографии {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Неожиданная ошибка в методе Delete для фотографии {Id}", id);
            return ResultVoid.Failure(InfrastructureErrors.Database.ConnectionFailed);
        }
    }
}