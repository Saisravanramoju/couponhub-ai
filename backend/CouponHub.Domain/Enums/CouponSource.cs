using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouponHub.Domain.Enums;

public enum CouponSource
{
    Manual = 1,
    Screenshot = 2,
    OCR = 3,
    Notification = 4,
    Email = 5,
    AI = 6
}