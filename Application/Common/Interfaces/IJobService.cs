using System.Linq.Expressions;

namespace Application.Common.Interfaces;

public interface IJobService
{
    // Fire and Forget (SMS, Emails)
    void Enqueue<T>(Expression<Action<T>> methodCall);

    // Scheduled (Run in 24 hours)
    void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

    // Recurring (Daily Reports for the Boss)
    void AddOrUpdateRecurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
}