namespace Application.DTOs;

public record TotalsResponse(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo
);
