using HRMarket.Configuration.Status;
using HRMarket.Entities;
using HRMarket.Entities.Stripe;
using Microsoft.EntityFrameworkCore;

namespace HRMarket.Core.StripeApi;

public interface ISubscriptionRepository
{
    Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId);
    Task<List<SubscriptionPlan>> GetAllActivePlansAsync();
    Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan);
    Task UpdatePlanAsync(SubscriptionPlan plan);
    Task<FirmSubscription?> GetActiveSubscriptionByFirmIdAsync(Guid firmId);
    Task<FirmSubscription?> GetSubscriptionByStripeIdAsync(string stripeSubscriptionId);
    Task<FirmSubscription> CreateFirmSubscriptionAsync(FirmSubscription subscription);
    Task UpdateFirmSubscriptionAsync(FirmSubscription subscription);
    Task<PaymentHistory> CreatePaymentHistoryAsync(PaymentHistory payment);
    Task<List<PaymentHistory>> GetPaymentHistoryByFirmIdAsync(Guid firmId);
}

public class SubscriptionRepository(ApplicationDbContext context) : ISubscriptionRepository
{
    public async Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId)
    {
        return await context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(sp => sp.Id == planId && sp.IsActive);
    }

    public async Task<List<SubscriptionPlan>> GetAllActivePlansAsync()
    {
        return await context.Set<SubscriptionPlan>()
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.PriceMonthly)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan)
    {
        context.Set<SubscriptionPlan>().Add(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public async Task UpdatePlanAsync(SubscriptionPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<FirmSubscription?> GetActiveSubscriptionByFirmIdAsync(Guid firmId)
    {
        return await context.Set<FirmSubscription>()
            .Include(fs => fs.SubscriptionPlan)
            .FirstOrDefaultAsync(fs =>
                fs.FirmId == firmId &&
                (fs.Status == SubscriptionStatus.Active || fs.Status == SubscriptionStatus.Trialing));
    }

    public async Task<FirmSubscription?> GetSubscriptionByStripeIdAsync(string stripeSubscriptionId)
    {
        return await context.Set<FirmSubscription>()
            .Include(fs => fs.SubscriptionPlan)
            .Include(fs => fs.Firm)
            .FirstOrDefaultAsync(fs => fs.StripeSubscriptionId == stripeSubscriptionId);
    }

    public async Task<FirmSubscription> CreateFirmSubscriptionAsync(FirmSubscription subscription)
    {
        context.Set<FirmSubscription>().Add(subscription);
        await context.SaveChangesAsync();
        return subscription;
    }

    public async Task UpdateFirmSubscriptionAsync(FirmSubscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<PaymentHistory> CreatePaymentHistoryAsync(PaymentHistory payment)
    {
        context.Set<PaymentHistory>().Add(payment);
        await context.SaveChangesAsync();
        return payment;
    }

    public async Task<List<PaymentHistory>> GetPaymentHistoryByFirmIdAsync(Guid firmId)
    {
        return await context.Set<PaymentHistory>()
            .Where(ph => ph.FirmId == firmId)
            .OrderByDescending(ph => ph.CreatedAt)
            .ToListAsync();
    }
}

