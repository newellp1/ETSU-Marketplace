// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ETSU_Marketplace.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IFileStorageService _fss;


        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileStorageService fss)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fss = fss;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [MaxLength(500)]
            public string Bio { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile AvatarFile { get; set; }

            public string AvatarUrl { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Username = userName,
                PhoneNumber = phoneNumber,
                Bio = user.Bio,
                AvatarUrl = user.Avatar?.Path
            };
        }

        private async Task<ApplicationUser> GetUserWithAvatarAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _userManager.Users
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await GetUserWithAvatarAsync();
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // 1. Handle Username Change Correctly
            var currentUserName = await _userManager.GetUserNameAsync(user);
            if (Input.Username != currentUserName)
            {
                // This method updates the UserName AND triggers internal logic
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Username);

                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "Error: Username is already taken or invalid.";
                    return RedirectToPage();
                }

                // CRITICAL: Manually ensure the NormalizedUserName is updated
                // Identity uses the normalized version for database lookups
                await _userManager.UpdateNormalizedUserNameAsync(user);

                // CRITICAL: Update the security stamp
                // This ensures the next login attempt recognizes the "new" user state
                await _userManager.UpdateSecurityStampAsync(user);
            }

            // 2. Handle Bio and Avatar (Your existing logic)
            user.Bio = Input.Bio;
            if (Input.AvatarFile != null)
            {
                user.Avatar = new Image { Path = await _fss.ProcessImageUpload(Input.AvatarFile) };
            }

            // Update the actual User record for the Bio/Avatar changes
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Error: Failed to update profile.";
                return RedirectToPage();
            }

            // 3. Refresh the Cookie
            // Since the Username (a claim in the cookie) changed, we must refresh
            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
