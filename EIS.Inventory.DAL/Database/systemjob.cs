//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EIS.Inventory.DAL.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class systemjob
    {
        public int Id { get; set; }
        public EIS.Inventory.Shared.Models.JobType JobType { get; set; }
        public bool IsNotified { get; set; }
        public EIS.Inventory.Shared.Models.JobStatus Status { get; set; }
        public Nullable<int> CurrentNumOfItems { get; set; }
        public Nullable<int> TotalNumOfItems { get; set; }
        public bool IsAddNewItem { get; set; }
        public bool HasPostAction_1 { get; set; }
        public bool HasPostAction_2 { get; set; }
        public bool HasHeader { get; set; }
        public string SubmittedBy { get; set; }
        public string Parameters { get; set; }
        public string ParametersOut { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public string SupportiveParameters { get; set; }
    }
}
