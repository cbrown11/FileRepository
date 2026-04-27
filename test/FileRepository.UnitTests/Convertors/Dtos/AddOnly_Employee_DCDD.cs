using System.ComponentModel.DataAnnotations;

namespace FileRepository.UnitTests.Convertors.Dtos
{
    public class AddOnly_Employee_DCDD
    {
        [Required]
        public string Employee_ID { get; set; }

        [Required]
        public string Applicant_Reference_ID { get; set; }

        [Required]
        public string Employee_Type_Reference_ID { get; set; }


        [Required]
        [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Incorrect date format YYYY-MM-DD for Hire_Date")]
        public string Hire_Date { get; set; }

        [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Incorrect date format YYYY-MM-DD for End_Employment_Date")]
        public string End_Employment_Date { get; set; }

        [Required]
        [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Incorrect date format YYYY-MM-DD for Position_Start_Date_for_Conversion")]
        public string Position_Start_Date_for_Conversion { get; set; }

        [RegularExpression("P-[0-9]*", ErrorMessage = "Incorrect  format P-12346")]
        public string Position_Reference_ID { get; set; }

        [Required]
        public string Job_Profile_Reference_ID { get; set; }

        [Required]
        public string Position_Title { get; set; }

        public string Business_Title { get; set; }

        [Required]
        public string Location_Reference_ID { get; set; }

        [Required]
        [RegularExpression("^(Full_time|Part_time)$", ErrorMessage = "Position_Time_Type_Reference_ID is either 'Full_time' or 'Part_time'.")]
        public string Position_Time_Type_Reference_ID { get; set; }

        [Required]
        public string Work_Shift_Reference_ID { get; set; }

        [Required]
        public int Default_Weekly_Hours { get; set; }

        [Required]
        public int Scheduled_Weekly_Hours { get; set; }

        [Required]
        [RegularExpression("^(Salary|Hourly)$", ErrorMessage = "Pay_Rate_Type_Reference_ID is either 'Salary' or 'Hourly'")]
        public string Pay_Rate_Type_Reference_ID { get; set; }

        [Required]
        public string Company_Assignments_Reference_ID { get; set; }

        [Required]
        public string Cost_Center_Assignments_Reference_ID { get; set; }
    }
}
