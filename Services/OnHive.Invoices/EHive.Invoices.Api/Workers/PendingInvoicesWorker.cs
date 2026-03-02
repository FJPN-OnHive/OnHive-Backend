using EHive.Invoices.Domain.Abstractions.Services;
using EHive.Invoices.Domain.Models;

namespace EHive.Invoices.Api.Workers
{
    public class PendingInvoicesWorker : BackgroundService
    {
        private readonly ILogger<PendingInvoicesWorker> logger;
        private readonly IInvoicesService invoicesService;
        private readonly InvoicesApiSettings invoicesApiSettings;

        public PendingInvoicesWorker(ILogger<PendingInvoicesWorker> logger,
                                     IInvoicesService invoicesService,
                                     InvoicesApiSettings invoicesApiSettings)
        {
            this.logger = logger;
            this.invoicesService = invoicesService;
            this.invoicesApiSettings = invoicesApiSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("PendingInvoicesWorker running.");
            var cronExpression = Cronos.CronExpression.Parse(invoicesApiSettings.PendingInvoiceIntervalCron);
            var nextRun = cronExpression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Utc);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (nextRun.HasValue && nextRun.Value < DateTimeOffset.Now)
                {
                    logger.LogInformation("PendingInvoicesWorker running at: {time}", DateTimeOffset.Now);
                    await invoicesService.VerifyPendingInvoices();
                    nextRun = cronExpression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Utc);
                }
            }
            logger.LogInformation("PendingInvoicesWorker stopped.");
        }
    }
}