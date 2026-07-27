using System.Collections.Generic;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalog.Application.Interfaces.Repositories;

public interface IQueryRepository
{
    List<Result<TV>> GetBy<TP, TV>(TP property);
}