using DocumentFlow.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace DocumentFlow.Services
{
    /// <summary>
    /// Результат валидации пароля
    /// </summary>
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public int StrengthScore { get; set; } // 0-100
        public string StrengthLevel { get; set; } = "Слабый";
    }

    /// <summary>
    /// Интерфейс сервиса валидации паролей
    /// </summary>
    public interface IPasswordValidatorService
    {
        PasswordValidationResult Validate(string password);
        Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password);
    }

    /// <summary>
    /// Расширенный сервис валидации паролей с проверкой на распространённые пароли
    /// </summary>
    public class PasswordValidatorService : IPasswordValidator<ApplicationUser>, IPasswordValidatorService
    {
        // Список распространённых слабых паролей
        private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            "123456", "123456789", "12345678", "1234567", "password", "qwerty", "abc123",
            "monkey", "1234567890", "letmein", "trustno1", "dragon", "baseball", "iloveyou",
            "master", "sunshine", "ashley", "bailey", "passw0rd", "shadow", "michael",
            "qwerty123", "password1", "password123", "admin", "admin123", "root", "toor",
            "welcome", "welcome1", "welcome123", "login", "qweasd", "qweasdzxc",
            "123qwe", "1q2w3e", "1q2w3e4r", "1qaz2wsx", "zaq12wsx", "!qaz2wsx",
            "пароль", "йцукен", "привет", "любовь", "россия", "123йцу", "qazwsx"
        };

        // Минимальная длина пароля
        private const int MinPasswordLength = 8;
        private const int MaxPasswordLength = 128;

        public PasswordValidationResult Validate(string password)
        {
            var result = new PasswordValidationResult();
            var errors = new List<string>();
            int score = 0;

            if (string.IsNullOrWhiteSpace(password))
            {
                result.Errors.Add("Пароль не может быть пустым.");
                result.IsValid = false;
                return result;
            }

            // Проверка длины
            if (password.Length < MinPasswordLength)
            {
                errors.Add($"Пароль должен содержать минимум {MinPasswordLength} символов.");
            }
            else
            {
                score += 20;
                if (password.Length >= 12) score += 10;
                if (password.Length >= 16) score += 10;
            }

            if (password.Length > MaxPasswordLength)
            {
                errors.Add($"Пароль не должен превышать {MaxPasswordLength} символов.");
            }

            // Проверка наличия строчных букв
            if (!Regex.IsMatch(password, @"[a-zа-яё]"))
            {
                errors.Add("Пароль должен содержать хотя бы одну строчную букву.");
            }
            else
            {
                score += 15;
            }

            // Проверка наличия заглавных букв
            if (!Regex.IsMatch(password, @"[A-ZА-ЯЁ]"))
            {
                errors.Add("Пароль должен содержать хотя бы одну заглавную букву.");
            }
            else
            {
                score += 15;
            }

            // Проверка наличия цифр
            if (!Regex.IsMatch(password, @"\d"))
            {
                errors.Add("Пароль должен содержать хотя бы одну цифру.");
            }
            else
            {
                score += 15;
            }

            // Проверка наличия специальных символов
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]"))
            {
                errors.Add("Пароль должен содержать хотя бы один специальный символ (!@#$%^&*()_+-=[]{}|;':\"\\,.<>/?~`).");
            }
            else
            {
                score += 15;
            }

            // Проверка на распространённые пароли
            if (CommonPasswords.Contains(password))
            {
                errors.Add("Этот пароль слишком распространён и небезопасен.");
                score = Math.Max(0, score - 50);
            }

            // Проверка на последовательности
            if (HasSequentialChars(password))
            {
                errors.Add("Пароль не должен содержать последовательные символы (123, abc, qwe).");
                score = Math.Max(0, score - 20);
            }

            // Проверка на повторяющиеся символы
            if (HasRepeatingChars(password))
            {
                errors.Add("Пароль не должен содержать повторяющиеся символы (aaa, 111).");
                score = Math.Max(0, score - 15);
            }

            result.Errors = errors;
            result.IsValid = errors.Count == 0;
            result.StrengthScore = Math.Min(100, score);
            result.StrengthLevel = GetStrengthLevel(result.StrengthScore);

            return result;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "EmptyPassword",
                    Description = "Пароль не может быть пустым."
                });
            }

            var validationResult = Validate(password);

            if (validationResult.IsValid)
            {
                return IdentityResult.Success;
            }

            var errors = validationResult.Errors.Select(e => new IdentityError
            {
                Code = "PasswordValidation",
                Description = e
            });

            return IdentityResult.Failed(errors.ToArray());
        }

        private static bool HasSequentialChars(string password)
        {
            var sequences = new[]
            {
                "012", "123", "234", "345", "456", "567", "678", "789",
                "abc", "bcd", "cde", "def", "efg", "fgh", "ghi", "hij",
                "ijk", "jkl", "klm", "lmn", "mno", "nop", "opq", "pqr",
                "qrs", "rst", "stu", "tuv", "uvw", "vwx", "wxy", "xyz",
                "qwe", "wer", "ert", "rty", "tyu", "yui", "uio", "iop",
                "asd", "sdf", "dfg", "fgh", "ghj", "hjk", "jkl",
                "zxc", "xcv", "cvb", "vbn", "bnm",
                "йцу", "цук", "уну", "кен", "енг", "нгш",
                "фыв", "ыва", "вап", "апр", "про", "рол",
                "ячс", "чсм", "сми", "мит", "ить"
            };

            var lowerPassword = password.ToLower();
            return sequences.Any(seq => lowerPassword.Contains(seq));
        }

        private static bool HasRepeatingChars(string password)
        {
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i] == password[i + 2])
                    return true;
            }
            return false;
        }

        private static string GetStrengthLevel(int score)
        {
            return score switch
            {
                >= 80 => "Отличный",
                >= 60 => "Хороший",
                >= 40 => "Средний",
                >= 20 => "Слабый",
                _ => "Очень слабый"
            };
        }
    }
}
