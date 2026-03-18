using Application.Interfaces;
using Hangfire;
using System.Linq.Expressions;

namespace Infrastructure.BackgroundJobs;

public class HangfireJobService(
    IBackgroundJobClient backgroundJobClient,
    IRecurringJobManager recurringJobManager) : IJobService
{
    public void Enqueue<T>(Expression<Action<T>> methodCall)
        => backgroundJobClient.Enqueue(methodCall);

    public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        => backgroundJobClient.Schedule(methodCall, delay);

    public void AddOrUpdateRecurring<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression)
        => recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression);
}