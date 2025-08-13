using System.ComponentModel.DataAnnotations;

namespace Matrix.DTOs
{
    public class UpdateUserStatusDto
    {
        public Guid UserId { get; set; }
        public int Status { get; set; }
    }
}

