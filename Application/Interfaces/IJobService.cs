using System.Linq.Expressions;

namespace Application.Interfaces;

public interface IJobService
{
    // Fire and Forget (SMS, Emails)
    void Enqueue<T>(Expression<Action<T>> methodCall);

    // Scheduled (Run in 24 hours)
    void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);


    void AddOrUpdateRecurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
}