namespace Domain.Exceptions;

public class MinorCannotCreateRevenueException : DomainException
{
    public MinorCannotCreateRevenueException()
        : base("Pessoas menores de 18 anos não podem cadastrar receitas.") { }
}
