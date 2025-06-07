using System.ComponentModel.DataAnnotations;

namespace EmployeeScheduling.API.Models
{
    public enum AssignmentStatus
    {
        [Display(Name = "Assigned")]
        Assigned = 0,
        
        [Display(Name = "Confirmed")]
        Confirmed = 1,
        
        [Display(Name = "Declined")]
        Declined = 2,
        
        [Display(Name = "In Progress")]
        InProgress = 3,
        
        [Display(Name = "Completed")]
        Completed = 4,
        
        [Display(Name = "No Show")]
        NoShow = 5
    }

    public enum AvailabilityType
    {
        [Display(Name = "Available")]
        Available = 0,
        
        [Display(Name = "Preferred")]
        Preferred = 1,
        
        [Display(Name = "Unavailable")]
        Unavailable = 2
    }
}