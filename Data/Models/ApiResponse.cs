namespace Data.Models;

public class ApiResponse
{
    public bool Succeeded { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Result { get; set; }
}
