using BroomService.Models;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BroomService.Services
{
    public class BookingService
    {
        BroomServiceEntities1 _db;

        public BookingService()
        {
            _db = new BroomServiceEntities1();
        }
        public List<JobRequestDetailViewModel> GetBookings(long userId, int jobStatus)
        {
            try
            {
                var jobRequestsVM = (from jr in _db.JobRequests
                                     join ps in _db.JobRequestPropertyServices on jr.Id equals ps.JobRequestId
                                     join p in _db.Properties on ps.PropertyId equals p.Id
                                     join s in _db.SubCategories on ps.ServiceId equals s.Id
                                     where jr.UserId == userId && ps.JobStatus == jobStatus
                                     select new JobRequestDetailViewModel
                                     {
                                         JobRequestId = (int)jr.Id,
                                         JobDescription = jr.Description,
                                         PropertyId = (int)p.Id,
                                         PropertyName = p.Name,
                                         PropertyAddress = p.Address,
                                         FromUserId = (int)jr.UserId,
                                         AssignedWorkerId = (int)ps.AssignedWorker,
                                         StartDateTime = ps.StartDateTime,
                                         JobRequestPropId = (int)ps.JobRequestPropId,
                                         PaymentPageId = jr.PayPageId,
                                         ServiceName = s.Name,
                                         JobStatus = (int)ps.JobStatus,
                                         TimeToDo = (int)ps.TimeToDo,
                                         QuotePrice = (decimal)jr.ServicePrice,
                                         SaleUrl = jr.SaleUrl
                                     }).ToList();
                return jobRequestsVM;
            }
            catch (Exception)
            {
                return new List<JobRequestDetailViewModel>();
            }
        }
    }
}