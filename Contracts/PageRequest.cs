namespace Contracts;

/// <summary>Standard paging parameters for queries that return collections.</summary>
public sealed record PageRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => (Page - 1) * PageSize;

    public static PageRequest Default => new();
}