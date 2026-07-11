using Application.DTOs;
using Application.Validators;
using Domain.Enums;
using FluentAssertions;

namespace UnitTests;

public class ValidatorTests
{
    private readonly CreatePersonRequestValidator _personValidator;
    private readonly CreateTransactionRequestValidator _transactionValidator;

    public ValidatorTests()
    {
        _personValidator = new CreatePersonRequestValidator();
        _transactionValidator = new CreateTransactionRequestValidator();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("A", true)]
    [InlineData("João Silva", true)]
    public void PersonValidator_Name_ShouldValidateCorrectly(string name, bool expected)
    {
        var request = new CreatePersonRequest(name, 25);
        var result = _personValidator.Validate(request);
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(150, true)]
    [InlineData(151, false)]
    public void PersonValidator_Age_ShouldValidateCorrectly(int age, bool expected)
    {
        var request = new CreatePersonRequest("João", age);
        var result = _personValidator.Validate(request);
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Compra", true)]
    public void TransactionValidator_Description_ShouldValidateCorrectly(string description, bool expected)
    {
        var request = new CreateTransactionRequest(description, 100m, TransactionType.Despesa, Guid.NewGuid());
        var result = _transactionValidator.Validate(request);
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(100, true)]
    public void TransactionValidator_Amount_ShouldValidateCorrectly(decimal amount, bool expected)
    {
        var request = new CreateTransactionRequest("Compra", amount, TransactionType.Despesa, Guid.NewGuid());
        var result = _transactionValidator.Validate(request);
        result.IsValid.Should().Be(expected);
    }
}
