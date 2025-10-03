
using System.Text.Json;

namespace BromcomReset.Api.Services
{
    public class AuditLogger
    {
        private readonly string _path;

        public AuditLogger(IConfiguration config, IWebHostEnvironment env)
        {
            var defaultPath = Path.Combine(env.ContentRootPath, "logs", "audit.log");
            _path = config.GetValue<string>("Audit:Path") ?? defaultPath;
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        }

        public async Task AppendAsync(object entry, CancellationToken ct = default)
        {
            var line = JsonSerializer.Serialize(entry) + Environment.NewLine;
            await File.AppendAllTextAsync(_path, line, ct);
        }
    }
}
