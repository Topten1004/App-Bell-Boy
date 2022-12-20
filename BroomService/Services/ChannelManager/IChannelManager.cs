using BroomService.Models;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroomService.Services.ChannelManager
{
    interface IChannelManager
    {
        Task<bool> Activate(long userId, UserChannelManagerViewModel userChannelManager);

        bool AutoOrderServices(Property property);

        Task<bool> ImportPropertyByApartmentId(long userId, long apartmentId);

        Task<List<ChannelManagerAccomodationViewModel>> Accomodations(string apiKey, bool onlyAvailable);
    }
}
