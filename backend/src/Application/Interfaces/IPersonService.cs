using Application.DTOs;

namespace Application.Interfaces;

public interface IPersonService
{
    Task<IEnumerable<PersonResponse>> GetAllAsync();
    Task<PersonResponse> CreateAsync(CreatePersonRequest request);
    Task DeleteAsync(Guid id);
}
