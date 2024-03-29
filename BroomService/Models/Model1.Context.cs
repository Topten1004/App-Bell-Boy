﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BroomService.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class BroomServiceEntities1 : DbContext
    {
        public BroomServiceEntities1()
            : base("name=BroomServiceEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AboutU> AboutUs { get; set; }
        public virtual DbSet<Card> Cards { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<ChannelManager> ChannelManagers { get; set; }
        public virtual DbSet<CheckRequest> CheckRequests { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<ContactU> ContactUs { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<DocumentJob> DocumentJobs { get; set; }
        public virtual DbSet<GreetingMessage> GreetingMessages { get; set; }
        public virtual DbSet<IncomeSource> IncomeSources { get; set; }
        public virtual DbSet<Inventory> Inventories { get; set; }
        public virtual DbSet<JobInventory> JobInventories { get; set; }
        public virtual DbSet<JobRequest> JobRequests { get; set; }
        public virtual DbSet<JobRequestCheckList> JobRequestCheckLists { get; set; }
        public virtual DbSet<JobRequestPropertyService> JobRequestPropertyServices { get; set; }
        public virtual DbSet<JobRequestQuoteDetail> JobRequestQuoteDetails { get; set; }
        public virtual DbSet<JobRequestQuoteType> JobRequestQuoteTypes { get; set; }
        public virtual DbSet<JobRequestRefImage> JobRequestRefImages { get; set; }
        public virtual DbSet<JobRequestSubSubCategory> JobRequestSubSubCategories { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<MeetingSchedulRequest> MeetingSchedulRequests { get; set; }
        public virtual DbSet<MonthInvoice> MonthInvoices { get; set; }
        public virtual DbSet<MonthInvoiceService> MonthInvoiceServices { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<OccupiedProvider> OccupiedProviders { get; set; }
        public virtual DbSet<Offer> Offers { get; set; }
        public virtual DbSet<PrivacyPolicy> PrivacyPolicies { get; set; }
        public virtual DbSet<Property> Properties { get; set; }
        public virtual DbSet<PropertyJobRequest> PropertyJobRequests { get; set; }
        public virtual DbSet<PropertyReport> PropertyReports { get; set; }
        public virtual DbSet<PropertyVisit> PropertyVisits { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<RejectJobRequest> RejectJobRequests { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<ServiceAutoPrice> ServiceAutoPrices { get; set; }
        public virtual DbSet<ServicePriceChangeRequest> ServicePriceChangeRequests { get; set; }
        public virtual DbSet<ServiceQuote> ServiceQuotes { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<SubCategory> SubCategories { get; set; }
        public virtual DbSet<SubSubCategory> SubSubCategories { get; set; }
        public virtual DbSet<SubUserRequest> SubUserRequests { get; set; }
        public virtual DbSet<tblAddInventoryRequest> tblAddInventoryRequests { get; set; }
        public virtual DbSet<tblAddServiceRequest> tblAddServiceRequests { get; set; }
        public virtual DbSet<tblAskForService> tblAskForServices { get; set; }
        public virtual DbSet<tblCategoryShift> tblCategoryShifts { get; set; }
        public virtual DbSet<tblEditJobRequest> tblEditJobRequests { get; set; }
        public virtual DbSet<tblGrantAccess> tblGrantAccesses { get; set; }
        public virtual DbSet<tblGrantProperty> tblGrantProperties { get; set; }
        public virtual DbSet<tblMessage> tblMessages { get; set; }
        public virtual DbSet<tblMessageType> tblMessageTypes { get; set; }
        public virtual DbSet<tblMission> tblMissions { get; set; }
        public virtual DbSet<tblPropertyImage> tblPropertyImages { get; set; }
        public virtual DbSet<tblProviderAvailableTime> tblProviderAvailableTimes { get; set; }
        public virtual DbSet<tblProviderScheduleByDate> tblProviderScheduleByDates { get; set; }
        public virtual DbSet<tblSavedChecklist> tblSavedChecklists { get; set; }
        public virtual DbSet<tblSavedChecklistDetail> tblSavedChecklistDetails { get; set; }
        public virtual DbSet<tblVisitor> tblVisitors { get; set; }
        public virtual DbSet<TermsCondition> TermsConditions { get; set; }
        public virtual DbSet<Testimonial> Testimonials { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserChannelManager> UserChannelManagers { get; set; }
        public virtual DbSet<UserChannelManagerSetting> UserChannelManagerSettings { get; set; }
        public virtual DbSet<UserChat> UserChats { get; set; }
        public virtual DbSet<UserJobReview> UserJobReviews { get; set; }
        public virtual DbSet<UserReview> UserReviews { get; set; }
        public virtual DbSet<UserSubCategory> UserSubCategories { get; set; }
        public virtual DbSet<WorkerQuestion> WorkerQuestions { get; set; }
        public virtual DbSet<LaundryRequest> LaundryRequests { get; set; }
    }
}
