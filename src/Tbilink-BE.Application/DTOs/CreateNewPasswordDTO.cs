using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tbilink_BE.Application.DTOs
{
    public class CreateNewPasswordDTO
    {
        public required string code {  get; set; }
        public required string Email {  get; set; }
        public required string Password { get; set; }
        public required string RepPassword { get; set; }
    }
}
