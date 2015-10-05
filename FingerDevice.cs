using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zkemkeeper;

namespace FingerprintSync
{
    class FingerDevice
    {
        public FingerDevice()
        {

        }

        private List<CZKEMClass> fingerDeviceList;
        public List<CZKEMClass> FingerDeviceList
        {
            get
            {
                return fingerDeviceList;
            }
            set
            {
                fingerDeviceList = value;
            }
        }

        public void InsertFingerDevice(CZKEMClass FingerDevice)
        {
            fingerDeviceList.Add(FingerDevice);
        }
    }

    
}
