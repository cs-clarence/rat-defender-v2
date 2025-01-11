using Common.Application.UnitOfWork.Abstractions;
using Iam.Domain.Entities;
using Iam.Domain.Repositories.Abstractions;
using Iam.Domain.ValueObjects;
using Iam.Shared.Authorization;
using Microsoft.Extensions.Logging;

namespace WebApiCli.Commands;

public class DatabaseCommands(
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork,
    IRoleRepository roleRepository,
    ILogger<DatabaseCommands> logger
)
{
    private async Task<IEnumerable<Role>> SeedRoles(
        CancellationToken cancellationToken
    )
    {
        var roles = new List<Role>();

        foreach (var r in Roles.All)
        {
            PathCode code = new(r.GetCode());
            if (
                await roleRepository.ContainsAsync(
                    role => role.Code == code,
                    cancellationToken
                )
            )
            {
                continue;
            }

            var role = new Role(
                0,
                new(r.GetCode()),
                r.GetName(),
                r.GetDescription(),
                isSystemManaged: true
            );

            var roleCode = r.GetCode();
            if (roleCode == Roles.Administrator.Code)
            {
                var adminCodes = Permissions.AdministratorCodes.ToList();
                var permissions = await permissionRepository.FindManyAsync(
                    (p) => adminCodes.Contains(p.Code),
                    cancellationToken
                );

                role.GrantPermission(permissions);
            }
            else if (roleCode == Roles.User.Code)
            {
                var userCodes = Permissions.UserCodes.ToList();
                var permissions = await permissionRepository.FindManyAsync(
                    (p) => userCodes.Contains(p.Code),
                    cancellationToken
                );

                role.GrantPermission(permissions);
            }

            await roleRepository.AddAsync(role, cancellationToken);
            roles.Add(role);
        }

        await roleRepository.AddManyAsync(roles, cancellationToken);

        return roles;
    }

    private async Task<IEnumerable<Permission>> SeedPermissions(
        CancellationToken cancellationToken
    )
    {
        var permissions = new List<Permission>();

        foreach (var permissionCode in Permissions.AllCodes)
        {
            PathCode code = new(permissionCode);
            if (
                await permissionRepository.ContainsAsync(
                    r => r.Code == code,
                    cancellationToken
                )
            )
            {
                continue;
            }

            var permission = new Permission(0, code, isSystemManaged: true);
            await permissionRepository.AddAsync(permission, cancellationToken);
            permissions.Add(permission);
        }

        await permissionRepository.AddManyAsync(permissions, cancellationToken);

        return permissions;
    }

    public async Task Seed(CancellationToken cancellationToken) =>
        await unitOfWork.ExecAsync(
            async () =>
            {
                var permissions = (
                    await SeedPermissions(cancellationToken)
                ).ToList();
                var permissionCodes = permissions.Select(p => p.Code).ToList();
                if (permissions.Count == 0)
                {
                    logger.LogWarning("No Permissions were created");
                }
                else
                {
                    logger.LogInformation(
                        "Created {Count} Permissions: {Codes}",
                        permissions.Count,
                        permissionCodes
                    );
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                var roles = (await SeedRoles(cancellationToken)).ToList();
                var roleCodes = roles.Select(r => r.Code).ToList();
                if (roles.Count == 0)
                {
                    logger.LogWarning("No Roles were created");
                }
                else
                {
                    logger.LogInformation(
                        "Created {Count} Roles: {Codes}",
                        roles.Count,
                        roleCodes
                    );
                }
            },
            cancellationToken
        );
}
