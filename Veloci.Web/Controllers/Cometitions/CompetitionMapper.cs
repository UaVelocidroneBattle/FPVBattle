﻿using Riok.Mapperly.Abstractions;
using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Cometitions;

[Mapper]
public static partial class CompetitionMapper
{
    public static partial CompetitionModel MapToModel(Competition competition);
    public static partial IQueryable<CompetitionModel> ProjectToModel(this IQueryable<Competition> competitions);
}
