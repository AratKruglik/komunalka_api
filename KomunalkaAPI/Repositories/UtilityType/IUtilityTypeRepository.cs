namespace KomunalkaAPI.Repositories.UtilityType;

public interface IUtilityTypeRepository : IRepository<Models.UtilityType>
{
    Task<IEnumerable<Models.UtilityType>> GetActiveAsync();
}
