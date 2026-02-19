using DocumentFlow.Data;
using DocumentFlow.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DocumentFlow.Services
{
    /// <summary>
    /// Результат верификации
    /// </summary>
    public class VerificationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int RemainingAttempts { get; set; }
    }

    /// <summary>
    /// Интерфейс сервиса верификации email
    /// </summary>
    public interface IEmailVerificationService
    {
        /// <summary>
        /// Генерирует и сохраняет новый код верификации
        /// </summary>
        Task<string> GenerateCodeAsync(string userId, VerificationType type, string? ipAddress = null);

        /// <summary>
        /// Проверяет код верификации
        /// </summary>
        Task<VerificationResult> VerifyCodeAsync(string userId, string code, VerificationType type);

        /// <summary>
        /// Отправляет код на email пользователя
        /// </summary>
        Task<bool> SendVerificationEmailAsync(string userId, string code);

        /// <summary>
        /// Проверяет, можно ли отправить новый код
        /// </summary>
        Task<(bool CanResend, int SecondsToWait)> CanResendCodeAsync(string userId, VerificationType type);

        /// <summary>
        /// Инвалидирует все существующие коды пользователя
        /// </summary>
        Task InvalidateAllCodesAsync(string userId, VerificationType type);

        /// <summary>
        /// Логирует попытку входа
        /// </summary>
        Task LogLoginAttemptAsync(string userId, bool isSuccessful, string? ipAddress, string? userAgent, string? failureReason = null);

        /// <summary>
        /// Получает историю входов пользователя
        /// </summary>
        Task<List<LoginHistory>> GetLoginHistoryAsync(string userId, int count = 10);

        /// <summary>
        /// Проверяет, заблокирован ли пользователь из-за множественных неудачных попыток
        /// </summary>
        Task<(bool IsBlocked, DateTime? BlockedUntil)> IsUserBlockedAsync(string userId, string? ipAddress);
    }

    /// <summary>
    /// Сервис верификации email и управления кодами подтверждения
    /// </summary>
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailVerificationService> _logger;

        // Настройки
        private const int CodeLength = 6;
        private const int CodeExpirationMinutes = 10;
        private const int MaxAttemptsPerCode = 5;
        private const int ResendCooldownSeconds = 60;
        private const int MaxFailedLoginsBeforeBlock = 5;
        private const int BlockDurationMinutes = 15;

        public EmailVerificationService(
            ApplicationDbContext context,
            ILogger<EmailVerificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateCodeAsync(string userId, VerificationType type, string? ipAddress = null)
        {
            // Инвалидируем существующие коды
            await InvalidateAllCodesAsync(userId, type);

            // Генерируем новый код
            var code = GenerateRandomCode();

            var verificationCode = new EmailVerificationCode
            {
                UserId = userId,
                Code = code,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes),
                MaxAttempts = MaxAttemptsPerCode,
                RequestIpAddress = ipAddress
            };

            _context.EmailVerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Создан код верификации для пользователя {UserId}, тип: {Type}", userId, type);

            return code;
        }

        public async Task<VerificationResult> VerifyCodeAsync(string userId, string code, VerificationType type)
        {
            var verificationCode = await _context.EmailVerificationCodes
                .Where(c => c.UserId == userId && c.Type == type && !c.IsUsed)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (verificationCode == null)
            {
                return new VerificationResult
                {
                    Success = false,
                    ErrorMessage = "Код верификации не найден. Запросите новый код.",
                    RemainingAttempts = 0
                };
            }

            // Увеличиваем счётчик попыток
            verificationCode.AttemptCount++;
            await _context.SaveChangesAsync();

            if (verificationCode.IsExpired)
            {
                return new VerificationResult
                {
                    Success = false,
                    ErrorMessage = "Срок действия кода истёк. Запросите новый код.",
                    RemainingAttempts = 0
                };
            }

            if (verificationCode.IsMaxAttemptsReached)
            {
                return new VerificationResult
                {
                    Success = false,
                    ErrorMessage = "Превышено максимальное количество попыток. Запросите новый код.",
                    RemainingAttempts = 0
                };
            }

            if (!string.Equals(verificationCode.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                var remaining = MaxAttemptsPerCode - verificationCode.AttemptCount;
                return new VerificationResult
                {
                    Success = false,
                    ErrorMessage = $"Неверный код. Осталось попыток: {remaining}",
                    RemainingAttempts = remaining
                };
            }

            // Код верный - помечаем как использованный
            verificationCode.IsUsed = true;
            verificationCode.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Код верификации успешно подтверждён для пользователя {UserId}", userId);

            return new VerificationResult
            {
                Success = true,
                RemainingAttempts = MaxAttemptsPerCode - verificationCode.AttemptCount
            };
        }

        public async Task<bool> SendVerificationEmailAsync(string userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("Не удалось отправить email: пользователь {UserId} не найден", userId);
                return false;
            }

            // TODO: Интеграция с реальным email-сервисом (SendGrid, SMTP и т.д.)
            // В демонстрационных целях просто логируем
            _logger.LogInformation(
                "Отправка кода верификации {Code} на email {Email} для пользователя {UserId}",
                code, user.Email, userId);

            // Симуляция отправки email
            // В реальном проекте здесь будет:
            // await _emailSender.SendEmailAsync(user.Email, "Код подтверждения", $"Ваш код: {code}");

            return true;
        }

        public async Task<(bool CanResend, int SecondsToWait)> CanResendCodeAsync(string userId, VerificationType type)
        {
            var lastCode = await _context.EmailVerificationCodes
                .Where(c => c.UserId == userId && c.Type == type)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastCode == null)
            {
                return (true, 0);
            }

            var secondsSinceLastCode = (DateTime.UtcNow - lastCode.CreatedAt).TotalSeconds;
            if (secondsSinceLastCode >= ResendCooldownSeconds)
            {
                return (true, 0);
            }

            return (false, ResendCooldownSeconds - (int)secondsSinceLastCode);
        }

        public async Task InvalidateAllCodesAsync(string userId, VerificationType type)
        {
            var existingCodes = await _context.EmailVerificationCodes
                .Where(c => c.UserId == userId && c.Type == type && !c.IsUsed)
                .ToListAsync();

            foreach (var code in existingCodes)
            {
                code.IsUsed = true;
                code.UsedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task LogLoginAttemptAsync(string userId, bool isSuccessful, string? ipAddress, string? userAgent, string? failureReason = null)
        {
            var loginHistory = new LoginHistory
            {
                UserId = userId,
                LoginTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason
            };

            _context.LoginHistories.Add(loginHistory);
            await _context.SaveChangesAsync();

            if (!isSuccessful)
            {
                _logger.LogWarning(
                    "Неудачная попытка входа для пользователя {UserId} с IP {IpAddress}: {Reason}",
                    userId, ipAddress, failureReason);
            }
        }

        public async Task<List<LoginHistory>> GetLoginHistoryAsync(string userId, int count = 10)
        {
            return await _context.LoginHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.LoginTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<(bool IsBlocked, DateTime? BlockedUntil)> IsUserBlockedAsync(string userId, string? ipAddress)
        {
            var recentFailedAttempts = await _context.LoginHistories
                .Where(h => h.UserId == userId && !h.IsSuccessful)
                .Where(h => h.LoginTime > DateTime.UtcNow.AddMinutes(-BlockDurationMinutes))
                .CountAsync();

            if (recentFailedAttempts >= MaxFailedLoginsBeforeBlock)
            {
                var lastAttempt = await _context.LoginHistories
                    .Where(h => h.UserId == userId && !h.IsSuccessful)
                    .OrderByDescending(h => h.LoginTime)
                    .FirstOrDefaultAsync();

                if (lastAttempt != null)
                {
                    var blockedUntil = lastAttempt.LoginTime.AddMinutes(BlockDurationMinutes);
                    if (DateTime.UtcNow < blockedUntil)
                    {
                        return (true, blockedUntil);
                    }
                }
            }

            // Также проверяем по IP
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var ipFailedAttempts = await _context.LoginHistories
                    .Where(h => h.IpAddress == ipAddress && !h.IsSuccessful)
                    .Where(h => h.LoginTime > DateTime.UtcNow.AddMinutes(-BlockDurationMinutes))
                    .CountAsync();

                if (ipFailedAttempts >= MaxFailedLoginsBeforeBlock * 2) // Более строгий лимит для IP
                {
                    return (true, DateTime.UtcNow.AddMinutes(BlockDurationMinutes));
                }
            }

            return (false, null);
        }

        private static string GenerateRandomCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
            return number.ToString("D6");
        }
    }
}
