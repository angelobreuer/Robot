namespace Robot.Server.Stages
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    internal sealed class AsyncStageProgress : IAsyncStageProgress
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly List<string> _scopes;
        private readonly object _syncRoot;
        private string? _cachedScopeList;

        public AsyncStageProgress(ILoggerFactory loggerFactory)
        {
            _syncRoot = new object();
            _scopes = new List<string>();
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _cachedScopeList = "<No Stage>";
        }

        /// <inheritdoc/>
        public ValueTask<IDisposable> OpenScopeAsync(string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_syncRoot)
            {
                _scopes.Add(name);
                RebuildScopeList();
            }

            var result = new ScopeDisposable(this, name);
            return new ValueTask<IDisposable>(result);
        }

        /// <inheritdoc/>
        public ValueTask ReportAsync(float progress, string status, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var logger = _loggerFactory.CreateLogger(_cachedScopeList);
            logger.LogInformation("({Progress:0}%) {Status}", Math.Round(progress * 100F, 2), status);

            return default;
        }

        private void RebuildScopeList()
        {
            if (_scopes.Count is 0)
            {
                _cachedScopeList = "<No Stage>";
            }
            else
            {
                _cachedScopeList = "(" + string.Join(", ", _scopes) + ")";
            }
        }

        private class ScopeDisposable : IDisposable
        {
            private readonly AsyncStageProgress _progress;
            private readonly string _scope;
            private bool _disposed;

            public ScopeDisposable(AsyncStageProgress progress, string scope)
            {
                _progress = progress ?? throw new ArgumentNullException(nameof(progress));
                _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _progress._scopes.Remove(_scope);
                _progress.RebuildScopeList();
            }
        }
    }
}
