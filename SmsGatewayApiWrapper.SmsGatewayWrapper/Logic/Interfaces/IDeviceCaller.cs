using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmsGatewayApiWrapper.SmsGatewayWrapper.Models;

namespace SmsGatewayApiWrapper.SmsGatewayWrapper {
    public interface IDeviceCaller {
        Device GetLastSeenDevice();
        Task<Device> GetLastSeenDeviceAsync();

        PaginingList<Device> GetDevices(int page = 1);
        Task<PaginingList<Device>> GetDevicesAsync(int page = 1);

        Device GetDevice(int id);
        Task<Device> GetDeviceAsync(int id);




    }
}
