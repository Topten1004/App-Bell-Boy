using BroomService.Models;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;

namespace BroomService.Services
{
    public class WorkerService
    {
        BroomServiceEntities1 _db;
        public WorkerService()
        {
            _db = new BroomServiceEntities1();
        }
        public List<AvailableTime> GetAvailableScheduleList(long user_id, bool isNextWeek = false, DateTime? vDate = null)
        {
            if (vDate == null) vDate = DateTime.Now;
            int weekNumber = GetWeekNumber(vDate.Value);
            if (isNextWeek) weekNumber = weekNumber + 1;
            int year = vDate.Value.Year;

            List<AvailableTime> vMs = new List<AvailableTime>();
            try
            {
                vMs = _db.tblProviderAvailableTimes.Where(a => a.provider_id == user_id && a.week_of_year == weekNumber && a.year == year).AsEnumerable().Select(a => new AvailableTime
                {
                    from_time = a.from_time,
                    to_time = a.to_time,
                    user_name = a.User.FullName,
                    day_of_week = a.day_of_week,
                    id = a.Id,
                    isCausallOff = a.isCausallOff.Value,
                    isOptionalOff = a.isOptionalOff.Value,
                    week_of_year = a.week_of_year.Value,
                    year = a.year.Value,
                    isNextWeek = isNextWeek,
                    vDate = a.vDate,
                    scheduleDate = a.vDate.Value.ToShortDateString(),
                    user_id = a.User.UserId
                }).ToList();

                vMs = vMs.OrderBy(a => a.day_of_week).ToList();
                var checkIfSunday = vMs.Where(a => a.day_of_week == 0).FirstOrDefault();
                var checkIfMonday = vMs.Where(a => a.day_of_week == 1).FirstOrDefault();
                var checkIfTuesday = vMs.Where(a => a.day_of_week == 2).FirstOrDefault();
                var checkIfWednesday = vMs.Where(a => a.day_of_week == 3).FirstOrDefault();
                var checkIfThrusday = vMs.Where(a => a.day_of_week == 4).FirstOrDefault();
                var checkIfFriday = vMs.Where(a => a.day_of_week == 5).FirstOrDefault();
                var checkIfSaturday = vMs.Where(a => a.day_of_week == 6).FirstOrDefault();

                string defaultStart = "08:00";
                string defaultEnd = "23:59";

                if (checkIfSunday == null)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.from_time = defaultStart;
                    availableTime.to_time = defaultEnd;
                    availableTime.day_of_week = 0;

                    availableTime.week_of_year = weekNumber;
                    availableTime.year = year;
                    availableTime.user_id = user_id;
                    availableTime.vDate = GetWeekFirstDay(0, isNextWeek);
                    availableTime.scheduleDate = GetWeekFirstDay(0, isNextWeek).ToShortDateString();
                    availableTime.userType = 3;
                    availableTime.isCausallOff = false;
                    availableTime.isOptionalOff = false;

                    long Sundayid = SaveChanges(availableTime);
                    availableTime.id = Sundayid;

                    vMs.Insert(0, availableTime);
                }

                if (checkIfMonday == null)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.from_time = defaultStart;
                    availableTime.to_time = defaultEnd;
                    availableTime.day_of_week = 1;

                    availableTime.week_of_year = weekNumber;
                    availableTime.year = year;
                    availableTime.user_id = user_id;
                    availableTime.vDate = GetWeekFirstDay(1, isNextWeek);
                    availableTime.scheduleDate = GetWeekFirstDay(1, isNextWeek).ToShortDateString();
                    availableTime.userType = 3;
                    availableTime.isCausallOff = false;
                    availableTime.isOptionalOff = false;


                    long Mondayid = SaveChanges(availableTime);
                    availableTime.id = Mondayid;
                    vMs.Insert(1, availableTime);
                }

                if (checkIfTuesday == null)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.from_time = defaultStart;
                    availableTime.to_time = defaultEnd;
                    availableTime.day_of_week = 2;

                    availableTime.week_of_year = weekNumber;
                    availableTime.year = year;
                    availableTime.user_id = user_id;
                    availableTime.vDate = GetWeekFirstDay(2, isNextWeek);
                    availableTime.scheduleDate = GetWeekFirstDay(2, isNextWeek).ToShortDateString();
                    availableTime.userType = 3;
                    availableTime.isCausallOff = false;
                    availableTime.isOptionalOff = false;


                    long Tuesdayid = SaveChanges(availableTime);
                    availableTime.id = Tuesdayid;

                    vMs.Insert(2, availableTime);
                }

                if (checkIfWednesday == null)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.from_time = defaultStart;
                    availableTime.to_time = defaultEnd;
                    availableTime.day_of_week = 3;

                    availableTime.week_of_year = weekNumber;
                    availableTime.year = year;
                    availableTime.user_id = user_id;
                    availableTime.vDate = GetWeekFirstDay(3, isNextWeek);
                    availableTime.scheduleDate = GetWeekFirstDay(3, isNextWeek).ToShortDateString();
                    availableTime.userType = 3;
                    availableTime.isCausallOff = false;
                    availableTime.isOptionalOff = false;


                    long Wednesdayid = SaveChanges(availableTime);
                    availableTime.id = Wednesdayid;
                    vMs.Insert(3, availableTime);
                }

                if (checkIfThrusday == null)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.from_time = defaultStart;
                    availableTime.to_time = defaultEnd;
                    availableTime.day_of_week = 4;

                    availableTime.week_of_year = weekNumber;
                    availableTime.year = year;
                    availableTime.user_id = user_id;
                    availableTime.vDate = GetWeekFirstDay(4, isNextWeek);
                    availableTime.scheduleDate = GetWeekFirstDay(4, isNextWeek).ToShortDateString();
                    availableTime.userType = 3;
                    availableTime.isCausallOff = false;
                    availableTime.isOptionalOff = false;


                    long Thrusdayid = SaveChanges(availableTime);
                    availableTime.id = Thrusdayid;
                    vMs.Insert(4, availableTime);
                }

                if (checkIfFriday == null)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.from_time = defaultStart;
                    availableTime.to_time = defaultStart;
                    availableTime.day_of_week = 5;

                    availableTime.week_of_year = weekNumber;
                    availableTime.year = year;
                    availableTime.user_id = user_id;
                    availableTime.vDate = GetWeekFirstDay(5, isNextWeek);
                    availableTime.scheduleDate = GetWeekFirstDay(5, isNextWeek).ToShortDateString();
                    availableTime.userType = 3;
                    availableTime.isCausallOff = false;
                    availableTime.isOptionalOff = true;


                    long Fridayid = SaveChanges(availableTime);
                    availableTime.id = Fridayid;
                    vMs.Insert(5, availableTime);
                }

                if (checkIfSaturday == null)
                {
                    AvailableTime availableTime = new AvailableTime();
                    availableTime.from_time = defaultStart;
                    availableTime.to_time = defaultStart;
                    availableTime.day_of_week = 6;

                    availableTime.week_of_year = weekNumber;
                    availableTime.year = year;
                    availableTime.user_id = user_id;
                    availableTime.vDate = GetWeekFirstDay(6, isNextWeek);
                    availableTime.scheduleDate = GetWeekFirstDay(6, isNextWeek).ToShortDateString();
                    availableTime.userType = 3;
                    availableTime.isCausallOff = false;
                    availableTime.isOptionalOff = true;


                    long Saturdayid = SaveChanges(availableTime);
                    availableTime.id = Saturdayid;
                    vMs.Insert(6, availableTime);
                }
            }
            catch (Exception)
            {
                vMs = new List<AvailableTime>();
            }
            return vMs;
        }

        private long SaveChanges(AvailableTime availableTime)
        {
            var vtime = _db.tblProviderAvailableTimes.Add(new tblProviderAvailableTime
            {
                day_of_week = availableTime.day_of_week,
                created_date = DateTime.Now,
                from_time = availableTime.from_time,
                to_time = availableTime.to_time,
                isCausallOff = availableTime.isCausallOff,
                isOptionalOff = availableTime.isOptionalOff,
                week_of_year = availableTime.week_of_year,
                year = availableTime.year,
                IsVisible = true,
                provider_id = availableTime.user_id,
                vDate = availableTime.vDate
            });

            _db.SaveChanges();

            return vtime.Id;
        }

        public AvailableTime AddAvailableTimeInProvider(AvailableTime model)
        {
            AvailableTime rtmodel = new AvailableTime();
            try
            {
                if (model.vDate == null) return null;

                var schedule = _db.tblProviderAvailableTimes.Where(a => a.Id == model.id).FirstOrDefault();

                if (schedule == null)
                {
                    schedule = new tblProviderAvailableTime();
                }
                // day_of_week
                // week_of_year
                schedule.vDate = model.vDate;
                schedule.day_of_week = model.vDate.Value.DayOfWeek.GetHashCode();
                schedule.week_of_year = GetWeekNumber((DateTime)model.vDate);
                schedule.year = model.vDate.Value.Year;
                schedule.from_time = model.from_time;
                schedule.to_time = model.to_time;
                schedule.provider_id = model.user_id;

                schedule.isCausallOff = model.isCausallOff;
                schedule.isOptionalOff = model.isOptionalOff;
                schedule.IsVisible = true;

                if (model.isCausallOff || model.isOptionalOff)
                {
                    schedule.from_time = "08:00";
                    schedule.to_time = "08:00";
                }
                if(model.id == null)
                {
                    // save the record
                    _db.tblProviderAvailableTimes.Add(schedule);
                }
                _db.SaveChanges();
                rtmodel = new AvailableTime
                {
                    id = schedule.Id,
                    day_of_week = schedule.day_of_week,
                    vDate = schedule.vDate,
                    from_time = schedule.from_time,
                    to_time = schedule.to_time,
                    user_id = schedule.provider_id.Value,
                    isCausallOff = schedule.isCausallOff.Value,
                    isOptionalOff = schedule.isOptionalOff.Value,
                    week_of_year = schedule.week_of_year.Value,
                    year = schedule.year.Value
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return rtmodel;
        }
        private int GetWeekNumber(DateTime vDate)
        {

            // Gets the Calendar instance associated with a CultureInfo.
            CultureInfo myCI = new CultureInfo("he-IL");
            Calendar myCal = myCI.Calendar;

            // Gets the DTFI properties required by GetWeekOfYear.
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            return myCal.GetWeekOfYear(vDate, myCWR, myFirstDOW);
        }
        private DateTime GetWeekFirstDay(int pdays = 0, bool bIsnextWeek = false)
        {
            DayOfWeek day = DateTime.Now.DayOfWeek;
            int days = day - DayOfWeek.Sunday;
            DateTime start = DateTime.Now.AddDays(-days);
            start = start.AddDays(pdays);
            if (bIsnextWeek) start = start.AddDays(7);
            return start;
        }
    }
}