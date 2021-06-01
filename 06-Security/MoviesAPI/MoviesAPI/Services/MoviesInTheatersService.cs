using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoviesAPI.Services
{
    public class MoviesInTheatersService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public MoviesInTheatersService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();
                var today = DateTime.Today;
                var movies = await _context.Movies.Where(x => x.ReleaseDate == today).ToListAsync();
                if(movies.Any())
                {
                    foreach(var movie in movies)
                    {
                        movie.InTheatre = true;
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
