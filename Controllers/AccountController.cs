using DocumentFlow.Models.Entities;
using DocumentFlow.Models.ViewModels;
using DocumentFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DocumentFlow.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditService _auditService;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IPasswordValidatorService _passwordValidatorService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuditService auditService,
            IEmailVerificationService emailVerificationService,
            IPasswordValidatorService passwordValidatorService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditService = auditService;
            _emailVerificationService = emailVerificationService;
            _passwordValidatorService = passwordValidatorService;
        }

        #region Login

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверный email или пароль, либо учётная запись заблокирована.");
                return View(model);
            }

            // Проверяем, не заблокирован ли пользователь из-за неудачных попыток
            var (isBlocked, blockedUntil) = await _emailVerificationService.IsUserBlockedAsync(user.Id, ipAddress);
            if (isBlocked)
            {
                var minutes = blockedUntil.HasValue 
                    ? (int)Math.Ceiling((blockedUntil.Value - DateTime.UtcNow).TotalMinutes) 
                    : 15;
                
                ModelState.AddModelError(string.Empty, 
                    $"Учётная запись временно заблокирована из-за множественных неудачных попыток входа. Попробуйте через {minutes} мин.");
                return View(model);
            }

            if (!user.IsActive)
            {
                await _emailVerificationService.LogLoginAttemptAsync(user.Id, false, ipAddress, userAgent, "Учётная запись деактивирована");
                ModelState.AddModelError(string.Empty, "Учётная запись заблокирована. Обратитесь к администратору.");
                return View(model);
            }

            // Проверяем пароль
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                await _emailVerificationService.LogLoginAttemptAsync(user.Id, false, ipAddress, userAgent, "Неверный пароль");
                ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
                return View(model);
            }

            // Проверяем подтверждён ли email
            if (!user.EmailConfirmed)
            {
                // Генерируем код и отправляем
                var code = await _emailVerificationService.GenerateCodeAsync(user.Id, VerificationType.EmailConfirmation, ipAddress);
                await _emailVerificationService.SendVerificationEmailAsync(user.Id, code);

                TempData["InfoMessage"] = "На ваш email отправлен код подтверждения.";
                return RedirectToAction(nameof(VerifyEmail), new { userId = user.Id, returnUrl = model.ReturnUrl });
            }

            // Проверяем требуется ли смена пароля
            if (user.MustChangePassword)
            {
                return RedirectToAction(nameof(ForceChangePassword), new { userId = user.Id, returnUrl = model.ReturnUrl });
            }

            // Выполняем вход
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Обновляем время последнего входа
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                await _emailVerificationService.LogLoginAttemptAsync(user.Id, true, ipAddress, userAgent);
                await _auditService.LogAsync(user.Id, AuditActionType.Login, "Вход в систему");
                
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                
                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                await _emailVerificationService.LogLoginAttemptAsync(user.Id, false, ipAddress, userAgent, "Учётная запись заблокирована (lockout)");
                ModelState.AddModelError(string.Empty, "Учётная запись временно заблокирована. Попробуйте позже.");
                return View(model);
            }

            await _emailVerificationService.LogLoginAttemptAsync(user.Id, false, ipAddress, userAgent, "Неудачная попытка входа");
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }

        #endregion

        #region Email Verification

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string userId, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var (canResend, secondsToWait) = await _emailVerificationService.CanResendCodeAsync(userId, VerificationType.EmailConfirmation);

            var model = new VerifyEmailViewModel
            {
                UserId = userId,
                Email = user.Email ?? "",
                ReturnUrl = returnUrl,
                SecondsToResend = canResend ? null : secondsToWait
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var (canResend, secondsToWait) = await _emailVerificationService.CanResendCodeAsync(model.UserId, VerificationType.EmailConfirmation);
                model.SecondsToResend = canResend ? null : secondsToWait;
                return View(model);
            }

            var result = await _emailVerificationService.VerifyCodeAsync(model.UserId, model.Code, VerificationType.EmailConfirmation);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ошибка верификации.");
                var (canResend, secondsToWait) = await _emailVerificationService.CanResendCodeAsync(model.UserId, VerificationType.EmailConfirmation);
                model.SecondsToResend = canResend ? null : secondsToWait;
                return View(model);
            }

            // Подтверждаем email
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _auditService.LogAsync(user.Id, AuditActionType.UserModified, "Email подтверждён");

            // Если требуется смена пароля
            if (user.MustChangePassword)
            {
                TempData["SuccessMessage"] = "Email подтверждён. Теперь необходимо сменить пароль.";
                return RedirectToAction(nameof(ForceChangePassword), new { userId = user.Id, returnUrl = model.ReturnUrl });
            }

            TempData["SuccessMessage"] = "Email успешно подтверждён. Теперь вы можете войти в систему.";
            return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(string userId, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var (canResend, secondsToWait) = await _emailVerificationService.CanResendCodeAsync(userId, VerificationType.EmailConfirmation);

            if (!canResend)
            {
                TempData["ErrorMessage"] = $"Подождите {secondsToWait} секунд перед повторной отправкой.";
                return RedirectToAction(nameof(VerifyEmail), new { userId, returnUrl });
            }

            var ipAddress = GetClientIpAddress();
            var code = await _emailVerificationService.GenerateCodeAsync(userId, VerificationType.EmailConfirmation, ipAddress);
            await _emailVerificationService.SendVerificationEmailAsync(userId, code);

            TempData["SuccessMessage"] = "Новый код отправлен на ваш email.";
            return RedirectToAction(nameof(VerifyEmail), new { userId, returnUrl });
        }

        #endregion

        #region Force Change Password

        [HttpGet]
        public async Task<IActionResult> ForceChangePassword(string userId, string? returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (!user.MustChangePassword)
                return RedirectToAction(nameof(Login));

            var model = new ForceChangePasswordViewModel
            {
                UserId = userId,
                Email = user.Email ?? "",
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceChangePassword(ForceChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            // Дополнительная валидация пароля
            var passwordValidation = _passwordValidatorService.Validate(model.NewPassword);
            if (!passwordValidation.IsValid)
            {
                foreach (var error in passwordValidation.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            // Сбрасываем пароль
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                user.MustChangePassword = false;
                user.PasswordChangedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                await _auditService.LogAsync(user.Id, AuditActionType.UserModified, "Пароль изменён (первый вход)");

                TempData["SuccessMessage"] = "Пароль успешно изменён. Теперь вы можете войти в систему.";
                return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _auditService.LogAsync(user.Id, AuditActionType.Logout, "Выход из системы");
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        #endregion

        #region Profile

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Position = user.Position,
                Department = user.Department,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.MiddleName = model.MiddleName;
            user.Position = model.Position;
            user.Department = model.Department;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Профиль успешно обновлён.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        #endregion

        #region Change Password

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Дополнительная валидация пароля
            var passwordValidation = _passwordValidatorService.Validate(model.NewPassword);
            if (!passwordValidation.IsValid)
            {
                foreach (var error in passwordValidation.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                user.PasswordChangedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                await _signInManager.RefreshSignInAsync(user);
                await _auditService.LogAsync(user.Id, AuditActionType.UserModified, "Пароль изменён");
                TempData["SuccessMessage"] = "Пароль успешно изменён.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        #endregion

        #region Password Strength API

        [HttpPost]
        [AllowAnonymous]
        public IActionResult CheckPasswordStrength([FromBody] string password)
        {
            var result = _passwordValidatorService.Validate(password ?? "");
            return Json(new PasswordStrengthResult
            {
                IsValid = result.IsValid,
                Errors = result.Errors,
                StrengthScore = result.StrengthScore,
                StrengthLevel = result.StrengthLevel
            });
        }

        #endregion

        #region Access Denied

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Helpers

        private string? GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Проверяем заголовки для прокси
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault()?.Trim();
            }
            
            return ipAddress;
        }

        #endregion
    }
}
