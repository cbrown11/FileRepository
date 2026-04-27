using System.ComponentModel.DataAnnotations;

namespace FileRepository.UnitTests.Convertors.Dtos
{
    public class TestDto
    {
        [Required]
        public string Employee_ID { get; set; }

        [Required]
        public string Applicant_Reference_ID { get; set; }
    }
}
