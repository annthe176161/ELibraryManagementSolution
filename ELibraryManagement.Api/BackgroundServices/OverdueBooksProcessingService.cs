using ELibraryManagement.Api.Services;

namespace ELibraryManagement.Api.BackgroundServices
{
    public class OverdueBooksProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueBooksProcessingService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1); // Chạy mỗi 1 phút cho test

        public OverdueBooksProcessingService(
            IServiceProvider serviceProvider,
            ILogger<OverdueBooksProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OverdueBooksProcessingService đã khởi động - chạy mỗi 1 phút");

            // Chạy lần đầu sau 10 giây (để hệ thống khởi động hoàn tất)
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOverdueBooks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong quá trình xử lý sách quá hạn");
                }

                // Chờ đến lần chạy tiếp theo (1 phút)
                await Task.Delay(_period, stoppingToken);
            }
        }

        private async Task ProcessOverdueBooks()
        {
            var currentTime = DateTime.UtcNow;
            _logger.LogInformation($"[{currentTime:yyyy-MM-dd HH:mm:ss}] Bắt đầu kiểm tra sách quá hạn...");

            using var scope = _serviceProvider.CreateScope();
            var overdueProcessingService = scope.ServiceProvider.GetRequiredService<IOverdueProcessingService>();

            try
            {
                var processedCount = await overdueProcessingService.ProcessOverdueBooksAsync();

                if (processedCount > 0)
                {
                    _logger.LogWarning($"[{currentTime:yyyy-MM-dd HH:mm:ss}] ✅ Đã xử lý {processedCount} borrow records quá hạn");
                }
                else
                {
                    _logger.LogInformation($"[{currentTime:yyyy-MM-dd HH:mm:ss}] ✅ Không có borrow records quá hạn nào cần xử lý");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{currentTime:yyyy-MM-dd HH:mm:ss}] ❌ Lỗi khi xử lý sách quá hạn");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OverdueBooksProcessingService đã dừng");
            return base.StopAsync(cancellationToken);
        }
    }
}