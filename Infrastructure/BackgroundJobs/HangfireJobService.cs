using Application.Interfaces;
using Hangfire;
using System.Linq.Expressions;

namespace Infrastructure.BackgroundJobs;

public class HangfireJobService : IJobService
{
    public void Enqueue<T>(Expression<Action<T>> methodCall)
        => BackgroundJob.Enqueue(methodCall);

    public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        => BackgroundJob.Schedule(methodCall, delay);

    public void AddOrUpdateRecurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression)
        => RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
}