using BroomService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.ViewModels
{
    public class PropertyViewModel
    {
        public PropertyVM PropertyModel { get; set; }
        public List<PropertyImageVM> PropertyImages { get; set; }
    }

    public class MainUserProperty
    {
        public List<PropertyViewModel> MainUserProperties { get; set; }

        public List<PropertyViewModel> GrantUserProperties { get; set; }
    }

    public class PropertyVM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool? ShortTermApartment { get; set; }
        public int? FloorNumber { get; set; }
        public int? ApartmentNumber { get; set; }
        public string BuildingCode { get; set; }
        public string LocationOfKey { get; set; }
        public string AccessToProperty { get; set; }
        public int? NoOfBathrooms { get; set; }
        public int? NoOfQueenBeds { get; set; }

        public int? NoOfKingBeds { get; set; }
        public int? NoOfDoubleBeds { get; set; }
        public int? NoOfSingleBeds { get; set; }
        public int? NoOfSingleSofaBeds { get; set; }
        public int? NoOfDoubleSofaBeds { get; set; }
        public bool? Doorman { get; set; }
        public bool? Parking { get; set; }
        public bool? Balcony { get; set; }
        public bool? Dishwasher { get; set; }
        public bool? Pool { get; set; }
        public bool? Garden { get; set; }
        public bool? Elevator { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? NoOfToilets { get; set; }
        public int? NoOfBedRooms { get; set; }
        public string Size { get; set; }
        public long? CountryId { get; set; }
        public long? CityId { get; set; }
        public string WifiLogin { get; set; }
        public bool? CoffeeMachine { get; set; }
        public bool? IsSingleBed { get; set; }
        public bool? IsKingBed { get; set; }
        public bool? IsSofaBed { get; set; }
        public bool? AccesstoCode { get; set; }
    }

    public class PropertySuccessVM
    {
        public long PropertyId { get; set; }

        public string SubUserEmail { get; set; }

        public string SubUserPassword { get; set; }
    }

    public class PropertyImageVM
    {
        public long Id { get; set; }
        public long? PropertyId { get; set; }
        public bool? IsImage { get; set; }
        public bool? IsVideo { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string VideoThumbnail { get; set; }
        public string ImageString { get; set; }
        public string VideoString { get; set; }
    }

    public class PropertyExcelVM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string LocationOfKey { get; set; }
        public string Type { get; set; }
        public string ShortTermApartment { get; set; }
        public int? FloorNumber { get; set; }
        public int? ApartmentNumber { get; set; }
        public string BuildingCode { get; set; }
        public string AccessToProperty { get; set; }
        public int? NoOfBathrooms { get; set; }
        public int? NoOfQueenBeds { get; set; }
        public int? NoOfDoubleBeds { get; set; }
        public int? NoOfSingleBeds { get; set; }
        public int? NoOfSingleSofaBeds { get; set; }
        public int? NoOfDoubleSofaBeds { get; set; }
        public string Doorman { get; set; }
        public string Parking { get; set; }
        public string Balcony { get; set; }
        public string Dishwasher { get; set; }
        public string Pool { get; set; }
        public string Garden { get; set; }
        public string Elevator { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? NoOfToilets { get; set; }
        public int? NoOfBedRooms { get; set; }
        public string Size { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string WifiLogin { get; set; }
        public string CoffeeMachine { get; set; }
        public string IsSingleBed { get; set; }
        public string IsKingBed { get; set; }
        public string IsSofaBed { get; set; }
        public string AccesstoCode { get; set; }
        public Nullable<int> NoOfkingBeds { get; set; }
        public Nullable<decimal> RoomPrice { get; set; }

    }

    public class Prediction
    {
        public string description { get; set; }
        public string id { get; set; }
        public string place_id { get; set; }
        public string reference { get; set; }
        public List<string> types { get; set; }
    }
    public class RootObject
    {
        public List<Prediction> predictions { get; set; }
        public string status { get; set; }
    }
    public class Root
    {
        public string status { get; set; }
        public Content content { get; set; }
    }
    public class Content
    {
        public int items_per_page { get; set; }
        public string page { get; set; }
        public int reviews_count_limit { get; set; }
        public int total_pages { get; set; }
        public int total_items { get; set; }
        public List<List> list { get; set; }
    }
    public class List
    {
        public int id { get; set; }
        public string city { get; set; }
        public string picture_url { get; set; }
        public string thumbnail_url { get; set; }
        public string medium_url { get; set; }
        public string xl_picture_url { get; set; }
        public int user_id { get; set; }
        public int price { get; set; }
        public string native_currency { get; set; }
        public int price_native { get; set; }
        public string price_formatted { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string country { get; set; }
        public string name { get; set; }
        public string smart_location { get; set; }
        public bool has_double_blind_reviews { get; set; }
        public bool instant_bookable { get; set; }
        public int bedrooms { get; set; }
        //public int beds { get; set; }
        public double bathrooms { get; set; }
        public string market { get; set; }
        public int min_nights { get; set; }
        public string neighborhood { get; set; }
        public int person_capacity { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string address { get; set; }
        public string country_code { get; set; }
        public string cancellation_policy { get; set; }
        public string property_type { get; set; }
        public int reviews_count { get; set; }
        public string room_type { get; set; }
        public string room_type_category { get; set; }
        public int picture_count { get; set; }
        public string currency_symbol_left { get; set; }
        public object currency_symbol_right { get; set; }
        public string bed_type { get; set; }
        public string bed_type_category { get; set; }
        public bool require_guest_profile_picture { get; set; }
        public bool require_guest_phone_verification { get; set; }
        public bool force_mobile_legal_modal { get; set; }
        public int cancel_policy { get; set; }
        public int? check_in_time { get; set; }
       // public int check_out_time { get; set; }
        public int guests_included { get; set; }
        public string license { get; set; }
        public int max_nights { get; set; }
        public object square_feet { get; set; }
        public string locale { get; set; }
        public bool? has_viewed_terms { get; set; }
        public object has_viewed_cleaning { get; set; }
        public bool? has_agreed_to_legal_terms { get; set; }
        public object has_viewed_ib_perf_dashboard_panel { get; set; }
        public string language { get; set; }
        public string public_address { get; set; }
        public string map_image_url { get; set; }
        public string experiences_offered { get; set; }
        public int? max_nights_input_value { get; set; }
        public int min_nights_input_value { get; set; }
        public bool requires_license { get; set; }
        public int property_type_id { get; set; }
        public string house_rules { get; set; }
        public int? security_deposit_native { get; set; }
        public int? security_price_native { get; set; }
        public string security_deposit_formatted { get; set; }
        public string description { get; set; }
        public string description_locale { get; set; }
        public string summary { get; set; }
        public string space { get; set; }
        public string access { get; set; }
        public string interaction { get; set; }
        public string neighborhood_overview { get; set; }
        public string transit { get; set; }
        public List<string> amenities { get; set; }
        public bool is_location_exact { get; set; }
        public string cancel_policy_short_str { get; set; }
       // public double star_rating { get; set; }
        public int price_for_extra_person_native { get; set; }
        public int? weekly_price_native { get; set; }
        public int? monthly_price_native { get; set; }
        public string time_zone_name { get; set; }
        public Loc loc { get; set; }
        public bool exists { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int? cleaning_fee_native { get; set; }
        public int? extras_price_native { get; set; }
        public bool in_building { get; set; }
        public bool in_toto_area { get; set; }
        public bool instant_book_enabled { get; set; }
        public bool? is_business_travel_ready { get; set; }
        public int? listing_cleaning_fee_native { get; set; }
        public int? listing_monthly_price_native { get; set; }
        public int listing_price_for_extra_person_native { get; set; }
        public int? listing_weekend_price_native { get; set; }
        public int? listing_weekly_price_native { get; set; }
        public string localized_city { get; set; }
       // public double monthly_price_factor { get; set; }
        public object special_offer { get; set; }
        public object toto_opt_in { get; set; }
        //public double? weekly_price_factor { get; set; }
        //public object wireless_info { get; set; }
        public int host_id { get; set; }
        public int airbnb_id { get; set; }
    }
    public class Loc
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }
    public class PropertyModelApi
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class PropertyModelData
    {
        public long? PropertyId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
    public class SubUserPropDetail
    {
        public string LoginId { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public int? FloorNumber { get; set; }
        public int? ApartmentNumber { get; set; }
        public string BuildingCode { get; set; }
        public string AccessToProperty { get; set; }
        public string Address { get; set; }
        public string WifiLogin { get; set; }
        public string LocationOfKey { get; set; }
        public long? SubUserId { get; set; }
        public bool status { get; set; }
    }
}