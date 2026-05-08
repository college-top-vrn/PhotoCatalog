```mermaid
classDiagram
    interface IPhotoRepository {
        +GetById(id: int) Result~Photo~
        +GetByPath(realPath: string) Result~Photo~
        +Add(photo: Photo) ResultVoid
        +Update(photo: Photo) ResultVoid
        +Delete(id: int) ResultVoid
        +GetByAlbumId(albumId: int) Result~IReadOnlyCollection~Photo~~
        +GetByTags(tagIds: IEnumerable~int~) Result~IReadOnlyCollection~Photo~~
    }
```