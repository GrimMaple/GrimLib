using System;
using System.Collections.Generic;
using System.Text;

namespace GrimLib.Configuration
{
    enum ParseState
    {
        None,
        Name,
        PreDots,
        Dots,
        Value,
        AfterValue
    }
}
