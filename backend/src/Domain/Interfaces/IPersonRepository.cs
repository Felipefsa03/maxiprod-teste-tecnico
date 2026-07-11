using Domain.Entities;

namespace Domain.Interfaces;

public interface IPersonRepository
{
    Task<IEnumerable<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(Guid id);
    Task<Person> AddAsync(Person person);
    Task DeleteAsync(Person person);
    Task<bool> ExistsAsync(Guid id);
}
