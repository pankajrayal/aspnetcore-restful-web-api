using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI {
    public class MoviesDbContext : DbContext {
        public MoviesDbContext(DbContextOptions options) : base(options) {
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Movie> Movies { get; set; }
    }
}
