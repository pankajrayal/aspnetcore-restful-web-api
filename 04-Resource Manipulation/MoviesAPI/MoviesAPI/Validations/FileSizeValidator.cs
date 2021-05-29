using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesAPI.Validations
{
    public class FileSizeValidator: ValidationAttribute
    {
        private readonly int _maxFileSizeInMbs;

        public FileSizeValidator(int MaxFileSizeInMbs)
        {
            _maxFileSizeInMbs = MaxFileSizeInMbs;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null)
            {
                return ValidationResult.Success;
            }
            IFormFile fromFile = value as IFormFile;
            if(fromFile == null)
            {
                return ValidationResult.Success;
            }

            if(fromFile.Length> _maxFileSizeInMbs * 1024 * 1024)
            {
                return new ValidationResult($"File size cannot be bigger than {_maxFileSizeInMbs} megabytes.");
            }
            return ValidationResult.Success;
        }
    }
}
