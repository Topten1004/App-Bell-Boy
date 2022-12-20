using BroomService.Models;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.ViewModels
{
    public class JobRequestViewModel
    {
        public List<JobRequestCheckListModel> CheckListData { get; set; }
        public Category CategoryData { get; set; }
        public long JobStatus { get; set; }
        public SubCategory SubCategoryData { get; set; }
        public SubSubCategory SubSubCategoryData { get; set; }
        public string JobStartDateTime { get; set; }
        public string JobEndDateTime { get; set; }
        public List<WorkersJobs> JobData { get; set; }
        public long Id { get; set; }
        public string DocReceipt_Url { get; set; }       
        public List<string> IdReceipt { get; set; }
        public string JobDesc { get; set; }
        public List<long> Property_List_Id { get; set; }
        public long PropertyId { get; set; }
        public List<long> Schedule_list { get; set; }
        public long? UserId { get; set; }
        public List<string> CheckList { get; set; }
        public List<ServiceData> Categories { get; set; }
        public List<PropertyServiceData> PropertyService { get; set; }
        public List<ChecklistImageVM> ReferenceImages { get; set; }
        public List<InventoryItems> InventoryList { get; set; }
        public ICountResponse DocumentJob { get; set; }
        public bool? IsFastOrder { get; set; }
        public bool? IsAddedBySupervisor { get; set; }
        public string FastOrderName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerImage { get; set; }
        public long CustomerId { get; set; }
        public List<JobRequestCheckListModel> CheckListDetails { get; set; }
        public int PaymentInfo { get; set; }
        public bool isPaymentDone { get; set; }
        public bool? RequestSupervisior { get; set; }
        public bool? IsVisitor { get; set; }
        public string ServicePrice { get; set; }
        public long ServiceId { get; set; }
        public string PropertyName { get; set; }

        public bool HasPrice { get; set; }
    }

    public class JobRequestDetailViewModel
    {
        public int JobRequestId { get; set; }
        public string JobDescription { get; set; }
        public int JobRequestPropId { get; set; }
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public int FromUserId { get; set; }
        public int AssignedWorkerId { get; set; }
        public DateTime? StartDateTime { get; set; }
        public string PaymentPageId { get; set; }
        public string ServiceName { get; set; }
        public string CategoryName { get; set; }
        public int JobStatus { get; set; }
        public int TimeToDo { get; set; }
        public string SaleUrl { get; set; }
        public decimal QuotePrice { get; set; }
        public decimal? ServicePrice { get; set; }

    }

    public class EditJobRequestViewModel
    {
       // public List<JobRequestCheckListModel> CheckListData { get; set; }
        public Category CategoryData { get; set; }
        public int? JobStatus { get; set; }
       // public SubSubCategoryDetail SubSubCategoryDetail { get; set; }
         public SubCategory SubCategoryData { get; set; }
        public SubSubCategory SubSubCategoryData { get; set; }
        public DateTime? JobStartDateTime { get; set; }
        public DateTime? JobEndDateTime { get; set; }
       // public List<WorkersJobs> JobData { get; set; }
        public long JobReqId { get; set; }
        //public long Id { get; set; }
        //public List<string> IdReceipt { get; set; }
        public string JobDesc { get; set; }
        public  long Property_List_Id { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public long? AssignWorker { get; set; }
       // public List<string> CheckList { get; set; }
        public List<ServiceData> Categories { get; set; }
       // public List<PropertyServiceData> PropertyService { get; set; }
       // public List<ChecklistImageVM> ReferenceImages { get; set; }
        public List<InventoryViewModel> InventoryList { get; set; }
       // public ICountResponse DocumentJob { get; set; }

        //public string CustomerName { get; set; }
        //public string CustomerImage { get; set; }
        //public long CustomerId { get; set; }
        public List<JobRequestCheckList> CheckListDetails { get; set; }
        public List<SelectListItem> Properties { get; set; }
    }
    public class EditJobRequestApiModel
    {
        // public List<JobRequestCheckListModel> CheckListData { get; set; }
        public CategoryModel CategoryData { get; set; }
        public int? JobStatus { get; set; }
        // public SubSubCategoryDetail SubSubCategoryDetail { get; set; }
        public SubCategoryModel SubCategoryData { get; set; }
        public SubSubCategoryModel SubSubCategoryData { get; set; }
        public DateTime? JobStartDateTime { get; set; }
        public DateTime? JobEndDateTime { get; set; }
        // public List<WorkersJobs> JobData { get; set; }
        public long JobReqId { get; set; }
        //public long Id { get; set; }
        //public List<string> IdReceipt { get; set; }
        public string JobDesc { get; set; }
        public long Property_List_Id { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public long? AssignWorker { get; set; }
        // public List<string> CheckList { get; set; }
        public List<ServiceData> Categories { get; set; }
        // public List<PropertyServiceData> PropertyService { get; set; }
        // public List<ChecklistImageVM> ReferenceImages { get; set; }
        public List<InventoryViewModel> InventoryList { get; set; }
        // public ICountResponse DocumentJob { get; set; }

        //public string CustomerName { get; set; }
        //public string CustomerImage { get; set; }
        //public long CustomerId { get; set; }
        public List<JobRequestCheckListModel> CheckListDetails { get; set; }
        public List<SelectListItem> Properties { get; set; }
    }
    public class EditJobApiData
    {
        public DateTime? JobStartDateTime { get; set; }
        public DateTime? JobEndDateTime { get; set; }
        public long JobReqId { get; set; }
        public string JobDesc { get; set; }
        public long Property_List_Id { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public long? AssignWorker { get; set; }
    }
    //public class GetcategoryDetailByJobId
    //{
    //    public long UserId { get; set; }
    //    public long CategoryId { get; set; }
    //    public long? SubCategoryId { get; set; }
    //    public long? SubSubCategoryId { get; set; }
    //}
    public class WorkersJobs
    {
        // changes code
        public string PropertyImage { get; set; }
        //
        public long? OrderId { get; set; }
        public long? JobPropId { get; set; }
        public long? CustomerId { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public long? SubSubCategoryId { get; set; }
        public string CustomerName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerImage { get; set; }
        public string CustomerImage { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public long? PropertyId { get; set; }
        public long? WorkerId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public string ServiceName { get; set; }
        public string SubServiceName { get; set; }
        public string SubSubServiceName { get; set; }
        public string JobDate { get; set; }
        public string JobTime { get; set; }
        public DateTime? JobDateTime { get; set; }
        public string TimeToDo { get; set; }
        public int? JobStatus { get; set; }
        public bool IsStarted { get; set; }
        public DateTime? TimerStartedTime { get; set; }
        public DateTime? TimerEndTime { get; set; }
        public List<InventoryItems> InventoryDetails { get; set; }
    }

    public class ICountResponse
    {
        public bool status { get; set; }
        public string reason { get; set; }
        public string error_description { get; set; }
        public string[] error_details { get; set; }
        public int client_id { get; set; }
        public string custom_client_id { get; set; }
        public string doctype { get; set; }
        public int docnum { get; set; }
        public string doc_url { get; set; }
        public string doc_copy_url { get; set; }
        public bool success { get; set; }
        public int confirmation_code { get; set; }
        public string cc_type { get; set; }
        public string paypage_id { get; set; }
        public string sale_uniqid { get; set; }
        public string sale_sid { get; set; }
        public string sale_url { get; set; }

    }


    public class ICountSalesResponse
    {
        public bool status { get; set; }
        public string reason { get; set; }
        //public string version { get; set; }
        //public string tz { get; set; }
        //public string ts { get; set; }
        //public string lang { get; set; }
        //public string rid { get; set; }
        //public string module { get; set; }
        //public string method { get; set; }       
        public string paypage_id { get; set; }
        public string sale_uniqid { get; set; }
        public string sale_sid { get; set; }
        public string sale_url { get; set; }
        public string error_description { get; set; }
        public string error_details { get; set; }
        public string paypage_url { get; set; }        

    }



    public class ICountCCardResponse
    {
        public bool status { get; set; }
        public string reason { get; set; }
        public string error_description { get; set; }
        public string[] error_details { get; set; }
        public int client_id { get; set; }
        public string custom_client_id { get; set; }
        public int cc_token_id { get; set; }
        public string token_id { get; set; }
        public string token { get; set; }
    }

    public class MainUserJobsVM
    {
        public List<JobRequestViewModel> MainUserJobs { get; set; }
        public List<JobRequestViewModel> GrantUserJobs { get; set; }
        public List<JobRequestViewModel> SubUserJobs { get; set; }
    }

    public class PropertyServiceData
    {
        public long AssignedUserId { get; set; }
        public bool Type { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime StartDateTime { get; set; }
        public long PropertyId { get; set; }
        public long ServiceId { get; set; }
        public int TimeToDo { get; set; }
    }

    public class GetWorkers
    {
        public bool IsWorker { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Date { get; set; }
        public int? Day { get; set; }
        public int? Rating { get; set; }
        public string FromTime { get; set; }
        public int FromTimeInt { get; set; }
        public string ToTime { get; set; }
        public long UserId { get; set; }
        public string WorkerType { get; set; }
        public decimal TimeToDo { get; set; }
    }

    public class ServiceData
    {
        public long? Id { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public long? SubSubCategoryyId { get; set; }
        public long? PropertyId { get; set; }
        public string SelectedTime { get; set; }
        public int Day { get; set; }
    }
    public class GetWorkerTiming
    {
        public long subCategoryId { get; set; }
        public long SubSubCategoryyId { get; set; }
        public string JobDate { get; set; }
        public string price { get; set; }
        public int dayofweek { get; set; }
    }

    public class JobOptionModel
    {
        public long ClientId { get; set; }
        public bool IsWorker { get; set; }
        public long CategoryId { get; set; }
        public int JoPropId { get; set; }
        public DateTime StartDate { get; set; }
        public long WorkerId { get; set; }
    }

    public class InventoryItems
    {
        public long? InventoryId { get; set; }
        public string InventoryName { get; set; }
        public long? PropertyId { get; set; }
        public string PropertyName { get; set; }
        public int? Qty { get; set; }
        public bool? IsDeliveryGuy { get; set; }
        public long DeliveryGuyId { get; set; }
    }
    public class ChecklistImageVM
    {
        public bool? IsImage { get; set; }
        public bool? IsVideo { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
    }

    public class InventoryViewModel
    {
        public long InventoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int? Stock { get; set; }
        public int? Price { get; set; }
        public int? Qty { get; set; }
        public long? JobReqPropServiceId { get; set; }
        public long? PropertyId { get; set; }
        public long DeliveryGuyId { get; set; }
    }

    public class UserReviewModel
    {
        public long? CustomerId { get; set; }
        public long? ToUserId { get; set; }
        public int? UserType { get; set; }
        public int? UserRating { get; set; }
        public string UserReview { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerImage { get; set; }
        public string WorkerName { get; set; }
        public string WorkerImage { get; set; }
        public long? JobRequestId { get; set; }
    }

    public class UpdateTimerTimeModel
    {
        public long UserId { get; set; }
        public long JobRequestId { get; set; }
        public TimeSpan TimerTime { get; set; }
        public bool IsStart { get; set; }
    }
    public class CompleteJobRequestModel
    {
        public long UserId { get; set; }
        public long JobRequestId { get; set; }
    }

    public class ServicePriceModel
    {
        public string status { get; set; }
        public string message { get; set; }
        public string price { get; set; }
    }

    public class ServiceCategories
    {
        public string CategoryId { get; set; }
        public string SubCategoryId { get; set; }
        public string SubSubCategoryyId { get; set; }
    }
    public class JobRequestPaymentDetail
    {
        public long? CustomerId { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public long? SubSubCategoryId { get; set; }
        public string CustomerImage { get; set; }
        public string CustomerName { get; set; }
        public string PropertyName { get; set; }
        public DateTime? StartDateTime { get; set; }
        public string JobDate { get; set; }
        public long? Id { get; set; }
        public string JobDesc { get; set; }
        // public List<long> Property_List_Id { get; set; }
        public List<PropertyServiceData> PropertyService { get; set; }
        //public long? SupervisorId { get; set; }
        //public List<string> CheckList { get; set; }
        //public List<ServiceData> Categories { get; set; }
        //public List<ChecklistImageVM> ReferenceImages { get; set; }
        public bool? IsFastOrder { get; set; }
        public bool? IsAddedBySupervisor { get; set; }
        public string FastOrderName { get; set; }
        //public List<WorkersJobs> JobData { get; set; }
        //public List<InventoryItems> InventoryDetails { get; set; }
        //public List<JobRequestCheckListModel> CheckListDetails { get; set; }
    }
    public class CheckList
    {
        public string CheckListText { get; set; }
    }
    public class EditJob
    {
        public long JobReqId { get; set; }
        public long UserId { get; set; }
    }
    public class CategoryModel
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public bool? HasPrice { get; set; }
        public string Icon { get; set; }
        public string Name_Russian { get; set; }
        public string Name_Hebrew { get; set; }
        public string Name_French { get; set; }
        public string Description { get; set; }
        public string Description_Russian { get; set; }
        public string Description_Hebrew { get; set; }
        public string Description_French { get; set; }
    }
    public class SubCategoryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? CatId { get; set; }
        public string Picture { get; set; }
        public string Icon { get; set; }
        public decimal? ClientPrice { get; set; }
        public decimal? Price { get; set; }
        public bool? HasSubSubCategories { get; set; }
        public string Name_Russian { get; set; }
        public string Name_Hebrew { get; set; }
        public string Name_French { get; set; }
        public string Description { get; set; }
        public string Description_Russian { get; set; }
        public string Description_Hebrew { get; set; }
        public string Description_French { get; set; }
    }
    public class SubSubCategoryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? SubCatId { get; set; }
        public decimal? ClientPrice { get; set; }
        public decimal? Price { get; set; }
        public string Picture { get; set; }
        public string Icon { get; set; }
        public string Name_Russian { get; set; }
        public string Name_French { get; set; }
        public string Name_Hebrew { get; set; }
        public string Description_French { get; set; }
        public string Description_Russian { get; set; }
        public string Description_Hebrew { get; set; }
        public string Description { get; set; }
    }
    public  class JobRequestCheckListModel
    {
        public long Id { get; set; }
        public string TaskDetail { get; set; }
        public long? JobRequestId { get; set; }
        public bool? IsDone { get; set; }
    }
    public class SaveChecklistModel
    {
        public long UserId { get; set; }
        public long PropId { get; set; }
        public int ServiceId { get; set; }
        public int SubCatId { get; set; }
        public int SubSubCatId { get; set; }
        public string ChecklistName { get; set; }
        public List<CheckList> Chklist { get; set; }
    }
    public class MondayPopupDetail
    {
        public long UserId { get; set; }
        public long? JobId { get; set; }
        public long? JobReqPropServiceId { get; set; }
        public long? PropId { get; set; }
        public long? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public bool  IsUpcommingService { get; set; }
        public bool? Later { get; set; }
    }
    public class SaveReceiptDoc
    {
        public long JobReqId { get; set; }
        public string Doc_Url { get; set; }
    }
}
