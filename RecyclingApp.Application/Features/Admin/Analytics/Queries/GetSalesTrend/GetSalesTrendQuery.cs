using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Admin.Analytics.Queries.GetSalesTrend;

/// <summary>
/// CQRS Query for sales trend analytics grouped by daily, weekly, or monthly time intervals.
/// </summary>
public record GetSalesTrendQuery(
    string Period = "monthly",
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<SalesTrendDto>>;

public class GetSalesTrendQueryHandler : IRequestHandler<GetSalesTrendQuery, Result<SalesTrendDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSalesTrendQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SalesTrendDto>> Handle(GetSalesTrendQuery request, CancellationToken cancellationToken)
    {
        var period = request.Period?.Trim().ToLowerInvariant() switch
        {
            "daily" => "daily",
            "weekly" => "weekly",
            _ => "monthly"
        };

        var now = DateTime.UtcNow;
        var endDate = request.EndDate ?? now;
        var startDate = request.StartDate ?? period switch
        {
            "daily" => endDate.AddDays(-30),
            "weekly" => endDate.AddDays(-84), // 12 weeks
            _ => endDate.AddMonths(-12)       // 12 months
        };

        // 1. Fetch filtered completed transactions from the database
        var transactions = await _context.RecyclingTransactions
            .AsNoTracking()
            .Where(t => t.Status == "Completed" && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderBy(t => t.TransactionDate)
            .Select(t => new
            {
                t.TransactionDate,
                t.Amount,
                t.Quantity
            })
            .ToListAsync(cancellationToken);

        // 2. Group dynamically based on the requested interval
        var dataPoints = new List<SalesTrendPointDto>();

        if (period == "daily")
        {
            var grouped = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .OrderBy(g => g.Key);

            foreach (var g in grouped)
            {
                dataPoints.Add(new SalesTrendPointDto(
                    g.Key.ToString("yyyy-MM-dd"),
                    g.Key,
                    g.Count(),
                    g.Sum(x => x.Amount),
                    g.Sum(x => x.Quantity)
                ));
            }
        }
        else if (period == "weekly")
        {
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var grouped = transactions
                .GroupBy(t =>
                {
                    var week = calendar.GetWeekOfYear(t.TransactionDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    return new { Year = t.TransactionDate.Year, Week = week };
                })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week);

            foreach (var g in grouped)
            {
                var minDate = g.Min(x => x.TransactionDate);
                dataPoints.Add(new SalesTrendPointDto(
                    $"W{g.Key.Week:D2} {g.Key.Year}",
                    minDate,
                    g.Count(),
                    g.Sum(x => x.Amount),
                    g.Sum(x => x.Quantity)
                ));
            }
        }
        else // monthly
        {
            var grouped = transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);

            foreach (var g in grouped)
            {
                var firstOfMonth = new DateTime(g.Key.Year, g.Key.Month, 1);
                dataPoints.Add(new SalesTrendPointDto(
                    firstOfMonth.ToString("MMM yyyy"),
                    firstOfMonth,
                    g.Count(),
                    g.Sum(x => x.Amount),
                    g.Sum(x => x.Quantity)
                ));
            }
        }

        var totalRevenue = transactions.Sum(t => t.Amount);
        var totalOrders = transactions.Count;

        var trendDto = new SalesTrendDto(
            period,
            startDate,
            endDate,
            totalRevenue,
            totalOrders,
            dataPoints
        );

        return Result<SalesTrendDto>.Success(trendDto);
    }
}

public class GetSalesTrendQueryValidator : AbstractValidator<GetSalesTrendQuery>
{
    public GetSalesTrendQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => string.IsNullOrWhiteSpace(p) || new[] { "daily", "weekly", "monthly" }.Contains(p.ToLowerInvariant()))
            .WithMessage("Period must be 'daily', 'weekly', or 'monthly'.");
    }
}
