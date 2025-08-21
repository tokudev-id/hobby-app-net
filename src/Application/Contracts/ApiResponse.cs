namespace HobbyApp.Application.Contracts;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string Status { get; set; } = "success";
    public string? Error { get; set; }
    public PaginationMeta? Meta { get; set; }

    public static ApiResponse<T> Success(T data) =>
        new() { Data = data, Status = "success" };

    public static ApiResponse<T> Success(T data, PaginationMeta meta) =>
        new() { Data = data, Status = "success", Meta = meta };

    public static ApiResponse<T> Fail(string error) =>
        new() { Status = "error", Error = error };
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Size);
}

