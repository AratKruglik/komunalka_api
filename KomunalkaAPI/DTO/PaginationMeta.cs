namespace KomunalkaAPI.DTO;

public class PaginationMeta
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public int Returned { get; set; }
    public long Total { get; set; }
}