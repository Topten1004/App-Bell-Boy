using BroomService.Resources;
using BroomService.Services;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BroomService.ApiControllers
{
    public class BookingsController : ApiController
    {
        OrderService orderService;

        public BookingsController()
        {
            orderService = new OrderService();
        }

        #region Job Requests

        /// <summary>
        /// Get Customer JobRequests Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetCustomerJobRequests(long userId)
        {
            var result = orderService.GetCustomerJobRequests(userId,0);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }

        [HttpGet]
        public IHttpActionResult GetWorkerJobRequests(long userId)
        {
            var result = orderService.GetWorkerJobRequests(userId);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }

        /// <summary>
        /// Get JobRequest Detail Api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetJobRequestDetail(long booking_id)
        {
            var result = orderService.BookingsDetails(booking_id);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }

        /// <summary>
        /// Start End Timer api
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult StartEndTimer(UpdateTimerTimeModel model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var timerStartedDate = orderService.StartEndTimer(model);

            return this.Ok(new
            {
                status = timerStartedDate!=null?true:false,
                message = orderService.message,
                TimerStartedDate= timerStartedDate
            });
        }


        /// <summary>
        /// Complete Job Request api
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CompleteJobRequest(CompleteJobRequestModel model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var status = orderService.CompleteJobRequest(model);

            return this.Ok(new
            {
                status = status,
                message = orderService.message,
            });
        }

        #endregion

        #region Fast Orders
        public IHttpActionResult GetFastOrderByPropertyId(long property_id,long user_id)
        {
            List<JobRequestViewModel> result = orderService.GetFastOrdersByPropertyId(property_id,user_id);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }

        public IHttpActionResult GetFastOrders(long user_id)
        {
            List<JobRequestViewModel> result = orderService.GetFastOrdersForUsers(user_id);
            return this.Ok(new
            {
                status = result == null ? false : true,
                message = orderService.message,
                data = result
            });
        }
        #endregion

        #region Reviews

        /// <summary>
        /// Submit User review which can be provider,worker,supervisor by the customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SubmitUserReview(UserReviewModel model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var status = orderService.SubmitUserReview(model);

            return this.Ok(new
            {
                status = status,
                message = orderService.message,
            });
        }

        /// <summary>
        /// Submit job review which will be send by the client for the job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost]
        public IHttpActionResult JobUserReview(UserReviewModel model)
        {
            if (model == null)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var status = orderService.SubmitJobReview(model);

            return this.Ok(new
            {
                status = status,
                message = orderService.message,
            });
        }

        public IHttpActionResult GetReviews(long UserId)
        {
            if (UserId == null||UserId==0)
            {
                return this.Ok(new
                {
                    status = false,
                    message = Resource.fill_required_records,
                });
            }
            var reviewList = orderService.GetUserReviews(UserId);

            return this.Ok(new
            {
                status = reviewList.Count>0?true:false,
                message = orderService.message,
                ReviewList=reviewList
            });
        }

        [HttpGet]
        public IHttpActionResult CheckUserReview(long fromUserId, long toUserId)
        {
            var reviewList = orderService.CheckUserReview(fromUserId,toUserId);

            return this.Ok(new
            {
                status = reviewList!=null ? true : false,
                message = orderService.message,
                ReviewList = reviewList
            });
        }

        [HttpGet]
        public IHttpActionResult CheckJobReview(long jobRequestId)
        {
            var reviewList = orderService.CheckJobReview(jobRequestId);

            return this.Ok(new
            {
                status = reviewList != null ? true : false,
                message = orderService.message,
                ReviewList = reviewList
            });
        }

        [HttpGet]
        public IHttpActionResult GetWorkerAvgRating()
        {
            var reviewList = orderService.GetWorkerAvgRating();
            return this.Ok(new
            {
                status = reviewList.Count > 0 ? true : false,
                message = orderService.message,
                ReviewList = reviewList
            });
        }
        #endregion

        #region Make Cancel Refund

        #endregion
    }
}
