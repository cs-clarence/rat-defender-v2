using Common.Application.UnitOfWork.Abstractions;
using ConsoleAppFramework;
using Iam.Domain.Services.Abstractions;
using Iam.Domain.ValueObjects;
using Iam.Shared.Authorization;
using Microsoft.Extensions.Logging;
using Sharprompt;
using Ct = System.Threading.CancellationToken;

namespace WebApiCli.Commands;

public class CreateEntityCommands(
    IUserAccountService userAccountService,
    IRoleService roleService,
    IUnitOfWork u,
    ILogger<CreateEntityCommands> logger
)
{
    [Command("admin")]
    public async Task CreateAdmin(Ct token) =>
        await u.ExecAsync(
            async () =>
            {
                var userName = "";

                while (true)
                {
                    userName = Prompt.Input<string>("User Name");

                    if (
                        !await userAccountService.IsUserNameAvailableAsync(
                            (UserName)userName,
                            token
                        )
                    )
                    {
                        logger.LogError("User Name already exists");
                        continue;
                    }

                    break;
                }

                var emailAddressExists = true;
                var emailAddress = "";

                while (emailAddressExists)
                {
                    emailAddress = Prompt.Input<string>("Email Address");

                    if (
                        !await userAccountService.IsEmailAddressAvailableAsync(
                            (EmailAddress)emailAddress,
                            token
                        )
                    )
                    {
                        logger.LogError("Email Address already exists");
                        emailAddressExists = true;
                        continue;
                    }

                    emailAddressExists = false;
                }

                var isValidPassword = false;
                var password = "";

                while (!isValidPassword)
                {
                    password = Prompt.Password("Password");
                    var confirmPassword = Prompt.Password("Confirm Password");

                    if (
                        string.IsNullOrEmpty(password)
                        || string.IsNullOrEmpty(confirmPassword)
                    )
                    {
                        logger.LogError("Password cannot be empty");
                        continue;
                    }

                    if (!password.Equals(confirmPassword))
                    {
                        logger.LogError("Passwords do not match");
                        continue;
                    }

                    isValidPassword = true;
                }

                var firstName = Prompt.Input<string>("First Name");
                var lastName = Prompt.Input<string>("Last Name");

                var adminRole = await roleService.GetRoleByCodeAsync(
                    (PathCode)Roles.Administrator.Code,
                    token
                );

                var user = await userAccountService.CreateUserAsync(
                    new NewUserAccount
                    {
                        UserName = (UserName)userName,
                        PrimaryEmailAddress = (EmailAddress)emailAddress,
                        IsPrimaryEmailAddressVerified = true,
                        Password = password,
                        UserProfile = new NewUserProfile
                        {
                            Name = Name.Create(firstName, lastName),
                            DisplayPictureUrl = null,
                        },
                        RoleIds = [adminRole.Id],
                    },
                    token
                );

                return user;
            },
            token
        );
}
