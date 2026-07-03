using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketing.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount);