using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouponHub.Domain.Enums;

public enum DiscountType
{
    Percentage = 1,
    Flat = 2,
    Cashback = 3,
    FreeDelivery = 4,
    BuyOneGetOne = 5,
    Other = 6
}
