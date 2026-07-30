// using PhotoCatalog.Application.Errors;
// using PhotoCatalog.Domain.Entities;
// using PhotoCatalog.Domain.Extensions;
// using PhotoCatalog.Domain.Interfaces.Repositories;
// using PhotoCatalog.Domain.Interfaces.Services;
// using PhotoCatalog.Domain.Primitives;
//
// using Serilog;
//
// namespace PhotoCatalog.Application.UseCases;
//
// /// <summary>
// ///     Сценарий использования для добавления фотографии в существующий альбом.
// /// </summary>
// public class AddPhotoToAlbumUseCase(
//     IAlbumQueryRepository albumQueryRepository,
//     IAlbumCommandRepository albumCommandRepository,
//     IPhotoQueryRepository photoQueryRepository,
//     IUnitOfWork unitOfWork,
//     ILogger logger)
// {
//     private readonly ILogger _logger = logger.ForContext<AddPhotoToAlbumUseCase>();
//
//     /// <summary>
//     ///     Выполняет добавление фотографии в альбом с сохранением изменений в рамках транзакции.
//     /// </summary>
//     /// <param name="albumId">Идентификатор альбома.</param>
//     /// <param name="photoId">Идентификатор фотографии.</param>
//     /// <returns>Результат выполнения операции (успех или ошибка).</returns>
//     public ResultVoid Execute(int albumId, int photoId)
//     {
//         Result<Album>? albumEntity = null;
//         _logger.Information("Запуск процесса добавления фото {PhotoId} в альбом {AlbumId}", photoId, albumId);
//         return photoQueryRepository.GetById(photoId)
//             .OnSuccess(_ =>
//                 _logger.Information("Фото {PhotoId} найдено", photoId))
//             .OnFailure(_ =>
//                 _logger.Warning("Фото {PhotoId} не найдено", photoId))
//             .Then(_ =>
//             {
//                 albumEntity = albumQueryRepository.GetById(albumId);
//                 return albumEntity;
//             })
//             .OnSuccess(_ =>
//                 _logger.Information("Альбом {AlbumId} найден", albumId))
//             .OnFailure(_ =>
//                 _logger.Warning("Альбом {AlbumId} не найден", albumId))
//             .Then(album => album.AddPhoto(photoId))
//             .ToResult()
//             .OnFailure(error =>
//                 _logger.Warning("Не удалось добавить фото {PhotoId} в альбом {AlbumId}: {Error}", photoId, albumId,
//                     error))
//             .Transform(_ => unitOfWork.BeginTransaction())
//             .Ensure(beginResult => beginResult.IsSuccess,
//                 ApplicationErrors.Transactions.StartTransactions)
//             .Transform(_ => albumCommandRepository.Update(albumEntity!.Value!))
//             .Ensure(updateResult => updateResult.IsSuccess,
//                 ApplicationErrors.Albums.UpdateFailed)
//             .Transform(_ => unitOfWork.Commit())
//             .Ensure(commitResult => commitResult.IsSuccess,
//                 ApplicationErrors.Transactions.CommitFailed)
//             .Finally(_ => ResultVoid.Success(),
//                 _ => ResultVoid.Failure(ApplicationErrors.UseCases.SystemFailure));
//     }
// }