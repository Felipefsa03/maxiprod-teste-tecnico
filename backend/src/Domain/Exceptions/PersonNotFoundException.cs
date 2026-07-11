namespace Domain.Exceptions;

public class PersonNotFoundException : DomainException
{
    public PersonNotFoundException(Guid id)
        : base($"Pessoa com Id '{id}' não encontrada.") { }
}
