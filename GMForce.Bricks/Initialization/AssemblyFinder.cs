using System.Reflection;

namespace GMForce.Bricks.Initialization;

public class AssemblyFinder(string projectPrefix)
{
    private readonly string _projectPrefix = projectPrefix ?? throw new ArgumentNullException(nameof(projectPrefix));

    public Assembly Api => FindAssembly("Api");
    public Assembly Infrastructure => FindAssembly("Infrastructure");

    private Assembly FindAssembly(string projectSuffix) => Assembly.Load($"{_projectPrefix}.{projectSuffix}");
}