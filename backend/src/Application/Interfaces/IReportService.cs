using Application.DTOs;

namespace Application.Interfaces;

public interface IReportService
{
    Task<TotalsResponse> GetTotalsAsync();
}
