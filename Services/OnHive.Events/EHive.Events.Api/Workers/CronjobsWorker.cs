using EHive.Events.Domain.Models;
using NCrontab;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EHive.Events.Api.Workers
{
    public class CronjobsWorker : BackgroundService
    {
        private readonly ILogger logger;
        private readonly EventsApiSettings eventsApiSettings;

        public CronjobsWorker(EventsApiSettings eventsApiSettings)
        {
            this.eventsApiSettings = eventsApiSettings;
            this.logger = Log.Logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.Information("CronjobsWorker initialized at: {time}", DateTimeOffset.Now);
            logger.Information("Loaded {CronjobCount} cronjobs from configuration.", eventsApiSettings.Cronjobs.Count);
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.Information("CronjobsWorker running at: {time}", DateTimeOffset.Now);
                foreach (var cronjob in eventsApiSettings.Cronjobs)
                {
                    try
                    {
                        if (cronjob.Enabled)
                        {
                            var cronExpression = CrontabSchedule.Parse(cronjob.CronExpression);
                            var nextOccurrence = cronExpression.GetNextOccurrence(DateTime.UtcNow.AddSeconds(-30));
                            logger.Information("Next occurrence for cronjob {CronjobName} is at {NextOccurrence}", cronjob.Name, nextOccurrence);
                            if (nextOccurrence <= DateTime.UtcNow)
                            {
                                try
                                {
                                    logger.Information("Executing cronjob: {CronjobName}", cronjob.Name);
                                    await ExecuteJobAsyc(cronjob);
                                    logger.Information("Finished executing cronjob: {CronjobName}", cronjob.Name);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, "Error executing cronjob: {CronjobName}", cronjob.Name);
                                }
                            }
                        }
                        else
                        {
                            logger.Information("Cronjob {CronjobName} is disabled, skipping execution.", cronjob.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error executing cronjob: {CronjobName}", cronjob.Name);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ExecuteJobAsyc(Cronjob cronjob)
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(cronjob.TriggerUrl);
            if (cronjob.Headers.Any())
            {
                foreach (var header in cronjob.Headers)
                {
                    if (httpClient.DefaultRequestHeaders.Contains(header.Key))
                    {
                        httpClient.DefaultRequestHeaders.Remove(header.Key);
                    }
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            switch (cronjob.Method)
            {
                case "GET":
                    var getResponse = await httpClient.GetAsync(cronjob.TriggerUrl);
                    getResponse.EnsureSuccessStatusCode();
                    logger.Information("GET Response: {Response}", await getResponse.Content.ReadAsStringAsync());
                    return;

                case "POST":
                    var postResponse = await httpClient.PostAsync(cronjob.TriggerUrl, new StringContent(cronjob.Body));
                    postResponse.EnsureSuccessStatusCode();
                    logger.Information("POST Response: {Response}", await postResponse.Content.ReadAsStringAsync());
                    return;

                case "PUT":
                    var putResponse = await httpClient.PutAsync(cronjob.TriggerUrl, new StringContent(cronjob.Body));
                    putResponse.EnsureSuccessStatusCode();
                    logger.Information("PUT Response: {Response}", await putResponse.Content.ReadAsStringAsync());
                    return;

                case "DELETE":
                    var deleteResponse = await httpClient.DeleteAsync(cronjob.TriggerUrl);
                    deleteResponse.EnsureSuccessStatusCode();
                    logger.Information("DELETE Response: {Response}", await deleteResponse.Content.ReadAsStringAsync());
                    return;

                case "PATCH":
                    var patchResponse = await httpClient.PatchAsync(cronjob.TriggerUrl, new StringContent(cronjob.Body));
                    patchResponse.EnsureSuccessStatusCode();
                    logger.Information("PATCH Response: {Response}", await patchResponse.Content.ReadAsStringAsync());
                    return;

                default:
                    logger.Warning("Unsupported HTTP method: {Method}", cronjob.Method);
                    break;
            }
        }
    }
}