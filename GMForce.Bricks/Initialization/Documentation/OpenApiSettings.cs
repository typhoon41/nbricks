using Swashbuckle.AspNetCore.SwaggerGen;

namespace GMForce.Bricks.Initialization.Documentation;

public record OpenApiSettings
{
    public string Email { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ApiDetails ApiDetails { get; set; } = new ApiDetails();
    public Action<SwaggerGenOptions> OnConfiguringSwagger { get; set; } = (options) => { };
}
