using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesAPI.DTOs
{
    public class MoviePatchDTO
    {
        [Required]
        [StringLength(300)]
        public string Title { get; set; }
        public string Summary { get; set; }
        public bool InTheatre { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
