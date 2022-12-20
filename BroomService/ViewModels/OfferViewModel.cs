using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BroomService.ViewModels
{
    public class OfferViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string OfferName { get; set; }
        public string OfferDesc { get; set; }
        public int Status  { get; set; }
    }
    public class  ChangeOfferStatus
    {
        public int OfferId { get; set; }
        public int OfferStatus { get; set; }
        public long UserId { get; set; }
    }
    public class ChangeQuotesStatus
    {
        public int QuotesId { get; set; }
        public int QuotesStatus { get; set; }
        public long UserId { get; set; }
    }
    public class SendQuotesRequest
    {
        public int PropertyId { get; set; }
        public int ServiceId { get; set; }
        public int SubCatId { get; set; }
        public int SubSubCatId { get; set; }
        public long UserId { get; set; }
    }
    public class AcceptRejectServiceRequest
    {
        public long ServiceReqId { get; set; }
        public int Status { get; set; }
        public long UserId { get; set; }
    }
    public class InventoryReqest
    {
        public string InventoryName { get; set; }
        public long UserId { get; set; }
    }
    public class AddServiceRequest
    {
        public int CatId { get; set; }
        public int SubCatId { get; set; }
        public int SubSubCatId { get; set; }
        public long UserId { get; set; }
        public string JobDateTime { get; set; }
        public long PropertyId { get; set; }
        public string Message { get; set; }
        public long WorkerId { get; set; }
        public string ServicePrice { get; set; }
    }
    public class GetAddServiceRequestDetail
    {
        public long UserId { get; set; }
        public SelectListItem Property { get; set; }
        public string JobStartDate { get; set; }
        public List<SelectListItem> CategoryList { get; set; }
    }
    public class GetAddServiceModelApi
    {
        public long UserId { get; set; }
        public long JobId { get; set; }
    }
    public class DeliveryDoneApi
    {
        public long DeliveryGuyId { get; set; }
        public long JobReqPropServiceId { get; set; }
    }

    public class GetWorkerRating
    {
        public long? UserId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int? Rating { get; set; }
        public int TotalService { get; set; }
    }
    public class DeliveryGuyDetail
    {
        public long? JobReqPropServiceId { get; set; }
        public long? DeliveryGuyId { get; set; }
        public string DeliveryGuyName { get; set; }
        public bool? IsDeliveryDone { get; set; }
        public long? WorkerId { get; set; }
        public string WorkerName { get; set; }
        public string WorkerContactNo { get; set; }       
        public DateTime? JobDateTime { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public long? PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public List<DeliveryGuyInventory> Inventory{ get; set; }
    }
    public class DeliveryGuyInventory
    {
        public long? InventoryId { get; set; }
        public string InventoryName { get; set; }
        public long? PropertyId { get; set; }
        public int? Qty { get; set; }
        public string Image { get; set; }
    }
}