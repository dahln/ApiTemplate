using System.Collections.Generic;

namespace ApiTemplate.Common;

public class Search
{
    public string? FilterText { get; set; } = null;
    public int? PageNumber { get; set; } = null;
    public int? PageSize { get; set; } = null;
    public string? SortBy { get; set; } = null;
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
}

public enum SortDirection
{
    Descending = -1,
    Ascending = 1
}

public class SearchResponse<T>
{
    /// <summary>
    /// The total number of results, before paging is applied.
    /// </summary>
    public int TotalResults { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public List<T> Results { get; set; } = new List<T>();
}




